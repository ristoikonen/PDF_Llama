using MemoryPack;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Ollama;
using Microsoft.SemanticKernel.Embeddings;
using OllamaSharp;
using OllamaSharp.Models;
using OpenAI.Chat;
using PdfReader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using UglyToad.PdfPig.Graphics;
using Agent_Ollama.Plugins;

#pragma warning disable CA1861 // Avoid constant arrays as arguments
#pragma warning disable SKEXP0070 // AddOllamaTextGeneration
#pragma warning disable SKEXP0010

namespace Agent_Ollama;  
    

public sealed class PDF_AI_Summariser : IOllamaBase
{
    public Uri ModelEndpoint { get; set; }
    public string ModelName { get; set; }

    public PDF_AI_Summariser(Uri modelEndpoint, string modelName)
    {
        this.ModelEndpoint = modelEndpoint;
        this.ModelName = modelName;
    }
        
    public async Task SummarizeFileUsingPdfContentPlugin(string PDF_filename = @"C:\Users\OneDrive\Documents\what_evolution_is_not.pdf")
    {
        // --- Configuration ---
        //const string ollamaEndpoint = "http://localhost:11434";
        //const string ollamaModel = "llama3.2";


        // --- Create a sample text file for demonstration ---
        // This ensures there's a file for the plugin to read.
        //await CreateSampleTextFile(PDF_filename);

        // --- Initialize the Semantic Kernel ---
        try
        {
            string PDF_filename_parts = @"C:\tmp\W812 - Parts List.pdf";

            Reader reader_parts = new Reader();
            var pdfpartstxt = reader_parts.ReadPdf(PDF_filename_parts);

            var httpClient = new HttpClient()
            {
                BaseAddress = ModelEndpoint,
                Timeout = TimeSpan.FromMinutes(5)
            };

            var builder = Kernel.CreateBuilder()
                .AddOllamaTextGeneration(
                    
                    modelId: ModelName,
                    httpClient: httpClient 
                );

            var kernel = builder.Build();

            Console.WriteLine($"Kernel initialized with Ollama model: {ModelName} at {ModelEndpoint}");

            // --- Import your custom plugin ---
            // The KernelPluginFactory.CreateFromType<T>() method is used to discover  kernel functions defined within the FileContentPlugin class.
            var pdfContentPlugin = kernel.CreatePluginFromObject(new PdfContentPlugin());
            Console.WriteLine("PdfContentPlugin loaded successfully.");

            var kvcPlugin = kernel.CreatePluginFromObject(new KVCPlugin());
            Console.WriteLine("KVCPlugin loaded successfully.");

            // --- Define the path to the text file ---
            //string filePath = Path.GetFullPath(sampleFileName);
            //Console.WriteLine($"Attempting to summarize file: {filePath}");

            var result_parts = await kernel.InvokeAsync(
                kvcPlugin["GetTuples"],
                new() { ["sourceText"] = pdfpartstxt }
            );


            Console.WriteLine("\n--- Result from Ollama ---");
            Console.WriteLine(result_parts.GetValue<string>());

            var result = await kernel.InvokeAsync(
                pdfContentPlugin["SummarizeFile"],
                new() { ["sourceText"] = PDF_filename }
            );

            Console.WriteLine("\n--- Result from Ollama ---");
            Console.WriteLine(result.GetValue<string>());

            builder.Services.AddOllamaTextGeneration(
                modelId: ModelName,
                endpoint: ModelEndpoint
            );

            //var Input = new List<string> { "your text to embed" };
            Reader reader = new Reader();
            var pdftxt = reader.ReadPdfToList(PDF_filename);

            // Assuming you have an IHttpClientFactory and a properly configured OllamaApiClient
            var ollamaClient = new OllamaApiClient(ModelEndpoint, ModelName);
            var gen = ollamaClient.AsTextEmbeddingGenerationService();
            var embeds  = await gen.GenerateEmbeddingsAsync(pdftxt);

            System.Console.WriteLine($"Generated {embeds.Count} embeddings from Ollama for PDF: {PDF_filename}"   );
            
            // Create the embedding service
            //var embeddingService = OllamaApiClient.AsEmbeddingGenerationService(ollamaClient, "nomic-embed-text");

            // Generate embeddings for a text
            //var text = "This is a sample sentence.";
            //var embedding = await embeddingService.GenerateEmbeddingAsync(text);

            /*
            var embeddingRequest = new EmbedRequest
            {
                 Input = Input,
            };

            var embeddingGenerator = new OllamaApiClient(ModelEndpoint)
                .EmbedAsync(embeddingRequest);
            */

            //OllamaApiClientExtensions
            //    .AddOllamaTextEmbeddingGeneration(
            //        builder.Services,
                    
            //        modelId: ollamaModel,
            //        endpoint: new Uri(ollamaEndpoint)
            //    );

            //var embeddingGenerator = new OllamaEmbeddingGenerator(
            //        modelId: ollamaModel,
            //        endpoint: new Uri(ollamaEndpoint)
            //    );

            //var embeddingGenerator = new AddOllamaTextEmbeddingGeneration(ollamaEndpoint, ollamaModel)
            //    .GetEmbeddingClient("your chosen model")
            //    .AsIEmbeddingGenerator();

        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            Console.WriteLine("Please ensure Ollama is running and the specified model is downloaded.");
            //Console.WriteLine($"Check your Ollama endpoint: {ollamaEndpoint} and model: {ollamaModel}");
        }

        Console.WriteLine("Press any key to exit.");
        //Console.Read();
    }


    public async Task GenerateEmbeddingsWithPdfContentPlugin(string PDF_filename = @"C:\Users\risto\source\repos\PDF_Llama\PDFs\VN.pdf")
    {
        var httpClient = new HttpClient()
        {
            BaseAddress = ModelEndpoint,
            Timeout = TimeSpan.FromMinutes(5)
        };

        try
        {
            var builder = Kernel.CreateBuilder()
                .AddOllamaTextGeneration(
                    endpoint: ModelEndpoint,
                    modelId: ModelName
                );

            var kernel = builder.Build();

            Console.WriteLine($"Kernel initialized with Ollama model: {ModelName} at {ModelEndpoint}");


            // --- Import your custom plugin ---
            // The KernelPluginFactory.CreateFromType<T>() method is used to discover  kernel functions defined within the FileContentPlugin class.
            //var pdfContentPlugin = kernel.CreatePluginFromObject(new PdfContentPlugin());
            //Console.WriteLine("PdfContentPlugin loaded successfully.");


            builder.Services.AddOllamaTextGeneration(
                ModelName,
                httpClient
            );

            Reader reader = new Reader();
            var pdftxtlist = reader.ReadPdfToList(PDF_filename);

            // Assuming you have an IHttpClientFactory and a properly configured OllamaApiClient
            //var ollamaClient = new OllamaApiClient(ModelEndpoint, ModelName);

            var ollamaClient = new OllamaApiClient(httpClient)
            {
                SelectedModel = ModelName
            };

            //var gen = ollamaClient.AsTextEmbeddingGenerationService();
            var embeds = await ollamaClient.AsTextEmbeddingGenerationService().GenerateEmbeddingsAsync(pdftxtlist);

            // Serialize
            byte[] bytearr = MemoryPackSerializer.Serialize(embeds);
            File.WriteAllBytes(@"C:\tmp\embedding.bin", bytearr);

            // Deserialize
            ReadOnlyMemory<float> embeds_from_file = MemoryPackSerializer.Deserialize<ReadOnlyMemory<float>>(bytearr);

            System.Console.WriteLine($"Generated {embeds.Count}, serialised {embeds_from_file.Length} embeddings from Ollama for PDF: {PDF_filename}");

            //var prompt = $"Summarize the following text in one sentence: {documentText}";
            //var response = await chatClient.GetResponseAsync(prompt);
            //Console.WriteLine($"\nSummary:\n{response.Message.Text}");
            // Create the embedding service
            //var embeddingService = OllamaApiClient.AsEmbeddingGenerationService(ollamaClient, "nomic-embed-text");

            var embeddingRequest = new EmbedRequest
            {
                Input = pdftxtlist,
            };

            var embeddingGenerator = new OllamaApiClient(ModelEndpoint, ModelName)
                .EmbedAsync(embeddingRequest);

            //OllamaApiClientExtensions
            //    .AddOllamaTextEmbeddingGeneration(
            //        builder.Services,

            //        modelId: ollamaModel,
            //        endpoint: new Uri(ollamaEndpoint)
            //    );

            //var embeddingGenerator = new OllamaEmbeddingGenerator(
            //        modelId: ollamaModel,
            //        endpoint: new Uri(ollamaEndpoint)
            //    );

            //var embeddingGenerator = new AddOllamaTextEmbeddingGeneration(ollamaEndpoint, ollamaModel)
            //    .GetEmbeddingClient("your chosen model")
            //    .AsIEmbeddingGenerator();

        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            Console.WriteLine("Please ensure Ollama is running and the specified model is downloaded.");
            Console.WriteLine($"Check your Ollama endpoint: {ModelEndpoint.AbsoluteUri} and model: {ModelName}");
        }

        Console.WriteLine("Press any key to exit.");
    }



    /// <summary>
    /// Creates a sample text file with some dummy content for testing.
    /// </summary>
    /// <param name="fileName">The name of the file to create.</param>
    private static async Task CreateSampleTextFile(string fileName)
    {
        string content = @"The quick brown fox jumps over the lazy dog.
    This is a sample document to demonstrate file reading and summarization using Semantic Kernel and Ollama.
    It contains a few sentences about various topics.
    We are exploring how to integrate local data with large language models for different tasks.
    The Semantic Kernel makes it easier to build AI applications by providing a framework for plugins and orchestrators.
    Ollama allows you to run open-source large language models locally on your machine, providing privacy and control.
    This example showcases a basic use case, but the possibilities are vast, including more complex RAG (Retrieval Augmented Generation) scenarios.";

        try
        {
            await File.WriteAllTextAsync(fileName, content);
            Console.WriteLine($"Created sample file: {fileName}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating sample file {fileName}: {ex.Message}");
        }
    }
}

