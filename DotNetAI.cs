using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Chat;
//using Microsoft.Extensions.AI.Ollama;
//using OllamaSharp;
//using OllamaSharp.Models;
using OpenAI.Images;
using System.ComponentModel;
using System.Text.Json;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;

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



    // https://github.com/microsoft/agent-framework/blob/main/dotnet/samples/02-agents/Agents/Agent_Step01_UsingFunctionToolsWithApprovals/Program.cs


    // Create a sample function tool that the agent can use.
    [Description("Get the weather for a given location.")]
    public static string GetWeather([Description("The location to get the weather for.")] string location)
        => $"The weather in {location} is ...";
    //  = @"Describe your model and it's abilities"



    public async Task UseAgent(string question)
    {
        // --- Configuration ---
        const string ollamaEndpoint = "http://localhost:11434";
        const string ollamaModel = "llama3.2";

        Console.WriteLine("Setting up OllamaChatClient as AsAIAgent...");

        var tool = AIFunctionFactory.Create(
            (string location) => $"Data for {location}",
            name: "get_location_data",
            description: "Retrieves data for a specific location"
        );
     

        try
        {
            IChatClient client = new OllamaChatClient(new Uri(ollamaEndpoint), ollamaModel);

            AIAgent agent = client.AsAIAgent(
                instructions: "You are a helpful assistant running locally via Ollama."
               // , tools: [tool]
               , tools: [new ApprovalRequiredAIFunction(AIFunctionFactory.Create(GetWeather))]
                );
                //, tools: [new ApprovalRequiredAIFunction(AIFunctionFactory.Create(GetWeather))]);


            AgentSession session = await agent.CreateSessionAsync();
            AgentResponse response = await agent.RunAsync(question, session);
            List<ToolApprovalRequestContent> approvalRequests = response.Messages.SelectMany(m => m.Contents).OfType<ToolApprovalRequestContent>().ToList();

            while (approvalRequests.Count > 0)
            {
                List<ChatMessage> userInputResponses = approvalRequests
                .ConvertAll(functionApprovalRequest =>
                {
                    Console.WriteLine($"The agent would like to invoke the following function, please reply Y to approve: Location: {((FunctionCallContent)functionApprovalRequest.ToolCall).Arguments?["location"] } , Name {((FunctionCallContent)functionApprovalRequest.ToolCall).Name}");
                    return new ChatMessage(ChatRole.User, [functionApprovalRequest.CreateResponse(Console.ReadLine()?.Equals("Y", StringComparison.OrdinalIgnoreCase) ?? false)]);
                });

                response = await agent.RunAsync(userInputResponses, session);

                approvalRequests = response.Messages.SelectMany(m => m.Contents).OfType<ToolApprovalRequestContent>().ToList();
            }

            Console.WriteLine($"\nAgent: {response}");

            //Console.WriteLine(await agent.RunAsync(question));  


        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            Console.WriteLine("Please ensure Ollama is running and the specified model is downloaded.");
            Console.WriteLine($"Check your Ollama endpoint: {ollamaEndpoint} and model: {ollamaModel}");
        }

        Console.WriteLine("Press any key to exit.");
    }

    public async Task GetResponse(string question = @"Describe your model")
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

    // Suppress MEAI001 diagnostic for evaluation-only API usage
#pragma warning disable MEAI001

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
                new OpenAI.Images.ImageGenerationOptions
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

#pragma warning restore MEAI001

}

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