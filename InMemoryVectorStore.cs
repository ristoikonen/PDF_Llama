﻿using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.InMemory;
using Microsoft.SemanticKernel.Connectors.Ollama;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Embeddings;
using OllamaSharp;
using OpenAI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable SKEXP0070
/*
#pragma warning disable SKEXP0001 // AsChatCompletionService
#pragma warning disable SKEXP0050 // Microsoft.SemanticKernel.Plugins.Web.WebSearchEnginePlugin
#pragma warning disable SKEXP0080 // Microsoft.SemanticKernel.Process.LocalRuntime
#pragma warning disable SKEXP0010 // response type
*/

namespace PDF_Llama;

// HOST OLLAMA LOCALLY - DOCKER
// https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/embedding-generation/?tabs=csharp-Ollama&pivots=programming-language-csharp


/// <summary>
/// Helper class for setting up and tearing down a <see cref="InMemoryVectorStore"/> for testing purposes.
/// </summary>
public class InMemoryVectorStoreFixture  //(if XUNIT) : IAsyncLifetime
{
    public IEmbeddingGenerator<string, Embedding<float>> EmbeddingGenerator { get; private set; }

    public InMemoryVectorStore InMemoryVectorStore { get; private set; }

    public VectorStoreCollection<Guid, DataModel>? VectorStoreRecordCollection { get; private set; }

    public string CollectionName => "records";

    // Fix for CS1061: AddJsonFile is an extension method provided by Microsoft.Extensions.Configuration.Json.
    // You need to ensure the package `Microsoft.Extensions.Configuration.Json` is installed in your project.
    // If it's not installed, add it via NuGet Package Manager or the command line:
    // dotnet add package Microsoft.Extensions.Configuration.Json

    public InMemoryVectorStoreFixture()
    {
        IConfigurationRoot configRoot = new ConfigurationBuilder()
            
            //.AddJsonFile("appsettings.Development.json", optional: true) // Ensure the optional parameter is correctly specified
            //.AddEnvironmentVariables()
            //.AddUserSecrets(Assembly.GetExecutingAssembly())
            .Build();
        //TestConfiguration.Initialize(configRoot);

        // Create an embedding generation service.
        this.EmbeddingGenerator = new OllamaApiClient(
                uriString: Endpoints.ModelEndpoint,
                defaultModel: Endpoints.ModelId
            );
        //.AsIEmbeddingGenerator();

        //OpenAIClient(TestConfiguration.OpenAI.ApiKey)
        //.GetEmbeddingClient(TestConfiguration.OpenAI.EmbeddingModelId)
        //.AsIEmbeddingGenerator();

        // Create an InMemory vector store.
        this.InMemoryVectorStore = new InMemoryVectorStore(new() { EmbeddingGenerator = this.EmbeddingGenerator });
    }

    /// <inheritdoc/>
    public async Task DisposeAsync()
    {
        if (this.VectorStoreRecordCollection is not null)
        {
            await this.VectorStoreRecordCollection.EnsureCollectionDeletedAsync().ConfigureAwait(false);
        }
        
    }

    /// <inheritdoc/>
    public async Task InitializeAsync()
    {
        this.VectorStoreRecordCollection = await InitializeRecordCollectionAsync();
    }

    #region private
    /// <summary>
    /// Initialize a <see cref="VectorStoreCollection{TKey, TRecord}"/> with a list of strings.
    /// </summary>
    private async Task<VectorStoreCollection<Guid, DataModel>> InitializeRecordCollectionAsync()
    {
        // Delegate which will create a record.
        static DataModel CreateRecord(int index, string text, ReadOnlyMemory<float> embedding)
        {
            var guid = Guid.NewGuid();
            return new()
            {
                Key = guid,
                Text = text,
                Link = $"guid://{guid}",
                Tag = index % 2 == 0 ? "Even" : "Odd",
            };
        }

        // Create a record collection from a list of strings using the provided delegate.
        string[] lines =
        [
            "Semantic Kernel is a lightweight, open-source development kit that lets you easily build AI agents and integrate the latest AI models into your C#, Python, or Java codebase. It serves as an efficient middleware that enables rapid delivery of enterprise-grade solutions.",
        "Semantic Kernel is a new AI SDK, and a simple and yet powerful programming model that lets you add large language capabilities to your app in just a matter of minutes. It uses natural language prompting to create and execute semantic kernel AI tasks across multiple languages and platforms.",
        "In this guide, you learned how to quickly get started with Semantic Kernel by building a simple AI agent that can interact with an AI service and run your code. To see more examples and learn how to build more complex AI agents, check out our in-depth samples.",
        "The Semantic Kernel extension for Visual Studio Code makes it easy to design and test semantic functions.The extension provides an interface for designing semantic functions and allows you to test them with the push of a button with your existing models and data.",
        "The kernel is the central component of Semantic Kernel.At its simplest, the kernel is a Dependency Injection container that manages all of the services and plugins necessary to run your AI application.",
        "Semantic Kernel (SK) is a lightweight SDK that lets you mix conventional programming languages, like C# and Python, with the latest in Large Language Model (LLM) AI “prompts” with prompt templating, chaining, and planning capabilities.",
        "Semantic Kernel is a lightweight, open-source development kit that lets you easily build AI agents and integrate the latest AI models into your C#, Python, or Java codebase. It serves as an efficient middleware that enables rapid delivery of enterprise-grade solutions. Enterprise ready.",
        "With Semantic Kernel, you can easily build agents that can call your existing code.This power lets you automate your business processes with models from OpenAI, Azure OpenAI, Hugging Face, and more! We often get asked though, “How do I architect my solution?” and “How does it actually work?”"

        ];
        var vectorizedSearch = await CreateCollectionFromListAsync<Guid, DataModel>(lines, CreateRecord);
        return vectorizedSearch;
    }

    /// <summary>
    /// Delegate to create a record.
    /// </summary>
    /// <typeparam name="TKey">Type of the record key.</typeparam>
    /// <typeparam name="TRecord">Type of the record.</typeparam>
    internal delegate TRecord CreateRecord<TKey, TRecord>(int index, string text, ReadOnlyMemory<float> vector) where TKey : notnull;

    /// <summary>
    /// Create a <see cref="VectorStoreCollection{TKey, TRecord}"/> from a list of strings by:
    /// 1. Creating an instance of <see cref="VectorStoreCollection{TKey, TRecord}"/>
    /// 2. Generating embeddings for each string.
    /// 3. Creating a record with a valid key for each string and it's embedding.
    /// 4. Insert the records into the collection.
    /// </summary>
    /// <param name="entries">A list of strings.</param>
    /// <param name="createRecord">A delegate which can create a record with a valid key for each string and it's embedding.</param>
    private async Task<VectorStoreCollection<TKey, TRecord>> CreateCollectionFromListAsync<TKey, TRecord>(
        string[] entries,
        CreateRecord<TKey, TRecord> createRecord)
        where TKey : notnull
        where TRecord : class
    {
        // Get and create collection if it doesn't exist.
        var collection = this.InMemoryVectorStore.GetCollection<TKey, TRecord>(this.CollectionName);
        await collection.EnsureCollectionExistsAsync().ConfigureAwait(false);

        // Create records and generate embeddings for them.
        var tasks = entries.Select((entry, i) => Task.Run(async () =>
        {
            var record = createRecord(i, entry, (await this.EmbeddingGenerator.GenerateAsync(entry).ConfigureAwait(false)).Vector);
            await collection.UpsertAsync(record).ConfigureAwait(false);
        }));
        await Task.WhenAll(tasks).ConfigureAwait(false);

        return collection;
    }

    /// <summary>
    /// Sample model class that represents a record entry.
    /// </summary>
    /// <remarks>
    /// Note that each property is decorated with an attribute that specifies how the property should be treated by the vector store.
    /// This allows us to create a collection in the vector store and upsert and retrieve instances of this class without any further configuration.
    /// </remarks>
    public sealed class DataModel
    {
        [VectorStoreKey]
        [TextSearchResultName]
        public Guid Key { get; init; }

        [VectorStoreData]
        [TextSearchResultValue]
        public string Text { get; init; }

        [VectorStoreData]
        [TextSearchResultLink]
        public string Link { get; init; }

        [VectorStoreData(IsIndexed = true)]
        public required string Tag { get; init; }

        [VectorStoreVector(1536)]
        public string Embedding => Text;
    }
    #endregion
}

