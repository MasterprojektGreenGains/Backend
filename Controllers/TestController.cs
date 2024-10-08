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

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SensorReading>>> Get()
    {
        return await _context.SensorReadings.ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<SensorReading>> postSensorData()
    {
        var sensorData = "";

        using (var reader = new StreamReader(Request.Body))
            sensorData = await reader.ReadToEndAsync();

        SensorReadingParser parser = new SensorReadingParser();

        IEnumerable<SensorReading> sensorReadings = parser.ParseSensorData(sensorData);

        foreach (var reading in sensorReadings)
        {
            await using var connection = new NpgsqlConnection(_configuration.GetConnectionString("GreenGainsDb"));
            await connection.OpenAsync();

            await using var cmd = new NpgsqlCommand("INSERT INTO sensorreadings(\"Topic\", \"Time\", \"Uptime\", \"Timestamp\", \"Code\", \"Value\") VALUES ($1, $2, $3, $4, $5, $6);", connection)
            {
                Parameters =
                {
                    new() { Value = reading.Topic},
                    new() { Value = reading.Time},
                    new() { Value = reading.Uptime},
                    new() { Value = reading.Timestamp},
                    new() { Value = reading.Code},
                    new() { Value = reading.Value}
                }
            };

            await cmd.ExecuteNonQueryAsync();
            await connection.CloseAsync();
        }

        await _context.SaveChangesAsync();


        return Created();
    }
}