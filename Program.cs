using A2A;
using Agent_Ollama.Plugins;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Newtonsoft.Json.Schema;
using OllamaSharp;
using OllamaSharp.Models;
using PdfReader;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using UglyToad.PdfPig.Graphics;
using static System.Net.Mime.MediaTypeNames;


#pragma warning disable CA1861 // Avoid constant arrays as arguments
#pragma warning disable SKEXP0070 // AddOllamaTextGeneration

namespace Agent_Ollama;

public class Configurator
{
    public Dictionary<string, string?> Values;
    public dynamic Store;
    public Configurator()
    {
        // https://stackoverflow.com/questions/1653046/what-are-the-true-benefits-of-expandoobject
        // https://www.daveabrock.com/2021/01/19/config-top-level-programs/
        Store = new ExpandoObject();
        Store.Endpoints = new ExpandoObject();
        Store.Endpoints.ModelName = "llama3.2";
        Store.Endpoints.ModelEndpoint = new Uri(@"http://localhost:11434");
        //Console.WriteLine(${Store.Endpoints.ModelEndpoint});

        Dictionary<String, object> dict = new Dictionary<string, object>();
        Dictionary<String, object> address = new Dictionary<string, object>();
        dict["Address"] = address;
        address["State"] = "WA";
        Console.WriteLine(((Dictionary<string, object>)dict["Address"])["State"]);

        Values = new Dictionary<string, string?>
        {
            ["SecretKey"] = "Dictionary MyKey Value",
            ["TransientFaultHandlingOptions:Enabled"] = bool.TrueString,
            ["TransientFaultHandlingOptions:AutoRetryDelay"] = "00:00:07",
            ["Logging:LogLevel:Default"] = "Warning"
        };

    }

    // public string MyProperty { get; set; }

}


public class Program
{
    static async Task Main(string[] args)
    {
        ConfigurationManager configurationManager = new ConfigurationManager();

        //Configuration config = ConfigurationManager..OpenExeConfiguration(Application.ExecutablePath);
        //ConfigurationSection section = config.GetSection("connectionStrings") as ConnectionStringsSection;

        var starts = await FillStartMeUpsAsync();
        //var config = configurationManager.GetRequiredSection("appSettings");

        var configvalue1 = configurationManager.Sources; // ("ModelEndpoint"); //.AppSettings["countoffiles"];

        SpectreConsoleOutput.DisplayTitleH3($"Use Semantic Kernel Plugin; PDF Summariser -  Get response from Ollama IChatClient");

        // user choice scenarios
        var scenarios = SpectreConsoleOutput.SelectScenarios();
        var scenario = scenarios[0];

        Uri uri = starts.ModelEndpoint;

        DotNetAI dotnetai = new(starts.ModelEndpoint, starts.ModelName);
        AtoA a2a = new(starts.ModelEndpoint, starts.ModelName);
        AgentStructuredOutput agent_struct =  new(starts.ModelEndpoint, starts.ModelName);

        
        Embed embed = new(starts.ModelEndpoint, starts.ModelName);

        ////var cardResolver = new A2ACardResolver(starts.ModelEndpoint);
        //var agentCard = await cardResolver.GetAgentCardAsync();

        switch (scenario)
        {
            case "Embed":
                //InMemoryVectorStoreFixture inMemoryVectorStoreFixture = new InMemoryVectorStoreFixture();
                //await inMemoryVectorStoreFixture.InitializeAsync();
                await OldMain(Array.Empty<string>());
                await embed.CreateAgent("", "");
                break;

            case "Nested":
                string parts = @"C:\tmp\W812 - Parts List.pdf";
                PDF_AI_Summariser pdf_AI_Summariser2 = new(starts.ModelEndpoint, starts.ModelName);
                await pdf_AI_Summariser2.SummarizeFileUsingPdfContentPlugin(parts);

                //await nested.CreateAgent("","");
                break;

            case "AtoA":
                await a2a.CreateAgent("What is the second largest city in France?",
                    "What is the third largest city in italy?");
                break;

            case "PDF AI Summariser":
                PDF_AI_Summariser pdf_AI_Summariser = new(starts.ModelEndpoint, starts.ModelName);
                await pdf_AI_Summariser.GenerateEmbeddingsWithPdfContentPlugin();
                break;

            case "Long Agent Task":
                string book = System.IO.File.ReadAllText(@"C:\tmp\BooklInstructionS2.txt");
                //await dotnetai.RunLongAgent($"Write booklet of about 25 pages that tutors student to pass AZ-900 'Azure Fundamentals' test. Write a paragraph per number: {book}");
                await dotnetai.RunLongAgent($"GENERATE QUESTIONS WITH ANSWERS FOR studentS to TRAIN FOR AZ-900 'Azure Fundamentals' test. CREATE ONE QUOESTION PER NUMBERED CAPTION: {book}");
                break;

            case "Store Embedding":
                ReadOnlyMemory<float> embedding = null;
                await dotnetai.GenerateEmbedding();
                //await dotnetai.StoreEmbedding(embedding, "Get oil price.");
                break;

            case "Oil Price Agent":
                await dotnetai.UseOilAgent("Get oil price.");
                break;

            case "Structured":
                await agent_struct.RunAgent();
                break;

            case "Traffic Agent":
                await dotnetai.TrafficAgent("Sydney");
                break;

            case "Conversation":
                //TODO: Use params
                await dotnetai.Conversation("");
                break;

            case "Use Agent":
                DotNetAI.GetWeather("paris");
                await dotnetai.UseAgent("what is price of bitcoin?");
                break;

            case "Get Response":
                await dotnetai.GetResponse("tell me about albert einstein");
                break;

            case "Generate image":
                await dotnetai.CreateImage("draw a circle");
                break;

            case "IChatClient":

                string input = AnsiConsole.Ask<string>($"Input text of an article for analysis with {starts.ModelName} by pasting it here.");
                //string clipboardText = System.Windows.Forms.Clipboard.GetText();
                var panel = new Panel(input);
                AnsiConsole.Write(panel);
                Console.WriteLine(Environment.NewLine);
                AnsiConsole.Markup($"[bold yellow]Analysing with {starts.ModelName}[/]");

                const string spareparts_text_file = @"c:\tmp\W812 - Parts List.pdf";
                Reader reader = new Reader();
                var spareparts_text = reader.ReadPdf(spareparts_text_file) ?? "";

                var jsonSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        key = new { type = "string" },
                        value = new { type = "string" },
                        count = new { type = "integer" }, 

                    },
                    required = new[] { "name", "value" }
                };

                string prompt = "Find and create Key-Value-Count part data from part list from text provided:" + spareparts_text;

                var request = new GenerateRequest
                {
                    Model = starts.ModelName,
                    Prompt = prompt,
                    System = "You are a data extractor.",
                    Format = JsonSerializer.Serialize(jsonSchema), // Apply JSON Schema here
                    ////Options = new RequestOptions
                    ////{
                    ////    Temperature = 0.2, // Controls creativity (lower is more deterministic)
                    ////    NumPredict = 250   // Max tokens to generate
                    ////}
                };


                var api_client = new OllamaApiClient(starts.ModelEndpoint, starts.ModelName);
                var response = api_client.GenerateAsync(request);
                    //TODO: continue!
                var jsonOutput = await response.StreamToEndAsync();
                Console.WriteLine(jsonOutput);

                //api_client.GenerateAsync<SparePart>(request );

                // Microsoft.Extensions.AI
                IChatClient chatClient =
                    new OllamaApiClient(starts.ModelEndpoint, starts.ModelName);



                //var posts = Directory.GetFiles("my_document").Take(1).ToArray();
                //var post = posts[0];
                //string post = @"A pupa (from Latin pupa 'doll'; pl.: pupae) is the life stage of insects from the Holometabola clade undergoing transformation between immature and mature stages. Insects that go through a pupal stage are holometabolous: they go through four distinct stages in their life cycle, the stages thereof being egg, larva, pupa, and imago. The processes of entering and completing the pupal stage are controlled by the insect's hormones, especially juvenile hormone, prothoracicotropic hormone, and ecdysone. The act of becoming a pupa is called pupation, and the act of emerging from the pupal case is called eclosion or emergence.
                //            The pupae of different groups of insects have different names such as chrysalis for the pupae of butterflies and tumbler for those of the mosquito family. Pupae may further be enclosed in other structures such as cocoons, nests, or shells.";
                
                // {File.ReadAllText(post)}
                string prompt2 = $$"""
                     You will receive an input text and the desired output format.
                     You need to analyze the text and produce the desired output format.
                     You not allow to change code, text, or other references.

                     # Desired response

                     Only provide a RFC8259 compliant JSON response following this format without deviation.

                     {
                        "title": "Title pulled from the front matter section",
                        "summary": "Summarize the article in no more than 100 words"
                     }

                     # Article content:

                     {{input}}
                     """;

                var response_to_prompt = await chatClient.GetResponseAsync(prompt2);
                Console.WriteLine(response_to_prompt.Text);
                Console.WriteLine(Environment.NewLine);


                /*
                List<ChatMessage> chatHistory = new();

                while (Console.ReadKey().Key != ConsoleKey.Escape)
                {
                    // Get user prompt and add to chat history
                    Console.WriteLine("Your prompt:");
                    var userPrompt = Console.ReadLine();
                    chatHistory.Add(new ChatMessage(ChatRole.User, userPrompt));

                    // Stream the AI response and add to chat history
                    Console.WriteLine("AI Response:");
                    var response = "";
                    await foreach (ChatResponseUpdate item in
                        chatClient.GetStreamingResponseAsync(chatHistory))
                    {
                        Console.Write(item.Text);
                        response += item.Text;
                    }
                    chatHistory.Add(new ChatMessage(ChatRole.Assistant, response));
                    Console.WriteLine();
                }
                */

                break;
        }
    }

    public class StructOut 
    { 
        string ID { get; set; }
        string Name { get; set; }
        int Count { get; set; }
    }


    public class SparePart
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public int Count { get; set; }
    }


    public class Doc
    {
        public string Recommendation { get; set; }
        public string Response { get; set; }
    }

    public static async Task OldMain(string[] args)
    {
        // --- Configuration ---
        const string ollamaEndpoint = "http://localhost:11434";
        const string ollamaModel = "llama3.2";
        // Name of the sample text file to summarize. Make sure this file exists in the
        // same directory as your application's executable, or provide a full path.
        const string sampleFileName = "my_document.txt";

        ImageExtractor extract = new ImageExtractor();
        await extract.ReadPdf(@"C:\tmp\Government-Response-p2019-41708.pdf");

        Console.WriteLine("Setting up Semantic Kernel with Ollama...");


        // --- Create a sample text file for demonstration ---
        // This ensures there's a file for the plugin to read.
        await CreateSampleTextFile(sampleFileName);

        // --- Initialize the Semantic Kernel ---
        try
        {
            // Add the Ollama text generation service to the kernel.
            var builder = Kernel.CreateBuilder()
                .AddOllamaTextGeneration(
                    modelId: ollamaModel,
                    endpoint: new Uri(ollamaEndpoint)
                );

            // Build the kernel instance
            var kernel = builder.Build();
            
            // utilize qwen2.5 or llama3.1:8b
            Console.WriteLine($"Kernel initialized with Ollama model: {ollamaModel} at {ollamaEndpoint}");

            // --- Import your custom plugin ---
            // The KernelPluginFactory.CreateFromType<T>() method is used to discover  kernel functions defined within the FileContentPlugin class.
            var pdfContentPlugin = kernel.CreatePluginFromObject(new PdfContentPlugin());
            kernel.Plugins.Add(pdfContentPlugin);

            Console.WriteLine("PdfContentPlugin loaded successfully.");

            // --- Define the path to the text file ---
            //string filePath = Path.GetFullPath(sampleFileName);

            // AIAgent aIAgent = new AIAgent(pdfContentPlugin);

            var pdfpath = @"C:\Users\risto\source\repos\Agent_Ollama\Vn.pdf";
            
            var evo = @"C:\Users\risto\OneDrive\Documents\what_evolution_is_not.pdf";
            Reader reader = new Reader(pdfpath);
            var pdftxt = reader.ReadPdf(pdfpath);
            var small_pdftxt = reader.ReadPdfSmall(evo);

            const string spareparts_text_file = @"c:\tmp\australian-government-response-to-the-final-report-of-the-royal-commission-into-aged-care-quality-and-safety.pdf";
            var spareparts_text = reader.ReadPdf(spareparts_text_file);

            var jsonSchemas = new
            {
                type = "object",
                properties = new
                {
                    foreword = new { type = "string" },
                    introduction = new { type = "string" },
                    //count = new { type = "integer" },

                },
                required = new[] { "recommendation", "response" }
            };

            string jsonSchema = """
            {
                "type": "object",
                "properties": {
                    "foreword": { "type": "string" },
                    "introduction": { "type": "string" }
                },
                "required": ["foreword", "introduction"]
            }
            """;

            AgentRunOptions runOptions = new()
            {
                ResponseFormat = ChatResponseFormat.ForJsonSchema(JsonElement.Parse(jsonSchema), "Doc", "Find parts of text")
                //ResponseFormat = ChatResponseFormat.ForJsonSchema<SparePart>()
            };

            // 2. Set up options to enforce Structured Output and low temperature for accuracy
            var structuredOptions = new ChatOptions
            {
                Temperature = 0.0f, // Essential for strict formatting adherence
                ResponseFormat = ChatResponseFormat.ForJsonSchema<SparePart>(
                )
            };
    

            IChatClient client = new OllamaChatClient(ollamaEndpoint, ollamaModel);

            AIAgent agent_KVC = client.AsAIAgent(
                instructions: "You are a helpful data extractor finding data from text: " + spareparts_text,
                name: "Summarum"
                );

            AgentResponse response = await agent_KVC.RunAsync("Extract text details.", options: runOptions);

            JsonElement result = JsonSerializer.Deserialize<JsonElement>(response.Text);

            Doc doc = JsonSerializer.Deserialize<Doc>(response.Text, JsonSerializerOptions.Web)!;

            SparePart personInfo = JsonSerializer.Deserialize<SparePart>(response.Text, JsonSerializerOptions.Web)!;


            var result_kvc = await agent_KVC.RunAsync<SparePart>("Get spare part details into a structure from text provided: " + spareparts_text);

            Console.WriteLine("\n--- Spare part details from agent_KVC ---");
            Console.WriteLine(result_kvc.Text);
            Console.WriteLine(result_kvc.Result);
            Console.WriteLine("---------------------------\n");

            var response2 = await client.GetResponseAsync<SparePart>("Get spare part details into a structure from text provided.");

            Console.WriteLine("\n--- Spare part details from agent_KVC ---");
            Console.WriteLine(response2.Text);
            Console.WriteLine(response2.Result);
            Console.WriteLine("---------------------------\n");

            AIAgent agent = client.AsAIAgent(
                instructions: "You are a text summariser running locally via Ollama.",
                name: "Summarum"
                );

            var res = await agent.RunAsync("Summarise this text: " + small_pdftxt);

            //pdfContentPlugin["SummarizeFile"],
            //new() { ["pdfFileName"] = PDF_filename }
            //);

            Console.WriteLine($"Here is the response: {res}");
            // --- Invoke the plugin function ---
            // Call the 'SummarizeFile' function from your 'FileContentPlugin'.
            // The file path is passed as a named argument.
            var result2 = await kernel.InvokeAsync(
                pdfContentPlugin["SummarizeFile"],
                new() { ["pdfFileName"] = evo }
            );

            // PDF_filename 

            Console.WriteLine("\n--- Summary from Ollama ---");
            Console.WriteLine(result2.GetValue<string>());
            Console.WriteLine("---------------------------\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            Console.WriteLine("Please ensure Ollama is running and the specified model is downloaded.");
            Console.WriteLine($"Check your Ollama endpoint: {ollamaEndpoint} and model: {ollamaModel}");
        }

        Console.WriteLine("Press any key to exit.");
        //Console.Read();
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

    static async Task<StartMeUps> FillStartMeUpsAsync()
    {
        return await Task.FromResult<StartMeUps>(new StartMeUps
        {
            ModelEndpoint = new Uri("http://localhost:11434"),
            ModelName = "llama3.2"
        });

        //"qwen3-embedding:0.6b" //"llama3.2" // "mistral"  "deepseek-r1:1.5b"
    }

}

//System.Diagnostics.Process p = new System.Diagnostics.Process();
//p.StartInfo.WorkingDirectory = @"C:\Users\risto\source\repos\Kernel";
//p.StartInfo.FileName = "runr README.MD";
//p.StartInfo.UseShellExecute = true;
//p.Start();

