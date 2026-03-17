# PDF_Llama

This repository demonstrates using Ollama (local LLM) with Semantic Kernel 
to **summarize PDF documents** and generate embeddings from documents for future use. 

## Usage scenarios

- Analyse organisations PDF documents and create embeddings for search.
- Document content summarisations.
- Rapid insights of large PDF collections. Get informartion of individiual documents and sort PDF collections.

## Architecture

Uses **plugin pattern** where the Kernel is injected into the plugin methods 
when required. 
Allows the plugin to use the kernel's AI services as configured.
PDFs read using PDFPig. Besides textual content, 
helpful piggy gives us the ability to 
AI analyse documents images, links and metadata.

### Future enhancements

#### User definable analysis steps
Offer logical steps for users who create sets for analysis.
Steps can be defined using function invocation filters. 
Users are able to opt in/out of certain analysis steps 
based on documents type or content. 
For exsample: 
 - AI is instructed to exclude personal data from medical documents.
 - Images are analysed for certain document types, such as marketing materials that contains certain product/object.

#### Analysis sets
Sets could be for finacial analysis, for organisations task history. 
Set of product information, set to contain historical price fluctuations.

#### Add Aspire MCP
Use Agents https://devblogs.microsoft.com/aspire/scaling-ai-agents-with-aspire-isolation.

## Prerequisites

- .NET 10 SDK
- Ollama installed and running locally (default endpoint `http://localhost:11434`).
	- Download model, llama3.2 works fine. 
	- To download: `ollama pull llama3.2`
- Ollama access, work in progress... Will decide between NuGet packages:
  - `OllamaSharp` or `Microsoft.AI.Extensions.Ollama`
- Process by
  - `Microsoft.SemanticKernel.`

Install packages using the CLI:

```bash
dotnet add package OllamaSharp
dotnet add package Microsoft.SemanticKernel
```


## How to run

1. Start Ollama: follow Ollama docs to run the service locally.
2. Build the project:

```bash
dotnet build
```

3. Run the sample that uses the summariser:

```bash
dotnet run --project PDF_Llama.csproj
```

## Footnotes

- If you intentionally want to use an alternate Microsoft extension package, ensure you add the correct NuGet package that provides `Microsoft.Extensions.AI.Ollama` or update code to match available libraries.

## Common fix for CS0234

You may see an error:

```
CS0234: The type or namespace name 'Ollama' does not exist in the namespace 'Microsoft.Extensions.AI' (are you missing an assembly reference?)
```

This usually means the code references a namespace that isn't provided by any installed package. In this project use `OllamaSharp` types and remove `using Microsoft.Extensions.AI.Ollama;` unless you add a package that actually provides that namespace.

Steps to resolve:

1. Remove the incorrect using directive from files (for example `DotNetAI.cs` and others):
   - `using Microsoft.Extensions.AI.Ollama;` -> remove it
2. Use the `OllamaSharp` namespace: ensure files include `using OllamaSharp;` and `using OllamaSharp.Models;` where needed.
3. Install the `OllamaSharp` NuGet package: `dotnet add package OllamaSharp` and restore packages.
4. Ensure Ollama is running and the expected model is downloaded, e.g. `llama3.2`.
