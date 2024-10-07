using GreenGainsBackend.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace GreenGainsBackend.Domain;

[Keyless]
[Table("meterreadings")]
public class MeterReading
{
    public string? Topic { get; set; }
    public DateTime Time { get; set; }
    public string? Uptime { get; set; }
    public DateTime Timestamp { get; set; }
    public OBISCode Code { get; set; }
    public double Value { get; set; }

    public MeterReading()
    {

    }

    public override string ToString()
    {
        var output = $"Topic: \t\t{Topic}\nTime: \t\t{Time}\nUptime: \t{Uptime}\nTimestamp: \t{Timestamp}\n";
        output += $"SensorData: \n";
        output += $"\t{Code}:\t{Value}\n";

        return output;
    }
}