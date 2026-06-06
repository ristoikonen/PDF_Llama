using Agent_Ollama.Models;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Agent_Ollama.Agents;

internal sealed class UserInfo
{
    public string? UserName { get; set; }
    public int? UserAge { get; set; }
}

internal class TrafficAgent
{
    //TODO: logging
    private Uri ModelEndpoint { get; set; }
    private string ModelName { get; set; }

    const string AgentName = "Roadie";
    public TrafficAgent(string ollamaModel, Uri ollamaEndpoint)
    {
        this.ModelName = ollamaModel;
        this.ModelEndpoint = ollamaEndpoint;
    }
    
    // https://devblogs.microsoft.com/dotnet/microsoft-agent-framework-building-blocks-for-ai-part-3/

    public async Task<List<Road>> RushHour(string city)
    {
        try
        {
            IChatClient client = new OllamaChatClient(ModelEndpoint, ModelName);

            AIAgent agent = client.AsAIAgent(
                instructions: "You are a helpful traffic assistant running locally via Ollama.",
                name: AgentName
                );

            string question = "Which roads have heavy traffic, inform about rush hour traffic times and directions, in " + (city ?? "Sydney");

            AgentSession session = await agent.CreateSessionAsync();
            
            AgentResponse<List<Road>> structuredResponse = await agent.RunAsync<List<Road>>(question, session);
            
            List<Road> roads = structuredResponse?.Result ?? new List<Road>();

            var recommendeations = await agent.RunAsync(
                "Now recommend how to get around these busiest roads.",
                session);

            Console.WriteLine(recommendeations);

            // JsonElement to file for inspection
            /*
            var serialized = await agent.SerializeSessionAsync(session);

            using FileStream createStream = File.Create(@"c:\tmp\output.json");
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            await JsonSerializer.SerializeAsync(createStream, serialized, options);
            var deserializedSession = await agent.DeserializeSessionAsync(serialized);
            */

            foreach (var road in roads)
            {
                Console.WriteLine(road.ToString() + Environment.NewLine);
            }
            
            return roads;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ensure Ollama is running, check your Ollama endpoint: {this.ModelEndpoint} and model: {this.ModelName}");
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
        return new List<Road>();
        //Console.WriteLine("Press any key to exit.");
    }
}
