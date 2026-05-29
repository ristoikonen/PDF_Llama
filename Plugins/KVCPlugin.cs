using Microsoft.SemanticKernel;
using PdfReader;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UglyToad.PdfPig.Graphics;


namespace Agent_Ollama.Plugins;

internal class KVCPlugin
{


    /// <summary>
    /// Read text into Key-Value-Count triples (KVC's). 
    /// Use for warehouse parts list. 
    /// The key is the part name, the value is the part number, and the count is the quantity of that part. 
    /// For example: 'Socket head screw M5X25    DM-2524    1'. 
    /// If the text is too short or doesn't contain meaningful information, state that.
    /// </summary>
    /// <param name="kernel">The Semantic Kernel instance (injected automatically).</param>
    /// <param name="filePath">Text source of KVC's.</param>
    /// <returns>KVC List.</returns>
    [KernelFunction("GetTuples")]
[Description("Reads a text file and generates a list of Key-Value-Count tuples of its content using an AI model.")]
public async Task<List<Tuple<string, string, int>>?> ExtractKVCs(
    Kernel kernel,
    [Description("The text to extract Key-Value-Count tuples from.")] string sourceText 
    )
{
        var triple = Tuple.Create("Socket head screw M5X25", "DM-2524", 1);
        var firstValue = triple.Item1; // returns "Socket head screw M5X25"
        var secondValue = triple.Item2; // returns "DM-2524"
        var thirdValue = triple.Item3; // returns 1

        List<Tuple<string, string, int>> kvcList = new List<Tuple<string, string, int>>();
        //kvcList.Add(triple);

        using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));

        try
        {
            string prompt = "";

            if (kernel is not null)
            {
                var builder = Kernel.CreateBuilder();

                // Create a prompt for the AI model.Instruct the model to summarize the provided text.
                // Socket head screw M5X25        1

                prompt = @$"Find and create Key-Value-Count triples of parts from text provided below. Sample Key-Value-Count tuple: 'Washer    DM-2524    2'
                If the text is too short or doesn't contain meaningful information, state that.

                Text to extract Key-Value-Count list from, originally from parts list PDF document, starts from here:
                {sourceText}

                Key-Value-Count list:";

                var result = await kernel.InvokePromptAsync(prompt); //, cancellationToken: cts.Token);

                //var list = result.GetValue<List<Tuple<string, string, int>>>() ?? null;

                var summary = result.GetValue<string>()?.Trim() ?? "No summary generated.";
                Console.WriteLine(summary);

                result.GetValue<string>()?.Trim();
                return null; // result.GetValue<string>()?.Trim() ?? "No summary generated." 

                //return list; 
            }
            return null;
        }
        catch (OperationCanceledException)
        {
            if (cts.IsCancellationRequested)
            {
                Console.WriteLine("The operation timed out!");
            }
            return kvcList;
        }
        catch (Exception ex)
        {
            //Console.WriteLine("Please ensure Ollama is running and the specified model is downloaded.");
            Console.WriteLine($"Error: {ex.Message}");
            return null;
        }

        // https://github.com/microsoft/semantic-kernel/blob/main/dotnet/samples/GettingStartedWithTextSearch/InMemoryVectorStoreFixture.cs#L139
    }
}

