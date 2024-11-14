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

    [HttpGet("generateData")]
    public async Task<ActionResult> GenerateSensorData(int rowsToGenerate = 2000000, int initialPower = 500, int powerVariance = 50)
    {
        var schemaName = GetSchemaNameFromDeviceTopic("generated/sensor");

        await using var connection = new NpgsqlConnection(_configuration.GetConnectionString("GreenGainsDb"));
        await connection.OpenAsync();

        await using var cmd = new NpgsqlCommand(
            $"""
                -- drop schema if it exists
                DROP SCHEMA IF EXISTS schema_generated_sensor CASCADE;

                -- Create schema if it doesn't exist
                CREATE SCHEMA IF NOT EXISTS schema_generated_sensor;

                -- Create table if it doesn't exist
                CREATE TABLE IF NOT EXISTS greengains.schema_generated_sensor.sensorreadings (
                    Topic TEXT NOT NULL,
                    Time TIMESTAMPTZ NOT NULL,
                    Uptime TEXT NOT NULL,      -- Fixed text value for Uptime
                    Timestamp TIMESTAMPTZ NOT NULL,
                    Code TEXT NOT NULL,
                    Value DECIMAL NOT NULL
                );

                DO $$
                DECLARE
                    rows_to_generate INTEGER := {rowsToGenerate};          -- Number of rows to generate
                    initial_power DECIMAL := {initialPower};              -- Base power value for realistic apparent power
                    power_variance DECIMAL := {powerVariance};              -- Fluctuation range for power values
                BEGIN
                    WITH RECURSIVE sensor_data AS (
                        SELECT
                            'generated_sensor'::TEXT AS Topic,
                            NOW() AS Time,
                            '0000:00:00:00'::TEXT AS Uptime,    -- Fixed text value for Uptime
                            NOW() AS Timestamp,
                            'code_16_7_0'::TEXT AS Code,
                            CEIL(initial_power + (RANDOM() - 0.5) * 2 * power_variance)::DECIMAL AS Value,
                            1 AS row_num
                        UNION ALL
                        SELECT
                            'generated_sensor'::TEXT,
                            Time - INTERVAL '60 seconds',
                            '0000:00:00:00'::TEXT,              -- Fixed text value for Uptime
                            Timestamp - INTERVAL '60 seconds',
                            'code_16_7_0'::TEXT,
                            CEIL(initial_power + (RANDOM() - 0.5) * 2 * power_variance)::DECIMAL,
                            row_num + 1
                        FROM sensor_data
                        WHERE row_num < rows_to_generate
                    )
                    INSERT INTO greengains.schema_generated_sensor.sensorreadings (Topic, Time, Uptime, Timestamp, Code, Value)
                    SELECT Topic, Time, Uptime, Timestamp, Code, Value
                    FROM sensor_data;
                END $$;

                -- get all unique timestamps and group them by day and show count of how many readings were taken on that day
                SELECT
                    DATE_TRUNC('day', timestamp) AS day,
                    COUNT(*) AS readings
                FROM greengains.schema_generated_sensor.sensorreadings
                GROUP BY day
                ORDER BY day DESC;
                """
            , connection);

        await cmd.ExecuteNonQueryAsync();

        await connection.CloseAsync();

        return Ok();
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