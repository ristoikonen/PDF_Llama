using Agent_Ollama.Agents;
using Agent_Ollama.Models;
using Azure.AI.OpenAI;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Agents.AI;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.SqliteVec;
using OpenAI.Images;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using static UglyToad.PdfPig.Core.PdfSubpath;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;
using Agent_Ollama.Helpers;

namespace Agent_Ollama;

#pragma warning disable MEAI001


public sealed class DotNetAI
{
    // comfyui 
    public Uri ModelEndpoint { get; set; }
    public string ModelName { get; set; }
    private readonly ILogger _logger;

    //record PriceResult(decimal Price);

    record PriceResult
    {
        public string? id { get; init; }
        public string? name { get; init; }
        public string? symbol { get; init; }
        public string? price_usd { get; init; }
        public string? percent_change_1h { get; init; }     
        public string? percent_change_24h { get; init; }
    }

    public DotNetAI(Uri modelEndpoint, string modelName, ILogger logger)
    {
        this.ModelEndpoint = modelEndpoint;
        this.ModelName = modelName;
        this._logger = logger;
    }

    // https://github.com/microsoft/agent-framework/blob/main/dotnet/samples/02-agents/Agents/Agent_Step01_UsingFunctionToolsWithApprovals/Program.cs


    [Description("Fetches the current price of a Bitcoin.")]
    public static async Task<string> GetBitcoinPrice(
        //[Description("Cryptocurrency symbol, e.g. 'BTC', 'ETH'")] string id,
        HttpClient httpClient)
    {
        try
        {
            // Example: replace with a real provider
            //var url = $"https://api.coinlore.net/api/ticker/?id={id.ToUpperInvariant()}";
            PriceResult? pr = null;

            var url = $"https://api.coinlore.net/api/ticker/?id=90";

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var content = await httpClient.GetStringAsync(url);

            //var result = await httpClient.GetFromJsonAsync<PriceResult>(url);

            dynamic? obj = JsonSerializer.Deserialize<dynamic>(content);

            var list = JsonSerializer.Deserialize<List<JsonElement>>(content);
            foreach (var element in list!)
            {
                pr = JsonSerializer.Deserialize<PriceResult>(element);
                return pr?.price_usd ?? "0";
                //Console.WriteLine(element.GetProperty("price_us").GetString());
            }

            return "0";

            //    ? $"{id.ToUpperInvariant()}: ${result.price_us ?? "N/A"} USD"
            //    : $"Price for {id} not available.";
        }
        catch (Exception ex)
        {
            return $"Failed to retrieve price : {ex.Message}";
        }
    }


    [Description("Get the weather for a given location.")]
    public static string GetWeather([Description("The location to get the weather for.")] string location)
        => $"The weather in {location} is ...";


    [Description("Get the oil price per barrel in USD.")]
    public static string GetOilBarrelPrice([Description("Gets current oli price per barrel.")] string price)
        => $"The oil price is {price}.";


    [Description("Get the oli price per barrel in USD.")]
    public static Delegate getOilPrice = (AIFunctionArguments args) =>
    {
        // Access named parameters from the arguments dictionary.
        string? price = args.TryGetValue("price", out object? loc) ? loc?.ToString() : "$";
        string? units = args.TryGetValue("units", out object? u) ? u?.ToString() : "barrel";

        return $"Oli price in {units}: 101{price}";
    };

    // Create the AIFunction.
    public static AIFunction getOil = AIFunctionFactory.Create(getOilPrice);

    public async Task UseOilAgent(string question)
    {

        try
        {
            IChatClient client = new OllamaChatClient(ModelEndpoint, ModelName);

            // running locally via Ollama

            AIAgent agent = client.AsAIAgent(
                instructions: "You are a helpful assistant finding current oil price in USD."
               // , tools: [tool]
               , tools: [AIFunctionFactory.Create(getOilPrice)]
                );

            AgentSession session = await agent.CreateSessionAsync();

            // First turn
            //_logger.LogAgentResponse(await agent.RunAsync("Find oli price per barrel.", session));


            //AIAgent agentoil = client.AsAIAgent(
            //    instructions: "You are a helpful assistant running finding current oil price."
            //   , tools: [AIFunctionFactory.Create(GetOilBarrelPrice)]
            //    );

            //_logger.LogAgentResponse(await agentoil.RunAsync("Find oli price per barrel."));


            AgentResponse response = await agent.RunAsync(question, session);

            List<ToolApprovalRequestContent> approvalRequests = response.Messages.SelectMany(m => m.Contents).OfType<ToolApprovalRequestContent>().ToList();

            while (approvalRequests.Count > 0)
            {
                List<ChatMessage> userInputResponses = approvalRequests
                .ConvertAll(functionApprovalRequest =>
                {
                    var functionName = ((Microsoft.Extensions.AI.FunctionCallContent)functionApprovalRequest.ToolCall).Name;
                    _logger.LogToolApprovalRequest(functionName);
                    return new ChatMessage(ChatRole.User, [functionApprovalRequest.CreateResponse(Console.ReadLine()?.Equals("Y", StringComparison.OrdinalIgnoreCase) ?? false)]);
                });

                response = await agent.RunAsync(userInputResponses, session);

                approvalRequests = response.Messages.SelectMany(m => m.Contents).OfType<ToolApprovalRequestContent>().ToList();
            }

            _logger.LogAgentResponse($"\nAgent: {response}");

        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
        }
    }

       
    // USAGE:
    // await dotnetai.RunLongAgent(@"Write tutorial to learn how to pass AZ-900 'Azure Fundamentals' test");
    public async Task RunLongAgent(string instructions)
    {

        try
        {
            long startTime = Stopwatch.GetTimestamp();
            
            IChatClient client = new OllamaChatClient(ModelEndpoint, ModelName);

            AIAgent agent = client.AsAIAgent(
                instructions: instructions
                //, tools: [AIFunctionFactory.Create(GetOilBarrelPrice)]
                );

            AgentRunOptions options = new()
            {
                AllowBackgroundResponses = true
                //,AdditionalProperties = { "response_mode" = "streaming" } // Enable streaming responses
            };  

            AgentSession session = await agent.CreateSessionAsync();

            // Get initial response - may return with or without a continuation token
            AgentResponse response = await agent.RunAsync(instructions, session, options);

            _logger.LogResponseElapsedTime("Initial response time:", Stopwatch.GetElapsedTime(startTime).ToString());

            // Continue to poll until the final response is received
            while (response.ContinuationToken is not null)
            {
                // Wait before polling again.
                await Task.Delay(TimeSpan.FromSeconds(2));

                options.ContinuationToken = response.ContinuationToken;
                response = await agent.RunAsync(session, options);
            }

            System.IO.File.WriteAllText(@"C:\tmp\agent_response2NEW.txt", instructions + Environment.NewLine + response.Text);

            _logger.LogResponseElapsedTime("Final response time:", Stopwatch.GetElapsedTime(startTime).ToString());


        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
        }
    }


    public async Task TrafficAgent(string city)
    {
        try
        {
            _logger.LogAgent("TrafficAgent", city);

            var start =  DateTime.Now;
            long startTime = Stopwatch.GetTimestamp();


            TrafficAgent traffic = new TrafficAgent(ModelName, ModelEndpoint);

            var roads = await traffic!.RushHour(city) ?? new List<Road>();

            //TimeSpan elapsed = Stopwatch.GetElapsedTime(startTime);

            //LogExtensions.LogResponseElapsedTime(_logger, "Traffic report time:", Stopwatch.GetElapsedTime(startTime));
            _logger.LogResponseElapsedTime("Traffic report time:", Stopwatch.GetElapsedTime(startTime).ToString());

        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
        }

    }




    public async Task UseAgent(string question)
    {


        try
        {

        //    var httpClient = new HttpClient();
        //    PriceResult? pr = null;

        //    var url = $"https://api.coinlore.net/api/ticker/?id=90";

        //    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        //    var content = await httpClient.GetStringAsync(url);

        //    dynamic? obj = JsonSerializer.Deserialize<dynamic>(content);

        //    var list = JsonSerializer.Deserialize<List<JsonElement>>(content);
        //    foreach (var element in list!)
        //    {
        //        pr = JsonSerializer.Deserialize<PriceResult>(element);
        //        if (pr is not null)
        //        {
        //            string price = pr.price_usd ?? "0";
        //            _logger.LogAgentResponse(price);

        //        }
        //        //_logger.LogAgentResponse(element.GetProperty("price_us").GetString());
        //    }

            IChatClient client = new OllamaChatClient(ModelEndpoint, ModelName);

            AIAgent agent = client.AsAIAgent(
                instructions: "You are a helpful assistant getting latest Bitcoin price using AI Function GetBitcoinPrice."
               // , tools: [tool]
               , tools: [new ApprovalRequiredAIFunction(AIFunctionFactory.Create(GetBitcoinPrice))]
            );


            AgentSession session = await agent.CreateSessionAsync();

            AgentResponse response = await agent.RunAsync(question, session);
            List<ToolApprovalRequestContent> approvalRequests = response.Messages.SelectMany(m => m.Contents).OfType<ToolApprovalRequestContent>().ToList();

            while (approvalRequests.Count > 0)
            {
                List<ChatMessage> userInputResponses = approvalRequests
                .ConvertAll(functionApprovalRequest =>
                {
                    //Microsoft.Extensions.AI.FunctionCallContent functionCall = (Microsoft.Extensions.AI.FunctionCallContent)functionApprovalRequest.ToolCall;

                    var functionName = ((Microsoft.Extensions.AI.FunctionCallContent)functionApprovalRequest.ToolCall).Name;
                    _logger.LogToolApprovalRequest(functionName);
                    //$"Id: {((Microsoft.Extensions.AI.FunctionCallContent)functionApprovalRequest.ToolCall).Arguments?["id"] } , " +
                    return new ChatMessage(ChatRole.User, [functionApprovalRequest.CreateResponse(Console.ReadLine()?.Equals("Y", StringComparison.OrdinalIgnoreCase) ?? false)]);
                });

                response = await agent.RunAsync(userInputResponses, session);

                approvalRequests = response.Messages.SelectMany(m => m.Contents).OfType<ToolApprovalRequestContent>().ToList();
            }

            _logger.LogAgentResponse($"\nAgent: {response}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
        }

    }



    // Store the embedding in a local SQLite
    public async Task StoreEmbedding(ReadOnlyMemory<float> embedding, string collectionName)
    {

        
        // 1. Create the connection
        var connection = new SqliteConnection("Data Source=pdfvec.db");
        await connection.OpenAsync();

        // 2. Initialize the SQLite Vector Store
        var vectorStore = new SqliteVectorStore("Data Source=pdfvec.db");
        var collection = vectorStore.GetCollection<string, DocumentChunk>(collectionName);

        // 3. Ensure the table and vector index exist
        await collection.EnsureCollectionExistsAsync();

        // 4. Save a record (assuming 'embedding' was generated via Ollama/Qwen3)
        var chunk = new DocumentChunk
        {
            Text = collectionName,
            Vector = embedding,
            //TODO: Add PDFs filename
            FileName = collectionName
        };

        await collection.UpsertAsync(chunk);


    }




    public async Task Conversation(string conversation_starter)
    {

        ////    AIAgent agent = new AzureOpenAIClient(
        ////new Uri(endpoint),
        ////new DefaultAzureCredential())
        ////.GetChatClient(deploymentName)
        ////.AsAIAgent(instructions: "You are good at telling jokes.", name: "Joker");


        try
        {
            const string question = "What roads have heavy traffic and which times and directions, in Melbourne?";

            IChatClient client = new OllamaChatClient(ModelEndpoint, ModelName);

            AIAgent agent = client.AsAIAgent(
                instructions: "You are a helpful personal assistant running locally via Ollama.",
                name: "Jeeves"
                );

            AgentSession session = await agent.CreateSessionAsync();
            var serialized = await agent.SerializeSessionAsync(session);

            AgentResponse<List<Road>> structuredResponse = await agent.RunAsync<List<Road>>(question, session);
            List<Road> movies = structuredResponse.Result;


            var deserializedSession = await agent.DeserializeSessionAsync(serialized);

            //_logger.LogAgentResponse(await agent.RunAsync("List things I could like.", session));

            //Optional streaming response
            /*
            await foreach (var update in agent.RunStreamingAsync("List things i could potetially do and like.", session))
            {
                _logger.LogAgentResponse(update);
            }
            */

            //var deserializedSession = await agent.DeserializeSessionAsync(session);


        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);

            _logger.LogCheckOllamaConfig(ModelEndpoint.AbsoluteUri, ModelName);
        }

    }

    public async Task GetResponse(string question = @"Describe your model")
    {
        // --- Configuration ---
        const string ollamaEndpoint = "http://localhost:11434";
        const string ollamaModel = "llama3.2";


        try
        {
            IChatClient client = new OllamaChatClient(new Uri(ollamaEndpoint), ollamaModel);
            var response = await client.GetResponseAsync(question);
            var txt = response?.Text;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            _logger.LogCheckOllamaConfig(ollamaEndpoint, ollamaModel);
        }

    }

    // Suppress MEAI001 diagnostic for evaluation-only API usage
#pragma warning disable MEAI001

    public async Task CreateImage(string question = @"Hello!")
    {
        // --- Configuration ---
        const string ollamaEndpoint = "http://localhost:11434";
        const string ollamaModel = "llama3.2";

        try
        {
            ImageClient client = new(ollamaModel,"");

            GeneratedImage generatedImage = await client.GenerateImageAsync("""
                A postal card with a happy hiker waving and a beautiful mountain in the background.
                There is a trail visible in the foreground.
                The postal card has text in red saying: 'You are invited for a hike!'
                """,
                new OpenAI.Images.ImageGenerationOptions
                {
                    Size = GeneratedImageSize.W1024xH1024
                });

        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            _logger.LogCheckOllamaConfig(ollamaEndpoint, ollamaModel);
        }

    }










    public async Task GenerateEmbedding(string PDF_filename = @"C:\Users\risto\source\repos\PDF_Llama\PDFs\VN.pdf")
    {

        try
        {
            // 1024

            var options = new EmbeddingGenerationOptions { Dimensions = 1024 };

            IEmbeddingGenerator<string, Embedding<float>> generator = new OllamaEmbeddingGenerator(ModelEndpoint, ModelName);
            // 2. Generate embedding for a single string
            var text = "Hello, world!";
            var embeddings = await generator.GenerateAsync([text],options);

            // 3. Extract the vector data
            var vector = embeddings[0].Vector;

            _logger.LogVectorDimension(vector.Length);

            //StoreEmbedding(vector, "arcdoc").Wait();
            // 1. Create the connection
            var connection = new SqliteConnection("Data Source=pdfvec.db");
            await connection.OpenAsync();

            // 2. Initialize the SQLite Vector Store
            var vectorStore = new SqliteVectorStore("Data Source=pdfvec.db");
            var collection = vectorStore.GetCollection<string, DocumentChunk>("pdfdoc");

            byte[] vectorBytes = FloatArrayToBytes(vector.ToArray());

            // 3. Ensure the table and vector index exist
            await collection.EnsureCollectionExistsAsync();

            // 4. Save a record (assuming 'embedding' was generated via Ollama/Qwen3)
            var chunk = new DocumentChunk
            {
                Text = "arcdoc",
                Vector = vector,
                //TODO: Add PDFs filename
                FileName = "arcdoc"
            };


            //// 4. Store in SQLite
            //using (var insertCmd = new SQLiteCommand("INSERT INTO embeddings (text, vector) VALUES (@text, @vector)", conn))
            //{
            //    insertCmd.Parameters.AddWithValue("@text", text);
            //    insertCmd.Parameters.AddWithValue("@vector", vectorBytes);
            //    insertCmd.ExecuteNonQuery();
            //}

            await collection.UpsertAsync(chunk);

            _logger.LogVectorDimension(chunk.Vector.Length);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            _logger.LogCheckOllamaConfig(ModelEndpoint.AbsoluteUri, ModelName);
        }

    }


    // Helper: Convert float[] to byte[]
    static byte[] FloatArrayToBytes(float[] array)
    {
        byte[] bytes = new byte[array.Length * sizeof(float)];
        Buffer.BlockCopy(array, 0, bytes, 0, bytes.Length);
        return bytes;
    }

    // Helper: Convert byte[] to float[]
    static float[] BytesToFloatArray(byte[] bytes)
    {
        float[] array = new float[bytes.Length / sizeof(float)];
        Buffer.BlockCopy(bytes, 0, array, 0, bytes.Length);
        return array;
    }



#pragma warning restore MEAI001

}


//    var httpClient = new HttpClient();
//    PriceResult? pr = null;

//    var url = $"https://api.coinlore.net/api/ticker/?id=90";

//    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

//    var content = await httpClient.GetStringAsync(url);

//    dynamic? obj = JsonSerializer.Deserialize<dynamic>(content);

//    var list = JsonSerializer.Deserialize<List<JsonElement>>(content);
//    foreach (var element in list!)
//    {
//        pr = JsonSerializer.Deserialize<PriceResult>(element);
//        if (pr is not null)
//        {
//            string price = pr.price_usd ?? "0";
//            _logger.LogAgentResponse(price);

//        }
//        //_logger.LogAgentResponse(element.GetProperty("price_us").GetString());
//    }

/*

#pragma warning disable MAAI001
internal sealed class UnitConverterSkill : AgentClassSkill
{
    private IReadOnlyList<AgentSkillResource>? _resources;
    private IReadOnlyList<AgentSkillScript>? _scripts;

    /// <inheritdoc/>
    public override AgentSkillFrontmatter Frontmatter { get; } = new(
        "unit-converter",
        "Convert between common units using a multiplication factor. Use when asked to convert miles, kilometers, pounds, or kilograms.");

    /// <inheritdoc/>
    protected override string Instructions => """
        Use this skill when the user asks to convert between units.

        1. Review the conversion-table resource to find the factor for the requested conversion.
        2. Use the convert script, passing the value and factor from the table.
        3. Present the result clearly with both units.
        """;

    /// <inheritdoc/>
    public override IReadOnlyList<AgentSkillResource>? Resources => this._resources ??=
    [
        CreateResource(
            "conversion-table",
            """
            # Conversion Tables

            Formula: **result = value × factor**

            | From        | To          | Factor   |
            |-------------|-------------|----------|
            | miles       | kilometers  | 1.60934  |
            | kilometers  | miles       | 0.621371 |
            | pounds      | kilograms   | 0.453592 |
            | kilograms   | pounds      | 2.20462  |
            """),
    ];

    /// <inheritdoc/>
    public override IReadOnlyList<AgentSkillScript>? Scripts => this._scripts ??=
    [
        CreateScript("convert", ConvertUnits),
    ];

    private static string ConvertUnits(double value, double factor)
    {
        double result = Math.Round(value * factor, 4);
        return JsonSerializer.Serialize(new { value, factor, result });
    }
}
#pragma warning restore MAAI001
*/