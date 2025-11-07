using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using OllamaSharp;
using Spectre.Console;
using System.Configuration;
using System.Dynamic;
using static System.Net.Mime.MediaTypeNames;



#pragma warning disable CA1861 // Avoid constant arrays as arguments
#pragma warning disable SKEXP0070 // AddOllamaTextGeneration

namespace PDF_Llama;

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
        // write title

        SpectreConsoleOutput.DisplayTitleH3($"Use Semantic Kernel Plugin; PDF Summariser -  Get response from Ollama IChatClient");

        // user choice scenarios
        var scenarios = SpectreConsoleOutput.SelectScenarios();
        var scenario = scenarios[0];

        DotNetAI dotnetai = new(starts.ModelEndpoint, starts.ModelName);

        // present
        switch (scenario)
        {
            

            case "PDF AI Summariser":
                PDF_AI_Summariser pdf_AI_Summariser = new(starts.ModelEndpoint, starts.ModelName);
                await pdf_AI_Summariser.SummarizeFileUsingPdfContentPlugin();
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

                // Microsoft.Extensions.AI
                IChatClient chatClient =
                    new OllamaApiClient(starts.ModelEndpoint, starts.ModelName);

                //var posts = Directory.GetFiles("my_document").Take(1).ToArray();
                //var post = posts[0];
                //string post = @"A pupa (from Latin pupa 'doll'; pl.: pupae) is the life stage of insects from the Holometabola clade undergoing transformation between immature and mature stages. Insects that go through a pupal stage are holometabolous: they go through four distinct stages in their life cycle, the stages thereof being egg, larva, pupa, and imago. The processes of entering and completing the pupal stage are controlled by the insect's hormones, especially juvenile hormone, prothoracicotropic hormone, and ecdysone. The act of becoming a pupa is called pupation, and the act of emerging from the pupal case is called eclosion or emergence.
                //            The pupae of different groups of insects have different names such as chrysalis for the pupae of butterflies and tumbler for those of the mosquito family. Pupae may further be enclosed in other structures such as cocoons, nests, or shells.";
                
                // {File.ReadAllText(post)}
                string prompt = $$"""
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

                var response_to_prompt = await chatClient.GetResponseAsync(prompt);
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


    public static async Task OldMain(string[] args)
    {
        // --- Configuration ---
        const string ollamaEndpoint = "http://localhost:11434";
        const string ollamaModel = "llama3.2";
        // Name of the sample text file to summarize. Make sure this file exists in the
        // same directory as your application's executable, or provide a full path.
        const string sampleFileName = "my_document.txt";
        const string PDF_filename = @"VN.pdf";

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

            Console.WriteLine($"Kernel initialized with Ollama model: {ollamaModel} at {ollamaEndpoint}");

            // --- Import your custom plugin ---
            // The KernelPluginFactory.CreateFromType<T>() method is used to discover  kernel functions defined within the FileContentPlugin class.
            var pdfContentPlugin = kernel.CreatePluginFromObject(new PdfContentPlugin());
            Console.WriteLine("PdfContentPlugin loaded successfully.");

            // --- Define the path to the text file ---
            //string filePath = Path.GetFullPath(sampleFileName);
            //Console.WriteLine($"Attempting to summarize file: {filePath}");

            // --- Invoke the plugin function ---
            // Call the 'SummarizeFile' function from your 'FileContentPlugin'.
            // The file path is passed as a named argument.
            var result = await kernel.InvokeAsync(
                pdfContentPlugin["SummarizeFile"],
                new() { ["pdfFileName"] = PDF_filename }
            );

            Console.WriteLine("\n--- Summary from Ollama ---");
            Console.WriteLine(result.GetValue<string>());
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
            ModelName = "llama3.2" // "mistral"  "deepseek-r1:1.5b"
        });
    }

}

//System.Diagnostics.Process p = new System.Diagnostics.Process();
//p.StartInfo.WorkingDirectory = @"C:\Users\risto\source\repos\Kernel";
//p.StartInfo.FileName = "runr README.MD";
//p.StartInfo.UseShellExecute = true;
//p.Start();

