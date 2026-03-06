# PDF_Llama

This repository demonstrates using Ollama (local LLM) with Semantic Kernel 
to summarize PDF collections and to generate embeddings. 

## Usage scenario

- Analyse organisations old PDF documents, creating embeddings for search and summarizing content.
- Users can get insight of large document collection rapidly.

## Architecture

Uses **plugin pattern** where the Kernel is injected into the plugin methods 
if they require it. This allows the plugin to use the kernel's configured AI services.

### Future enhancements

#### User definable analysis steps
Target is to offer logical analysis steps for users who can create logical analysis sets.
Steps can be defined using function invocation filters. For exsample, user might need to 
opt in/out of certain analysis steps based on document type or content.

#### Analysis sets
Sets could be for finacial analysis, for organisations task and people history, set for product information etc.


## Prerequisites

- .NET 10 SDK
- Ollama installed and running locally (default endpoint `http://localhost:11434`).
	- Download model, llama3.2 works ok. 
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

3. Run the sample that uses the summarizer (adjust program entry as needed):

```bash
dotnet run --project PDF_Llama.csproj
```

## Notes
- Will concentrate on speed, target is to AI analyse lots of PDFs; fast.
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
