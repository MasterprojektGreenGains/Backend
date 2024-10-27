using GreenGainsBackend.Domain;
using GreenGainsBackend.Domain.Helper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace GreenGainsBackend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TestController : ControllerBase
{

    private readonly MeterDataDbContext _context;
    private readonly IConfiguration _configuration;

    public TestController(MeterDataDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [HttpGet("/test")]
    public async Task<ActionResult<IEnumerable<SensorReading>>> Get()
    {
        return await _context.SensorReadings.ToListAsync();
    }

    [HttpPost("sensor/data")]
    public async Task<ActionResult> PostSensorData()
    {
        var sensorData = "";

        using (var reader = new StreamReader(Request.Body))
            sensorData = await reader.ReadToEndAsync();

        SensorReadingParser parser = new SensorReadingParser();

        IEnumerable<SensorReading> sensorReadings = parser.ParseSensorData(sensorData);

        if (sensorReadings.Count() == 0)
        {
            return BadRequest();
        }

        var schemaExists = await CheckIfSchemaExists(sensorReadings.First().Topic);

        if (!schemaExists)
        {
            await CreateSchemaAndTableForDevice(sensorReadings.First().Topic);
        }

        await using var connection = new NpgsqlConnection(_configuration.GetConnectionString("GreenGainsDb"));
        await connection.OpenAsync();
        var transaction = await connection.BeginTransactionAsync();

        foreach (var reading in sensorReadings)
        {
            var schemeName = await GetSchemaNameFromDeviceTopic(reading.Topic);

            await using var cmd = new NpgsqlCommand($"INSERT INTO {schemeName}.sensorreadings(\"Topic\", \"Time\", \"Uptime\", \"Timestamp\", \"Code\", \"Value\") VALUES ($1, $2, $3, $4, $5, $6);", connection, transaction)
            {
                Parameters =
                {
                    new() { Value = reading.Topic       },
                    new() { Value = reading.Time        },
                    new() { Value = reading.Uptime      },
                    new() { Value = reading.Timestamp   },
                    new() { Value = reading.Code        },
                    new() { Value = reading.Value       }
                }
            };

            await cmd.ExecuteNonQueryAsync();
        }

        await transaction.CommitAsync();
        await connection.CloseAsync();
        await _context.SaveChangesAsync();

        return Created();
    }

    private async Task<bool> CheckIfSchemaExists(string deviceTopic)
    {
        var schemaName = await GetSchemaNameFromDeviceTopic(deviceTopic);

        await using var connection = new NpgsqlConnection(_configuration.GetConnectionString("GreenGainsDb"));
        await connection.OpenAsync();

        await using var cmd = new NpgsqlCommand($"SELECT EXISTS(SELECT 1 FROM information_schema.schemata WHERE schema_name = '{schemaName}');", connection);

        var result = (bool)await cmd.ExecuteScalarAsync();

        await connection.CloseAsync();

        return result;

    }

    private async Task CreateSchemaAndTableForDevice(string deviceTopic)
    {
        var schemaName = await GetSchemaNameFromDeviceTopic(deviceTopic);
        var createSchemaQuery = $"CREATE SCHEMA IF NOT EXISTS {schemaName};";
        var createTableQuery = $"CREATE TABLE" +
                                $"   IF NOT EXISTS" +
                                $"       {schemaName}.sensorreadings" +
                                $"       (\"Topic\" TEXT, \"Time\" TIMESTAMP, \"Uptime\" TEXT, \"Timestamp\" TIMESTAMP, \"Code\" TEXT, \"Value\" DOUBLE PRECISION);" +
                                $"SELECT" +
                                $"   create_hypertable('{schemaName}.sensorreadings', 'Timestamp');";


        await using var connection = new NpgsqlConnection(_configuration.GetConnectionString("GreenGainsDb"));

        await using var createSchemaCmd = new NpgsqlCommand(createSchemaQuery, connection);
        await using var createTableCmd = new NpgsqlCommand(createTableQuery, connection);

        await connection.OpenAsync();
        await createSchemaCmd.ExecuteNonQueryAsync();
        await createTableCmd.ExecuteNonQueryAsync();
        await connection.CloseAsync();

        return;
    }

    private async Task<string> GetSchemaNameFromDeviceTopic(string deviceTopic)
    {
        var deviceTopicFormatted = deviceTopic.Replace("/", "_").ToLower();

        return $"schema_{deviceTopicFormatted}";
    }

}