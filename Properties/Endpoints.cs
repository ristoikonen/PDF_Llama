using System;

namespace Agent_Ollama;
internal static class Endpoints
{
    public static readonly Uri ModelEndpointUri = new Uri("http://localhost:11434");
    public const string ModelEndpoint = "http://localhost:11434";
    public const string ModelId = "llama3.2";
}
