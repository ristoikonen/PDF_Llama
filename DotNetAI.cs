using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using OllamaSharp;
using OllamaSharp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDF_Llama;

public sealed class DotNetAI
{

    public Uri ModelEndpoint { get; set; }
    public string ModelName { get; set; }

    public DotNetAI(Uri modelEndpoint, string modelName)
    {
        this.ModelEndpoint = modelEndpoint;
        this.ModelName = modelName;
    }

    public async Task GetResponse(string question = @"Hello!")
    {
        // --- Configuration ---
        const string ollamaEndpoint = "http://localhost:11434";
        const string ollamaModel = "llama3.2";

        Console.WriteLine("Setting up OllamaChatClient...");

        try
        {
            IChatClient client = new OllamaChatClient(new Uri(ollamaEndpoint), ollamaModel);
            var response = await client.GetResponseAsync(question);
            var txt = response?.Text;
            Console.WriteLine($"Response: {txt}");

        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            Console.WriteLine("Please ensure Ollama is running and the specified model is downloaded.");
            Console.WriteLine($"Check your Ollama endpoint: {ollamaEndpoint} and model: {ollamaModel}");
        }

        Console.WriteLine("Press any key to exit.");

    }






}
