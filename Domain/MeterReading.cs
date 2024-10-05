using GreenGainsBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace GreenGainsBackend.Domain;

[Keyless]
public class MeterReading
{
    public string? Topic { get; set; }
    public DateTime Time { get; set; }
    public string? Uptime { get; set; }
    public DateTime Timestamp { get; set; }

    public Dictionary<OBISCode, double> SensorData { get; set; }

    public MeterReading()
    {
        SensorData = new Dictionary<OBISCode, double>();
    }

    public void AddSensorData(OBISCode code, double value)
    {
        SensorData.Add(code, value);
    }

    public override string ToString()
    {
        var output = $"Topic: \t\t{Topic}\nTime: \t\t{Time}\nUptime: \t{Uptime}\nTimestamp: \t{Timestamp}\n";
        output += $"SensorData: \n";
        foreach (var data in SensorData)
        {
            output += $"\t{data.Key}:\t{data.Value}\n";
        }

        return output;
    }
}