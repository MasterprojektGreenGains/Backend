using GreenGainsBackend.Domain;
using GreenGainsBackend.Models;
using Newtonsoft.Json.Linq;

public class MeterReadingParser
{
    public MeterReading ParseSensorData(string json)
    {
        JObject parsedData = JObject.Parse(json);

        var message = new MeterReading
        {
            Topic = parsedData["topic"]?.ToString(),
            Time = DateTime.Parse(parsedData["time"]?.ToString() ?? ""),
            Uptime = parsedData["message"]?["uptime"]?.ToString(),
            Timestamp = DateTime.Parse(parsedData["message"]?["timestamp"]?.ToString() ?? "")
        };

        var messageData = parsedData["message"] as JObject;

        messageData.Remove("uptime");
        messageData.Remove("timestamp");

        foreach (var data in messageData)
        {
            var key = data.Key;
            double value = (double)data.Value;

            switch (key)
            {
                case "1.7.0":
                    message.Code = OBISCode.Code_1_7_0;
                    message.Value = value;
                    break;
                case "1.8.0":
                    message.Code = OBISCode.Code_1_8_0;
                    message.Value = value;
                    break;
                case "2.7.0":
                    message.Code = OBISCode.Code_2_7_0;
                    message.Value = value;
                    break;
                case "2.8.0":
                    message.Code = OBISCode.Code_2_8_0;
                    message.Value = value;
                    break;
                case "3.8.0":
                    message.Code = OBISCode.Code_3_8_0;
                    message.Value = value;
                    break;
                case "4.8.0":
                    message.Code = OBISCode.Code_4_8_0;
                    message.Value = value;
                    break;
                case "16.7.0":
                    message.Code = OBISCode.Code_16_7_0;
                    message.Value = value;
                    break;
                case "31.7.0":
                    message.Code = OBISCode.Code_31_7_0;
                    message.Value = value;
                    break;
                case "32.7.0":
                    message.Code = OBISCode.Code_32_7_0;
                    message.Value = value;
                    break;
                case "51.7.0":
                    message.Code = OBISCode.Code_51_7_0;
                    message.Value = value;
                    break;
                case "52.7.0":
                    message.Code = OBISCode.Code_52_7_0;
                    message.Value = value;
                    break;
                case "71.7.0":
                    message.Code = OBISCode.Code_71_7_0;
                    message.Value = value;
                    break;
                case "72.7.0":
                    message.Code = OBISCode.Code_72_7_0;
                    message.Value = value;
                    break;
            }
        }
        return message;
    }
}