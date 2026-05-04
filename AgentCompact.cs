using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Compaction;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
//using Newtonsoft.Json;
using OllamaSharp;

using Spectre.Console;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;


namespace Agent_Llama;

// Suppress MEAI001 diagnostic for evaluation-only API usage
#pragma warning disable MEAI001
#pragma warning disable MAAI001

public sealed class AgentStructuredOutput
{

    const string trees = @"ARGYLE APPLE - (Eucalyptus cinerea)
A small to medium sized tree with a straight trunk but usually twisted branches. Its bark is coarse, stringy and reddish-brown.

BLAKELYS RED GUM - (Eucalyptus blakelyi)
A medium sized spreading tree, 10 to 24 metres high. Its trunk is usually short and stout. The bark is smooth with large irregular white, grey and reddish brown streaks but grey and scaly at the base of the tree. The bark sheds in large plates, revealing fresh colours beneath. Its juvenile leaves are ovate and blue-green, while the adult leaves are lance-shaped, dull bluish-green, and 6–20 cm long. Its clusters of 7–15 white flowers bloom from October to December. The tree was named after William Blakely, a prominent early 20th-century Australian botanist.
Source and further information: Field Guide to the Native Trees of the ACT, National Parks Association of the ACT Inc., 3rd ed, 2017, p.71.

BRITTLE GUM - (Eucalyptus mannifera)
A small to medium tree, 6 to 20 metres high with an erect trunk and drooping leaves. The bark is smooth, occasionally dimpled with rough patches on the lower part of the trunk which is mottled creamy-white to grey. It is often powdery to reddish before shedding in flakes. There are no sharply-edged ‘scribbles’. The adult has grey-green lance-like leaves that are alternately stalked. It flowers from February to April. Insects feed on its sweet exudate.

Source and further information: Field Guide to the Native Trees of the ACT, National Parks Association of the ACT Inc., 3rd ed, 2017, p.71.

BROAD-LEAVED PEPPERMINT - (Eucalyptus dives)
A small to medium-sized tree, 8 to 20 metres high with low branches. Its bark is grey, finely interlaced and crumbly fibrous. The leaves are stalked, alternate, thick, broad and lance-like. They are dark or grey-green and have a strong peppermint small when crushed. It flowers from October to November.

Source and further information: Field Guide to the Native Trees of the ACT, National Parks Association of the ACT Inc., 3rd ed, 2017, p.88.

BUNDY - (Eucalyptus goniocalyx)
A small low-branching tree, 8 to 16 metres, with a short crooked trunk and a spreading crown of longish leaves. Its bark is rough, grey-brown coarsely flaky or blocky on the trunk, sometimes thick and deeply fissured at its base. The leaves are stalked, alternate and lance-like. They are long, green and glossy. It flowers from March to August.

Source and further information: Field Guide to the Native Trees of the ACT, National Parks Association of the ACT Inc., 3rd ed, 2017, p.86.

CANDLEBARK - (Eucalyptus rubida)
An irregular tree, 10 to 25 metres high. Its bark is smooth, creamy-white with a red tinge in late summer. Often rough at the base with strips on the trunk. The leaves are stalked, grey-green and dull. It flowers from November to February.

Source and further information: Field Guide to the Native Trees of the ACT, National Parks Association of the ACT Inc., 3rd ed, 2017, p.66
";



    // <summary>
    /// Represents information about a plant or tree.
    /// </summary>
    [Description("Information about a plant including it's name, latin name, size in metres, and description")]
    public class PlantInfo
    {

        [JsonPropertyName("name")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Name { get; set; }

        [JsonPropertyName("latinname")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? NameInLatin { get; set; }

        [JsonPropertyName("sizeinmetres")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? SizeInMetres { get; set; }

        [JsonPropertyName("description")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Description { get; set; }
    }

    public Uri ModelEndpoint { get; set; }
    public string ModelName { get; set; }

    public AgentStructuredOutput(Uri modelEndpoint, string modelName)
    {
        this.ModelEndpoint = modelEndpoint;
        this.ModelName = modelName;
    }


    [Description("Get plants name, latin name.")]
    public static PlantInfo ExtractPlantInfo(string name, string latinname)
    {
       
        return new PlantInfo
        {
            //LatinName = latinname,
            Name = name
        };
        //return Array.Empty<PlantInfo>();
    }


    public async Task RunAgent()
    {
        try { 


            var httpClient = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:11434"),
                Timeout = TimeSpan.FromMinutes(10) // 10-minute timeout
            };

            //var options = new ChatOptions
            //{
            //    ResponseFormat = ChatResponseFormat.ForJsonSchema<PlantInfo[]>()
            //};

            AIAgent agent = new OllamaApiClient(httpClient, ModelName)
                .AsAIAgent(
                
                name: "StructuredOutputAssistant",
                description: "You are a helpful assistant that extracts structured information about plants like their name, plants name in latin, size in metres and description."
            
            );


            AgentRunOptions runOptions = new()
            {
                ResponseFormat = ChatResponseFormat.ForJsonSchema<PlantInfo[]>()
            };

            AgentResponse<PlantInfo[]> response = await agent.RunAsync<PlantInfo[]>(trees.Trim(), options: runOptions);


            var data = new { Message = response.Text };
            string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });


            var jsonElement = JsonSerializer.Deserialize<JsonElement>(response.Text);

            dynamic allItems = JsonSerializer.Deserialize<dynamic>(response.Text)!;

            var res = response.Result;

            //if (response.Result is not null)
            //{ 
               // PlantInfo[]? plInfos = response.Result.ToArray<PlantInfo>(); // JsonSerializer.Deserialize<PlantInfo[]>(response.Result)!;
            //}

            //PlantInfo[] plInfo = response.Result?.ToArray<PlantInfo>()!;

            //PlantInfo[] plInfo = JsonSerializer.Deserialize<PlantInfo[]>(response.Text)!;


            // PlantInfo[] personInfo = JsonSerializer.Deserialize<PlantInfo[]>(response.Text, JsonSerializerOptions.Web)!;


            //, chatoptions: options
            /*
            chatptions: new ChatOptions()
            {
                ModelId = ModelName,
                Instructions = "You are a helpful assistant that extracts structured information about plants like their name, latin name, size in metres and description in an array. For exsample plant named silver wattle has latin name of Acacia dealbata."
                //,
                //ResponseFormat = ChatResponseFormat.ForJsonSchema<PlantInfo>
            }
            */

            // Microsoft.Extensions.AI.ChatResponseFormat.ForJsonSchema<PlantInfo[]>()




            /*
           AIAgent agent = new OllamaApiClient(httpClient, ModelName)
               .AsAIAgent(
                   instructions: "You are a helpful assistant that extracts structured information about plants like their name, latin name, size in metres and description in an array. For exsample plant named silver wattle has latin name of Acacia dealbata.",
                   name: "StructuredOutputAssistant",
                   tools: new[] { AIFunctionFactory.Create((Func<string,string,PlantInfo>)ExtractPlantInfo)}
               );


                      AIAgent agent = new OllamaApiClient(httpClient, ModelName)
                  .AsAIAgent(new ChatClientAgentOptions()
                  {
                      Name = "PlantExtractor",
                      ChatOptions = new()
                      {
                          Instructions = "You are a helpful assistant that extracts structured information about trees and plants like their name, latin name, size in metres and description into array.",
                          ResponseFormat = ChatResponseFormat.ForJsonSchema<PlantInfo>()
                      }
                  });

                      IAsyncEnumerable<AgentResponseUpdate> updates = agent.RunStreamingAsync(
                          trees);

                      AgentResponse response = await updates.ToAgentResponseAsync();

                      PlantInfo[] plants = JsonSerializer.Deserialize<PlantInfo[]>(response.Text)!;
                               */


            //AgentSession session = await agent.CreateSessionAsync();

            AgentRunOptions runOptions2 = new()
            {
                ResponseFormat = ChatResponseFormat.ForJsonSchema<PlantInfo[]>()
            };

            AgentResponse response2 = await agent.RunAsync( trees, options: runOptions2);

            //var data = new { Message = response2.Text };
            //string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });


            //var jsonElement = JsonSerializer.Deserialize<JsonElement>(response2.Text);

            //PlantInfo[] plInfo = JsonSerializer.Deserialize<PlantInfo[]>(response2.Text)!;

            PlantInfo[] pInfo = JsonSerializer.Deserialize<PlantInfo[]>(response2.Text, JsonSerializerOptions.Web)!;
            




            //Console.WriteLine($" FirstName: {personInfo.Name},LastName: {personInfo.Name}, Age: {personInfo.LatinName}, Occupation: {personInfo.Description}");


            //Console.WriteLine(await agent.RunAsync("List things I could like.", session));

            //Optional streaming response
            /*
            await foreach (var update in agent.RunStreamingAsync("List things i could potetially do and like.", session))
            {
                Console.WriteLine(update);
            }
            */

            //var deserializedSession = await agent.DeserializeSessionAsync(session);


        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            Console.WriteLine("Please ensure Ollama is running and the specified model is downloaded.");
            Console.WriteLine($"Check your Ollama endpoint: {ModelEndpoint} and model: {ModelName}");
        }

        Console.WriteLine("Press any key to exit.");

    } 
}
        //IChatClient summarizerChatClient = openAIClient.GetChatClient(deploymentName).AsIChatClient();

//        // Configure the compaction pipeline with one of each strategy, ordered least to most aggressive.
//        PipelineCompactionStrategy compactionPipeline =
//            new(// 1. Gentle: collapse old tool-call groups into short summaries
//                //new ToolResultCompactionStrategy(CompactionTriggers.MessagesExceed(7)),

//                //// 2. Moderate: use an LLM to summarize older conversation spans into a concise message
//                //new SummarizationCompactionStrategy(summarizerChatClient, CompactionTriggers.TokensExceed(0x500)),

//                //// 3. Aggressive: keep only the last N user turns and their responses
//                //new SlidingWindowCompactionStrategy(CompactionTriggers.TurnsExceed(4)),

//                //// 4. Emergency: drop oldest groups until under the token budget
//                //new TruncationCompactionStrategy(CompactionTriggers.TokensExceed(0x8000))
//                );

//        // Create the agent with a CompactionProvider that uses the compaction pipeline.
//        AIAgent agent =
//            agentChatClient
//                .AsBuilder()
//                // Note: Adding the CompactionProvider at the builder level means it will be applied to all agents
//                // built from this builder and will manage context for both agent messages and tool calls.
//                .UseAIContextProviders(new CompactionProvider(compactionPipeline))
//                .BuildAIAgent(
//                    new ChatClientAgentOptions
//                    {
//                        Name = "ShoppingAssistant",
//                        ChatOptions = new()
//                        {
//                            Instructions =
//                                """
//                        You are a helpful, but long winded, shopping assistant.
//                        Help the user look up prices and compare products.
//                        When responding, Be sure to be extra descriptive and use as
//                        many words as possible without sounding ridiculous.
//                        """,
//                            Tools = [AIFunctionFactory.Create(LookupPrice)]
//                        },
//                        // Note: AIContextProviders may be specified here instead of ChatClientBuilder.UseAIContextProviders.
//                        // Specifying compaction at the agent level skips compaction in the function calling loop.
//                        //AIContextProviders = [new CompactionProvider(compactionPipeline)]
//                    });

//        AgentSession session = await agent.CreateSessionAsync();

//    }

//// WARNING: DefaultAzureCredential is convenient for development but requires careful consideration in production.
//// In production, consider using a specific credential (e.g., ManagedIdentityCredential) to avoid
//// latency issues, unintended credential probing, and potential security risks from fallback mechanisms


//// Define a tool the agent can use, so we can see tool-result compaction in action.
//[Description("Look up the current price of a product by name.")]
//static string LookupPrice([Description("The product name to look up.")] string productName) =>
//    productName.ToUpperInvariant() switch
//    {
//        "LAPTOP" => "The laptop costs $999.99.",
//        "KEYBOARD" => "The keyboard costs $79.99.",
//        "MOUSE" => "The mouse costs $29.99.",
//        _ => $"Sorry, I don't have pricing for '{productName}'."
//    };



//// Helper to print chat history size
//void PrintChatHistory()
//{
//    //if (session.TryGetInMemoryChatHistory(out var history))
//    //{
//        Console.ForegroundColor = ConsoleColor.Cyan;
//        //Console.WriteLine($"\n[Messages: #{history.Count}]\n");
//        Console.ResetColor();
//    //}
//}

//// Run a multi-turn conversation with tool calls to exercise the pipeline.
//string[] prompts =
//[
//    "What's the price of a laptop?",
//    "How about a keyboard?",
//    "And a mouse?",
//    "Which product is the cheapest?",
//    "Can you compare the laptop and the keyboard for me?",
//    "What was the first product I asked about?",
//    "Thank you!",
//];
//public void RunAgentConversation()
//{ 
//    foreach (string prompt in prompts)
//    {
//        Console.ForegroundColor = ConsoleColor.Cyan;
//        Console.Write("\n[User] ");
//        Console.ResetColor();
//        Console.WriteLine(prompt);
//        Console.ForegroundColor = ConsoleColor.Cyan;
//        Console.Write("\n[Agent] ");
//        Console.ResetColor();
//        //Console.WriteLine(await agent.RunAsync(prompt, session));

//        PrintChatHistory();
//    }
//}

//}

////[Experimental(DiagnosticIds.Experiments.AgentsAIExperiments)]
////[Experimental(DiagnosticIds.Experiments.AgentsAIExperiments)]
//[SuppressMessage("Microsoft.Agents.AI", "MAAI001", Justification = "Acknowledged experimental API; suppressing analyzer for now to proceed with implementation.")]
//public sealed class PipelineCompactionStrategy : CompactionStrategy
//{
//    /// <summary>
//    /// Initializes a new instance of the <see cref="PipelineCompactionStrategy"/> class.
//    /// </summary>
//    /// <param name="strategies">The ordered sequence of strategies to execute.</param>
//    public PipelineCompactionStrategy(params IEnumerable<CompactionStrategy> strategies)
//        : base(CompactionTriggers.Always)
//    {
//        this.Strategies = [.. Throw.IfNull(strategies)];
//    }

//    /// <summary>
//    /// Gets the ordered list of strategies in this pipeline.
//    /// </summary>
//    public IReadOnlyList<CompactionStrategy> Strategies { get; }

//    /// <inheritdoc/>
//    protected override async ValueTask<bool> CompactCoreAsync(CompactionMessageIndex index, ILogger logger, CancellationToken cancellationToken)
//    {
//        bool anyCompacted = false;

//        foreach (CompactionStrategy strategy in this.Strategies)
//        {
//            bool compacted = await strategy.CompactAsync(index, logger, cancellationToken).ConfigureAwait(false);

//            if (compacted)
//            {
//                anyCompacted = true;
//            }
//        }

//        return anyCompacted;
//    }
//}