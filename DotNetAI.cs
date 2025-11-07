using Microsoft.Extensions.AI;
//using Microsoft.Extensions.AI.Ollama;
//using OllamaSharp;
//using OllamaSharp.Models;
using OpenAI.Images;

namespace PDF_Llama;

public sealed class DotNetAI
{
    // comfyui 
    public Uri ModelEndpoint { get; set; }
    public string ModelName { get; set; }

    public DotNetAI(Uri modelEndpoint, string modelName)
    {
        this.ModelEndpoint = modelEndpoint;
        this.ModelName = modelName;
    }

    public async Task GetResponse(string question = @"Describe your model")
    {
        // --- Configuration ---
        const string ollamaEndpoint = "http://localhost:11434";
        const string ollamaModel = "llama3.2";

        Console.WriteLine("Setting up OllamaChatClient...");

        //Microsoft.Extensions.AI.Ollama.OllamaChatClient c = new Microsoft.Extensions.AI.Ollama.OllamaChatClient(
        //    new Uri(ollamaEndpoint), ollamaModel);

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

    public async Task CreateImage(string question = @"Hello!")
    {
        // --- Configuration ---
        const string ollamaEndpoint = "http://localhost:11434";
        const string ollamaModel = "llama3.2";

        Console.WriteLine("Setting up OllamaChatClient...");

        try
        {
            ImageClient client = new(ollamaModel,"");

            GeneratedImage generatedImage = await client.GenerateImageAsync("""
                A postal card with a happy hiker waving and a beautiful mountain in the background.
                There is a trail visible in the foreground.
                The postal card has text in red saying: 'You are invited for a hike!'
                """,
                new ImageGenerationOptions
                {
                    Size = GeneratedImageSize.W1024xH1024
                });


            Console.WriteLine($"Response: {ollamaModel}");

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
