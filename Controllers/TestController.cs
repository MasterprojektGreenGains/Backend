using GreenGainsBackend.Domain;
using GreenGainsBackend.Domain.Helper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Diagnostics;

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

    [HttpGet("seedDB")]
    public async Task<IActionResult> SeedDBFromJSON()
    {
        var watch = new Stopwatch();

        watch.Start();

        var filePath = "./mqtt_messages_2.json";

        if (!System.IO.File.Exists(filePath))
            return NotFound("File not found");

        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        using var reader = new StreamReader(stream);
        var sensorData = await reader.ReadToEndAsync();

        Console.WriteLine("Read JSON File");

        SensorReadingParser parser = new SensorReadingParser();
        List<SensorReading> sensorReadings = parser.ParseTestSensorData(sensorData);

        Console.WriteLine("Parsed JSON File");

        if (!sensorReadings.Any())
            return BadRequest("No sensor readings found in file");

        var deviceTopic = "test_sensor";

        var schemaExists = await CheckIfSchemaExists(deviceTopic);

        if (!schemaExists)
            await CreateSchemaAndTableForDevice(deviceTopic);

        await using var connection = new NpgsqlConnection(_configuration.GetConnectionString("GreenGainsDb"));
        await connection.OpenAsync();

        int i = 0;
        int max = sensorReadings.Count();

        foreach (var sensorReading in sensorReadings)
        {
            var schemaName = GetSchemaNameFromDeviceTopic(deviceTopic);

            sensorReading.Time = sensorReading.Time.AddYears(1);
            sensorReading.Timestamp = sensorReading.Timestamp.AddYears(1);

            await using var cmd = new NpgsqlCommand($"INSERT INTO {schemaName}.sensorreadings(\"Topic\", \"Time\", \"Uptime\", \"Timestamp\", \"Code\", \"Value\") VALUES ($1, $2, $3, $4, $5, $6);",
                                                    connection)
            {
                Parameters =
                {
                    new() { Value = sensorReading.Topic       },
                    new() { Value = sensorReading.Time        },
                    new() { Value = sensorReading.Uptime      },
                    new() { Value = sensorReading.Timestamp   },
                    new() { Value = sensorReading.Code        },
                    new() { Value = sensorReading.Value       }
                }
            };

            await cmd.ExecuteNonQueryAsync();

            Console.Clear();
            Console.WriteLine("-------------------------------------");
            Console.WriteLine("Added Number\t" + i);
            Console.WriteLine("From max\t " + max);
            Console.WriteLine("-------------------------------------");
            i++;
        }

        await connection.CloseAsync();
        await _context.SaveChangesAsync();

        watch.Stop();

        TimeSpan ts = watch.Elapsed;

        string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);

        Console.WriteLine("elapsed Time: " + elapsedTime);

        return Ok("Database seeded successfully!");
    }

    [HttpGet("sensor/data")]
    public async Task<ActionResult<IEnumerable<SensorReading>>> GetSensorData(string deviceTopic, string interval)
    {
        if (string.IsNullOrEmpty(interval))
            return BadRequest("Interval is required");

        if (string.IsNullOrEmpty(deviceTopic))
            return BadRequest("Device topic is required");

        var schemaName = GetSchemaNameFromDeviceTopic(deviceTopic);

        await using var connection = new NpgsqlConnection(_configuration.GetConnectionString("GreenGainsDb"));
        await connection.OpenAsync();

        await using var cmd = interval switch
        {
            "hour" => new NpgsqlCommand($"SELECT *" +
                                        $"  FROM {schemaName}.sensorreadings" +
                                        $"      WHERE \"Timestamp\" >= NOW() - INTERVAL '1 hour';"),
            "day" => new NpgsqlCommand($"SELECT *" +
                                        $"  FROM {schemaName}.sensorreadings" +
                                        $"      WHERE \"Timestamp\" >= NOW() - INTERVAL '24 hours';"),
            "week" => new NpgsqlCommand($"SELECT *" +
                                        $"  FROM {schemaName}.sensorreadings" +
                                        $"      WHERE \"Timestamp\" >= NOW() - INTERVAL '7 days';"),
            "month" => new NpgsqlCommand($"SELECT *" +
                                        $"  FROM {schemaName}.sensorreadings" +
                                        $"      WHERE \"Timestamp\" >= NOW() - INTERVAL '1 month';"),
            "" => throw new ArgumentException("Invalid interval"),
            _ => throw new ArgumentException("Invalid interval")
        };

        cmd.Connection = connection;

        await using var reader = await cmd.ExecuteReaderAsync();

        var sensorReadings = new List<SensorReading>();

        while (await reader.ReadAsync())
        {
            sensorReadings.Add(new SensorReading
            {
                Topic = reader.GetString(0),
                Time = reader.GetDateTime(1),
                Uptime = reader.GetString(2),
                Timestamp = reader.GetDateTime(3),
                Code = (OBISCode)Enum.Parse(typeof(OBISCode), reader.GetString(4)),
                Value = reader.GetDouble(5)
            });
        }

        await connection.CloseAsync();

        return sensorReadings;
    }

    [HttpGet("sensor/data/bucket")]
    public async Task<ActionResult<IEnumerable<SensorReadingBucket>>> GetSensorDataBucket(string deviceTopic, string interval)
    {
        if (string.IsNullOrEmpty(interval))
            return BadRequest("Interval is required");

        if (string.IsNullOrEmpty(deviceTopic))
            return BadRequest("Device topic is required");

        var schemaName = GetSchemaNameFromDeviceTopic(deviceTopic);

        await using var connection = new NpgsqlConnection(_configuration.GetConnectionString("GreenGainsDb"));
        await connection.OpenAsync();

        await using var cmd = interval switch
        {
            "hour" => new NpgsqlCommand(
                                        $"""
                                            SELECT time_bucket('5 minutes', "Timestamp") as bucket,
                                                "Code",
                                                AVG("Value") as "Value"
                                            FROM {schemaName}.sensorreadings
                                            WHERE "Timestamp" >= NOW() - INTERVAL '1 hour'
                                            GROUP BY bucket, "Code"
                                            ORDER BY bucket, "Code";
                                        """),

            "day" => new NpgsqlCommand(
                                        $"""
                                            SELECT time_bucket('1 hour', "Timestamp") as bucket,
                                                "Code",
                                                AVG("Value") as "Value"
                                            FROM {schemaName}.sensorreadings
                                            WHERE "Timestamp" >= NOW() - INTERVAL '24 hours'
                                            GROUP BY bucket, "Code"
                                            ORDER BY bucket, "Code";
                                        """),
            "week" => new NpgsqlCommand(
                                        $"""
                                            SELECT time_bucket('1 day', "Timestamp") as bucket,
                                                "Code",
                                                AVG("Value") as "Value"
                                            FROM {schemaName}.sensorreadings
                                            WHERE "Timestamp" >= NOW() - INTERVAL '7 days'
                                            GROUP BY bucket, "Code"
                                            ORDER BY bucket, "Code";
                                        """),
            "month" => new NpgsqlCommand(
                                        $"""
                                            SELECT time_bucket('1 week', "Timestamp") as bucket,
                                                "Code",
                                                AVG("Value") as "Value"
                                            FROM {schemaName}.sensorreadings
                                            WHERE "Timestamp" >= NOW() - INTERVAL '1 month'
                                            GROUP BY bucket, "Code"
                                            ORDER BY bucket, "Code";
                                        """),

            _ => throw new ArgumentException("Invalid interval")
        };

        cmd.Connection = connection;

        await using var reader = await cmd.ExecuteReaderAsync();

        var sensorReadings = new List<SensorReadingBucket>();

        while (await reader.ReadAsync())
        {
            sensorReadings.Add(new SensorReadingBucket
            {
                bucket = reader.GetDateTime(0),
                Code = (OBISCode)Enum.Parse(typeof(OBISCode), reader.GetString(1)),
                Value = reader.GetDouble(2)
            });
        }

        await connection.CloseAsync();

        return sensorReadings;

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

        var deviceTopic = sensorReadings.First().Topic;

        if (string.IsNullOrEmpty(deviceTopic))
        {
            return BadRequest("Device topic is required");
        }

        var schemaExists = await CheckIfSchemaExists(deviceTopic);

        if (!schemaExists)
            await CreateSchemaAndTableForDevice(deviceTopic);

        var schemeName = GetSchemaNameFromDeviceTopic(deviceTopic);

        await using var connection = new NpgsqlConnection(_configuration.GetConnectionString("GreenGainsDb"));
        await connection.OpenAsync();

        foreach (var reading in sensorReadings)
        {

            await using var cmd = new NpgsqlCommand($"INSERT INTO {schemeName}.sensorreadings(\"Topic\", \"Time\", \"Uptime\", \"Timestamp\", \"Code\", \"Value\") VALUES ($1, $2, $3, $4, $5, $6);", connection)
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

        await connection.CloseAsync();
        await _context.SaveChangesAsync();

        return Created();
    }

    private async Task<bool> CheckIfSchemaExists(string deviceTopic)
    {
        var schemaName = GetSchemaNameFromDeviceTopic(deviceTopic);

        await using var connection = new NpgsqlConnection(_configuration.GetConnectionString("GreenGainsDb"));
        await connection.OpenAsync();

        await using var cmd = new NpgsqlCommand($"SELECT EXISTS(SELECT 1 FROM information_schema.schemata WHERE schema_name = '{schemaName}');", connection);

        var result = (bool)await cmd.ExecuteScalarAsync();

        await connection.CloseAsync();

        return result;

    }

    private async Task CreateSchemaAndTableForDevice(string deviceTopic)
    {
        var schemaName = GetSchemaNameFromDeviceTopic(deviceTopic);
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

    private string GetSchemaNameFromDeviceTopic(string deviceTopic)
    {
        var deviceTopicFormatted = deviceTopic.Replace("/", "_").ToLower();

        return $"schema_{deviceTopicFormatted}";
    }

}