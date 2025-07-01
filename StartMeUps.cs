using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDF_Llama;

public interface IStartMeUps
{
    public Uri ModelEndpoint { get; set; }
    public string ModelName { get; set; }
}

public class StartMeUps
{
    public required Uri ModelEndpoint { get; set; }
    public required string ModelName { get; set; }

    public static float ConvertWordToFloat(string word)
    {
        // Simple example: convert each character to its ASCII value and sum them up
        float sum = 0;
        foreach (var ch in word)
        {
            sum += ch;
        }
        return sum;
    }
}

/*
         //TODO: finish this
        public static async Task StoreJSONAsync(string storeMe)
        {
            string jsonString = JsonSerializer.Serialize(storeMe);
            string fileName = "WeatherForecast.json";
            await using FileStream createStream = File.Create(fileName);
            await JsonSerializer.SerializeAsync(createStream, jsonString);

            Console.WriteLine(File.ReadAllText(fileName));
        }
 */