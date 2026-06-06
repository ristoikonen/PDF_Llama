using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agent_Ollama.Helpers;

public static partial class LogExtensions
{
    [LoggerMessage(
        EventId = 101,
        Level = LogLevel.Information,
        Message = "Stored embedding of collection name {collectionName}.")]
    public static partial void LogStoreEmbedding(this ILogger logger, ReadOnlyMemory<float> embedding, string collectionName);

    [LoggerMessage(
        EventId = 102,
        Level = LogLevel.Information,
        Message = "Agent {agentName} with parameter {param}...")]
    public static partial void LogAgent(this ILogger logger, string agentName, string param);

    // Response/Result logs
    [LoggerMessage(
        EventId = 106,
        Level = LogLevel.Information,
        Message = "Agent response: {response}")]
    public static partial void LogAgentResponse(this ILogger logger, string response);

    [LoggerMessage(
        EventId = 108,
        Level = LogLevel.Information,
        Message = "Vector dimension: {vectorLength}")]
    public static partial void LogVectorDimension(this ILogger logger, int vectorLength);


    // Timing logs
    [LoggerMessage(
        EventId = 110,
        Level = LogLevel.Debug,
        Message = "Initial response at {timestamp}")]
    public static partial void LogInitialResponseTime(this ILogger logger, string timestamp);
    //\"HH:mm:ss.fff\")
    [LoggerMessage(
        EventId = 111,
        Level = LogLevel.Debug,
        Message = "{msg}. Elapsed: {timestamp}")]
    public static partial void LogResponseElapsedTime(this ILogger logger, string msg, string timestamp);

    // Error logs
    [LoggerMessage(
        EventId = 112,
        Level = LogLevel.Error,
        Message = "An error occurred: {errorMessage}")]
    public static partial void LogError(this ILogger logger, string errorMessage);

    [LoggerMessage(
        EventId = 115,
        Level = LogLevel.Error,
        Message = "Check your Ollama endpoint: {endpoint} and model: {model}")]
    public static partial void LogCheckOllamaConfig(this ILogger logger, string endpoint, string model);

    // Approval request logs
    [LoggerMessage(
        EventId = 116,
        Level = LogLevel.Information,
        Message = "The agent invoking function: {functionName}")]
    public static partial void LogToolApprovalRequest(this ILogger logger, string functionName);


}


