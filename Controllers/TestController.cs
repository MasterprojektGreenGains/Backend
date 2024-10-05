using GreenGainsBackend.Domain;
using Microsoft.AspNetCore.Mvc;

namespace GreenGainsBackend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TestController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok("Hello World");
    }

    [HttpPost]
    public async Task<IActionResult> SensorData()
    {
        var sensorData = "";

        using (var reader = new StreamReader(Request.Body))
            sensorData = await reader.ReadToEndAsync();

        MeterReadingParser parser = new MeterReadingParser();
        MeterReading meterReading = parser.ParseSensorData(sensorData);

        Console.WriteLine(meterReading.ToString());

        return Ok("received");
    }
}