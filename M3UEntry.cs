using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;



namespace Agent_Llama;

public class M3UEntry
{
    public string? Title { get; set; }
    public string? Artist { get; set; }
    public int DurationInSeconds { get; set; }
    public string Path { get; set; }
}

public class M3UParser
{
    public List<M3UEntry> Parse(string filePath)
    {
        var playlist = new List<M3UEntry>();
        string[] lines = File.ReadAllLines(filePath);

        // Regex to capture: #EXTINF:<duration>,<artist> - <title>
        // Note: Some M3Us use commas or dashes differently; this is a standard pattern.
        var regex = new Regex(@"#EXTINF:(?<duration>-?\1\d+),(?<artist>.+?)\s-\s(?<title>.+)");

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].Trim();

            if (line.StartsWith("#EXTINF:"))
            {
                var match = regex.Match(line);
                if (match.Success && i + 1 < lines.Length)
                {
                    playlist.Add(new M3UEntry
                    {
                        DurationInSeconds = int.Parse(match.Groups["duration"].Value),
                        Artist = match.Groups["artist"].Value.Trim(),
                        Title = match.Groups["title"].Value.Trim(),
                        Path = lines[i + 1].Trim() // The song path is the next line
                    });
                    i++; // Skip the path line in the next iteration
                }
            }
        }
        return playlist;
    }
}