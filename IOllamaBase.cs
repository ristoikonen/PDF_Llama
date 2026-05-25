using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using OllamaSharp;
using OpenAI.Models;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;
//using static
//.AgentStructuredOutput;
//using static System.Net.Mime.MediaTypeNames;


#pragma warning disable CA1861 // Avoid constant arrays as arguments
#pragma warning disable SKEXP0070 // AddOllamaTextGeneration

namespace Agent_Ollama;

public interface IOllamaBase
{
    public Uri ModelEndpoint { get; set; }
    public string ModelName { get; set; }
    //public OllamaBase(string modelEndpoint, string modelName)
    //{
    //    ModelEndpoint = new Uri(modelEndpoint);
    //    ModelName = modelName;
    //}
}


public class Helper<T> : IOllamaBase where T : class
{
    public Helper(string modelEndpoint, string modelName)
    {
        ModelEndpoint = new Uri(modelEndpoint);
        ModelName = modelName;
    }

    public Uri ModelEndpoint { get; set; }
    public string ModelName { get; set; }

    public async Task<T?> GetAPIData(Uri ApiEndpoint)
    {
        try
        {
            var httpClient = new HttpClient
            {
                //BaseAddress = new Uri("http://localhost:11434"),
                Timeout = TimeSpan.FromMinutes(10) // 10-minute timeout
            };

            //T? t = null;
            //var options = new ChatOptions
            //{
            //    ResponseFormat = ChatResponseFormat.ForJsonSchema<PlantInfo[]>()
            //};


            var url = $"https://api.coinlore.net/api/ticker/?id=90";

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var content = await httpClient.GetStringAsync(ApiEndpoint.AbsoluteUri.ToString());

            //var result = await httpClient.GetFromJsonAsync<PriceResult>(url);

            dynamic? obj = JsonSerializer.Deserialize<dynamic>(content);

            var list = JsonSerializer.Deserialize<List<JsonElement>>(content);
            foreach (var element in list!)
            {
                return JsonSerializer.Deserialize<T>(element);
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            Console.WriteLine("Please ensure Ollama is running and the specified model is downloaded.");
            Console.WriteLine($"Check your Ollama endpoint: {ModelEndpoint.AbsoluteUri} and model: {ModelName}");
        }

        return default(T);
    }
}

//  https://stackoverflow.com/questions/12543094/nested-dictionary-collection-in-net
//  Source - https://stackoverflow.com/a/12543547
//  Posted by Matthew Layton

public class NestedDictionary<K, V> : Dictionary<K, NestedDictionary<K, V>> where K : notnull
{
    public V? Value { set; get; }

    public new NestedDictionary<K, V> this[K key]
    {
        set { base[key] = value; }

        get
        {
            if (!base.Keys.Contains<K>(key))
            {
                base[key] = new NestedDictionary<K, V>();
            }
            return base[key];
        }
    }
}


