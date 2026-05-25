using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.VectorData;

namespace Agent_Ollama;
/*
 
 use TODO


CREATE TABLE DocumentChunk (
    Id varchar(256) PRIMARY KEY,
    FileName NVARCHAR(256),
	Text NVARCHAR(256),
    -- Stores a vector with 1536 dimensions (common for OpenAI models)
    Vector VECTOR(1536) 
);
 */

public class DocumentChunk
{
    [VectorStoreKey]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [VectorStoreData]
    public string? FileName { get; set; }

    [VectorStoreData(IsFullTextIndexed = true)]
    public string? Text { get; set; }

    // Qwen3-embedding uses 1024 dimensions. 
    // 3072
    // CosineDistance instaed of CosineSimilarity is the standard for text embeddings.
    [VectorStoreVector(1024, DistanceFunction = "CosineDistance")]
    public ReadOnlyMemory<float> Vector { get; set; }
}