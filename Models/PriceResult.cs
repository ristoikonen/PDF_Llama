using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

//    //[JsonPropertyName("price_usd")]

namespace Agent_Ollama.Models;

public partial record PriceResult
{
    public string? id { get; init; }
    public string? name { get; init; }
    public string? symbol { get; init; }
    
    public string? price_usd { get; init; }
    public string? percent_change_1h { get; init; }
    public string? percent_change_24h { get; init; }
}
