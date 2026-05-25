using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.Connectors.InMemory;
using Microsoft.SemanticKernel.Embeddings;
using OllamaSharp;
using OllamaSharp.Models.Chat;
using OpenAI;
using OpenAI.Files;
using OpenAI.Responses;
using OpenAI.VectorStores;
using System;
using System.ClientModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Speech.Recognition;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;


namespace  Agent_Ollama;

#pragma warning disable CA1050 // Declare types in namespaces
#pragma warning disable MEAI001

public class Embed
{
    public string txt { get; set; }

    public Embed(string modelEndpoint, string modelName)
    {
        ModelEndpoint = new Uri(modelEndpoint);
        ModelName = modelName;
    }

    public Embed(Uri modelEndpoint, string modelName)
    {
        ModelEndpoint = modelEndpoint;
        ModelName = modelName;
    }

    public Uri ModelEndpoint { get; set; }
    public string ModelName { get; set; }

    private Dictionary<string, JsonElement?> stateStore = new Dictionary<string, JsonElement?>();

    public async Task CreateAgent(string input1, string input2)
    {

        var http = new System.Net.Http.HttpClient();
        http.Timeout = TimeSpan.FromSeconds(1000);

        // 1. Initialize the In-Memory Vector Store
        var vectorStore = new InMemoryVectorStore();

        // 2. Setup your Embedding Generator (Required for semantic search)
        IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator = new OllamaEmbeddingGenerator(ModelEndpoint, ModelName);

        // https://github.com/microsoft/agent-framework/blob/main/dotnet/samples/02-agents/Agents/Agent_Step10_BackgroundResponsesWithToolsAndPersistence/Program.cs

        var chatClient = new OllamaChatClient(
            ModelEndpoint,
            ModelName,
            http
        );

        // 3. Configure the Chat History Memory Provider with an in-memory store
        var memoryProvider = new InMemoryChatHistoryProvider();

        // 4. Attach the memory provider to your agent
        var agent = chatClient.AsAIAgent(
            //name: "MyPersonalAgent",
            //instructions: "You are a helpful assistant with a memory.",
            
            new ChatClientAgentOptions
            {
                ChatOptions = new()
                {
                    Instructions = "You are a helpful assistant with a memory."
                },
                // Attach the built-in in-memory provider
                ChatHistoryProvider = memoryProvider
            }
        );

        // [AIFunctionFactory.Create(ResearchSpaceFactsAsync)],

        AgentRunOptions options = new() { AllowBackgroundResponses = true };

        AgentSession session = await agent.CreateSessionAsync();

        // Start the initial run.
        AgentResponse response = await agent.RunAsync("Write a sentence about size of our galaxy.", session, options);

        await PersistAgentState(agent, session, response.ContinuationToken);

        //// Poll for background responses until complete.
        while (response.ContinuationToken is not null)
        {
            await PersistAgentState(agent, session, response.ContinuationToken);

            await Task.Delay(TimeSpan.FromSeconds(1));

            var (restoredSession, continuationToken) = await RestoreAgentState(agent);

            options.ContinuationToken = continuationToken;
            response = await agent.RunAsync(restoredSession, options);
        }

        Console.WriteLine(response.Text);


        //var embeddingGenerationService = 
        //    ollamaApiClient.AsEmbeddingGenerationService();

        //var list = await embeddingGenerationService.GenerateEmbeddingsAsync([
        //    "Hello world",
        //    "How are you?"
        //]);

        //Console.WriteLine(list.Count);

        //ITextEmbeddingGenerationService
        //OllamaApiClient openAIClient = aiProjectClient.get .GetProjectOpenAIClient((Azure.AI.Projects.OpenAI.ProjectOpenAIClientOptions?)null);
        //AgentSession session = await agent.CreateSessionAsync();
        //Console.WriteLine(await agent.RunAsync("What's graphrag?", session));

    }
    async Task PersistAgentState(AIAgent agent, AgentSession? session, ResponseContinuationToken? continuationToken)
    {
        stateStore["session"] = await agent.SerializeSessionAsync(session!);
        stateStore["continuationToken"] = JsonSerializer.SerializeToElement(continuationToken, AgentAbstractionsJsonUtilities.DefaultOptions.GetTypeInfo(typeof(ResponseContinuationToken)));
    }

    async Task<(AgentSession Session, ResponseContinuationToken? ContinuationToken)> RestoreAgentState(AIAgent agent)
    {
        JsonElement serializedSession = stateStore["session"] ?? throw new InvalidOperationException("No serialized session found in state store.");
        JsonElement? serializedToken = stateStore["continuationToken"];

        AgentSession session = await agent.DeserializeSessionAsync(serializedSession);
        ResponseContinuationToken? continuationToken = (ResponseContinuationToken?)serializedToken?.Deserialize(AgentAbstractionsJsonUtilities.DefaultOptions.GetTypeInfo(typeof(ResponseContinuationToken)));

        return (session, continuationToken);
    }

    [Description("Researches relevant space facts and scientific information for writing a science fiction novel")]
    async Task<string> ResearchSpaceFactsAsync(string topic)
    {
        Console.WriteLine($"[ResearchSpaceFacts] Researching topic: {topic}");

        // Simulate a research operation
        await Task.Delay(TimeSpan.FromSeconds(10));

        string result = topic.ToUpperInvariant() switch
        {
            var t when t.Contains("GALAXY") => "Research findings: Galaxies contain billions of stars. Uncharted galaxies may have unique stellar formations, exotic matter, and unexplored phenomena like dark energy concentrations.",
            var t when t.Contains("SPACE") || t.Contains("TRAVEL") => "Research findings: Interstellar travel requires advanced propulsion systems. Challenges include radiation exposure, life support, and navigation through unknown space.",
            var t when t.Contains("ASTRONAUT") => "Research findings: Astronauts undergo rigorous training in zero-gravity environments, emergency protocols, spacecraft systems, and team dynamics for long-duration missions.",
            _ => $"Research findings: General space exploration facts related to {topic}. Deep space missions require advanced technology, crew resilience, and contingency planning for unknown scenarios."
        };

        Console.WriteLine("[ResearchSpaceFacts] Research complete");
        return result;
    }
}
/*
internal sealed class UserInfoMemory : AIContextProvider
{
private readonly ProviderSessionState<UserInfo> _sessionState;
private readonly IChatClient _chatClient;

public UserInfoMemory(IChatClient chatClient)
{
    _sessionState = new ProviderSessionState<UserInfo>(
        _ => new UserInfo(),
        GetType().Name);
    _chatClient = chatClient;
}

protected override async ValueTask StoreAIContextAsync(
    InvokedContext context,
    CancellationToken cancellationToken = default)
{
    var userInfo = _sessionState.GetOrInitializeState(context.Session);

    if (userInfo.UserName is null
        && context.RequestMessages.Any(x => x.Role == ChatRole.User))
    {
        var result = await _chatClient.GetResponseAsync<UserInfo>(
            context.RequestMessages,
            new ChatOptions()
            {
                Instructions =
                    "Extract the user's name from the message if present."
            },
            cancellationToken: cancellationToken);

        userInfo.UserName ??= result.Result.UserName;
    }

    _sessionState.SaveState(context.Session, userInfo);
}

protected override ValueTask<AIContext> ProvideAIContextAsync(
    InvokingContext context,
    CancellationToken cancellationToken = default)
{
    var userInfo = _sessionState.GetOrInitializeState(context.Session);

    var instructions = userInfo.UserName is null
        ? "Ask the user for their name."
        : $"The user's name is {userInfo.UserName}.";

    return new ValueTask<AIContext>(
        new AIContext { Instructions = instructions });
}
}
*/