using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System;
using System.Threading.Tasks;
using A2A;

namespace Agent_Llama
{
    public class AtoA : IOllamaBase
    {
        public AtoA(string modelEndpoint, string modelName)
        {
            ModelEndpoint = new Uri(modelEndpoint);
            ModelName = modelName;
        }

        public AtoA(Uri modelEndpoint, string modelName)
        {
            ModelEndpoint = modelEndpoint;
            ModelName = modelName;
        }

        public Uri ModelEndpoint { get; set; }
        public string ModelName { get; set; }


        public async Task CreateAgent(string input1, string input2)
        {
            // Create an Ollama agent using Microsoft.Extensions.AI.Ollama
            // Requires: dotnet add package Microsoft.Extensions.AI.Ollama --prerelease
            var chatClient = new OllamaChatClient(
                ModelEndpoint,
                ModelName);

            AIAgent agent1 = chatClient.AsAIAgent(
                instructions: "You are a helpful assistant running locally via Ollama.");

            //AIAgent agent2 = chatClient.AsAIAgent(
            //    instructions: "You are a helpful assistant running locally via Ollama.");

            var response1 = await agent1.RunAsync(input1);
            Console.WriteLine(response1);

            ////var response2 = await agent2.RunAsync(input2);
            //Console.WriteLine(response2);

            // AgentRunResponse workflowResponse = await workflowAgent.RunAsync("Write a short story about a haunted house.");
            //string url = ModelEndpoint.ToString() + @"/.well-known/agent.card.json";

            string url = ModelEndpoint.ToString() + @"/agent.card.json";

            A2ACardResolver a2ACardResolver = new(new Uri(@"http://localhost:11434"));
                        
            AIAgent agent = await a2ACardResolver.GetAIAgentAsync();

            AgentCard agentCard = await a2ACardResolver.GetAgentCardAsync();

            // Prefer HTTP+JSON protocol binding. For JSON-RPC, set PreferredBindings = [ProtocolBindingNames.JsonRpc]
            A2AClientOptions options = new()
            {
                PreferredBindings = [ProtocolBindingNames.HttpJson]
            };

            AIAgent agentc = agentCard.AsAIAgent(options: options);


            // Initialize a resolver pointing at the remote agent's host.
            A2ACardResolver resolver = new(new Uri(""));

            // Resolve the agent card and create an AIAgent in one step.
            AIAgent agentr = await resolver.GetAIAgentAsync();

            // Use the agent.
            Console.WriteLine(await agentr.RunAsync("Hello!"));

        }
    }
}
