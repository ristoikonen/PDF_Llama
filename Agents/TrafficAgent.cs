using Agent_Ollama.Helpers;
using Agent_Ollama.Loggers;
using Agent_Ollama.Models;
using Azure;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using UglyToad.PdfPig.Logging;
using static OllamaSharp.OllamaApiClient;

namespace Agent_Ollama.Agents;

internal sealed class UserInfo
{
    public string? UserName { get; set; }
    public int? UserAge { get; set; }
}

internal class TrafficAgent
{
    TrafficAgentHtmlLogger Logger { get; set; }
    private Uri ModelEndpoint { get; set; }
    private string ModelName { get; set; }
    const string AgentName = "Roadie";

    public TrafficAgent(string ollamaModel, Uri ollamaEndpoint,
        [FromKeyedServices("TrafficAgentHtmlLogger")] ILogger logger)
    {
        this.ModelName = ollamaModel;// configuration["ModelName"] ?? "";
        this.ModelEndpoint = ollamaEndpoint;
        //TrafficAgentHtmlLogger trafficHtmlLogger = new TrafficAgentHtmlLogger("OilPriceAgent", @"c:\tmp");
        this.Logger = (TrafficAgentHtmlLogger)logger;
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

            Logger.LogAgent("TrafficAgent", $"Question: {question}");

            AgentResponse<List<Road>> structuredResponse = await agent.RunAsync<List<Road>>(question, session);
            List<Road> roads = structuredResponse?.Result ?? new List<Road>();

            Logger.LogInfo("heavy roads:");
            
            roads.ForEach(r => Logger.LogInformation(new EventId(1), r.ToString()));

            Logger.LogAgentResponse($"Response: {string.Join(", ", roads.Select(r => r.ToString()))}");

            //string followUp = "Now recommend how to get around these busiest roads.";

            //AgentResponse<List<Road>> structuredResponse2 = await agent.RunAsync<List<Road>>(followUp, session);
            //List<Road> roads2 = structuredResponse2?.Result ?? new List<Road>();

            //Logger.LogInformation("better roads:");
            //roads2.ForEach(r => Logger.LogInformation(new EventId(2), r.ToString()));

            //var recommendeations = await agent.RunAsync("Now recommend how to get around these busiest roads.",
            //    session);
            //foreach (var road in roads2)
            //{
            //    Console.WriteLine(road.ToString() + Environment.NewLine);
            //    Logger.LogInformation(new EventId(2), road.ToString());
            //}
            return roads;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ensure Ollama is running, check your Ollama endpoint: {this.ModelEndpoint} and model: {this.ModelName}");
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
        return new List<Road>();
    }

    public async Task<List<Road>> RoundRushHour(string city)
    {
        try
        {
            IChatClient client = new OllamaChatClient(ModelEndpoint, ModelName);

            AIAgent agent = client.AsAIAgent(
                instructions: "You are a helpful traffic assistant running locally via Ollama.",
                name: AgentName
                );

            string question = "How to get around these busiest roads, in " + (city ?? "Sydney");

            AgentSession session = await agent.CreateSessionAsync();

            AgentResponse<List<Road>> structuredResponse = await agent.RunAsync<List<Road>>(question, session);
            List<Road> roads = structuredResponse?.Result ?? new List<Road>();

            Logger.LogInformation("around roads:");
            roads.ForEach(r => Logger.LogInformation(new EventId(2), r.ToString()));


            //string followUp = "Now recommend how to get around these busiest roads.";

            //AgentResponse<List<Road>> structuredResponse2 = await agent.RunAsync<List<Road>>(followUp, session);
            //List<Road> roads2 = structuredResponse2?.Result ?? new List<Road>();

            //Logger.LogInformation("better roads:");
            //roads2.ForEach(r => Logger.LogInformation(new EventId(2), r.ToString()));

            //var recommendeations = await agent.RunAsync("Now recommend how to get around these busiest roads.",
            //    session);
            //foreach (var road in roads2)
            //{
            //    Console.WriteLine(road.ToString() + Environment.NewLine);
            //    Logger.LogInformation(new EventId(2), road.ToString());
            //}
            return roads;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ensure Ollama is running, check your Ollama endpoint: {this.ModelEndpoint} and model: {this.ModelName}");
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
        return new List<Road>();
    }

}
