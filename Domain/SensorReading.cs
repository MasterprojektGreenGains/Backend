using Microsoft.EntityFrameworkCore;
using NodaTime;
using System.ComponentModel.DataAnnotations.Schema;

namespace GreenGainsBackend.Domain;

[Keyless]
[Table("sensorreadings")]
public class SensorReading
{
    public string? Topic { get; set; }
    public Instant Time { get; set; }
    public string? Uptime { get; set; }
    public Instant Timestamp { get; set; }
    public OBISCode Code { get; set; }
    public double Value { get; set; }

    public SensorReading()
    {

    }

    public override string ToString()
    {
        var output = $"Topic: \t\t{Topic}\nTime: \t\t{Time}\nUptime: \t{Uptime}\nTimestamp: \t{Timestamp}\n";
        output += $"Code and Value: \n";
        output += $"{Code}:\t{Value}\n";

        return output;
    }
}