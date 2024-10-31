using Newtonsoft.Json.Linq;

namespace GreenGainsBackend.Domain.Helper;

public class SensorReadingParser
{
    public List<SensorReading> ParseSensorData(string json)
    {
        List<SensorReading> sensorReadings = new List<SensorReading>();
        JObject parsedData = JObject.Parse(json);

        var messageData = parsedData["message"] as JObject;

        if (messageData == null)
            throw new Exception("Invalid JSON data");

        foreach (var data in messageData)
        {

            var message = new SensorReading
            {
                Topic = parsedData["topic"]?.ToString(),
                Time = DateTime.Parse(parsedData["time"]?.ToString() ?? ""),
                Uptime = parsedData["message"]?["uptime"]?.ToString(),
                Timestamp = DateTime.Parse(parsedData["message"]?["timestamp"]?.ToString() ?? "")
            };

            var key = data.Key;

            if (data.Value == null) continue;

            switch (key)
            {
                case "1.7.0":
                    message.Code = OBISCode.code_1_7_0;
                    message.Value = (double)data.Value;
                    sensorReadings.Add(message);
                    break;
                case "1.8.0":
                    message.Code = OBISCode.code_1_8_0;
                    message.Value = (double)data.Value;
                    sensorReadings.Add(message);
                    break;
                case "2.7.0":
                    message.Code = OBISCode.code_2_7_0;
                    message.Value = (double)data.Value;
                    sensorReadings.Add(message);

                    break;
                case "2.8.0":
                    message.Code = OBISCode.code_2_8_0;
                    message.Value = (double)data.Value;
                    sensorReadings.Add(message);

                    break;
                case "3.8.0":
                    message.Code = OBISCode.code_3_8_0;
                    message.Value = (double)data.Value;
                    sensorReadings.Add(message);
                    break;
                case "4.8.0":
                    message.Code = OBISCode.code_4_8_0;
                    message.Value = (double)data.Value;
                    sensorReadings.Add(message);

                    break;
                case "16.7.0":
                    message.Code = OBISCode.code_16_7_0;
                    message.Value = (double)data.Value;
                    sensorReadings.Add(message);
                    break;
                case "31.7.0":
                    message.Code = OBISCode.code_31_7_0;
                    message.Value = (double)data.Value;
                    sensorReadings.Add(message);
                    break;
                case "32.7.0":
                    message.Code = OBISCode.code_32_7_0;
                    message.Value = (double)data.Value;
                    sensorReadings.Add(message);
                    break;
                case "51.7.0":
                    message.Code = OBISCode.code_51_7_0;
                    message.Value = (double)data.Value;
                    sensorReadings.Add(message);
                    break;
                case "52.7.0":
                    message.Code = OBISCode.code_52_7_0;
                    message.Value = (double)data.Value;
                    sensorReadings.Add(message);
                    break;
                case "71.7.0":
                    message.Code = OBISCode.code_71_7_0;
                    message.Value = (double)data.Value;
                    sensorReadings.Add(message);
                    break;
                case "72.7.0":
                    message.Code = OBISCode.code_72_7_0;
                    message.Value = (double)data.Value;
                    sensorReadings.Add(message);
                    break;
                default:
                    break;
            }
        }

        return sensorReadings;
    }
}