using Microsoft.Extensions.Logging;
using OllamaSharp.Models;
using Spectre.Console;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;

namespace Agent_Ollama.Loggers;





public class ResponseHtmlLogger : ILogger
{
    private readonly string _categoryName;
    private readonly string _filePath;
    private static readonly object _lock = new();

    public ResponseHtmlLogger(string categoryName, string filePath)
    {
        //TODO: use categoryName?
        _categoryName = categoryName;
        // YYYY missing
        var filepathnow = DateTime.Now.ToString("MMM_d_dd_hh_mm_ss"); 
        var finalFilePath = Path.Combine(filePath, Process.GetCurrentProcess().ProcessName + "_" + filepathnow + @".html");
        _filePath = finalFilePath;
        InitializeHtmlFile();
    }

    // Ensures the file exists with a basic HTML boiler template and a CSS-styled table
    private void InitializeHtmlFile()
    {
        lock (_lock)
        {
            if (!File.Exists(_filePath))
            {
                string header = @"<!DOCTYPE html>
                    <html>
                    <head>
                        <title>Application Logs - " + _filePath + @"</title>
                        <style>
                            body { font-family: Segoe UI, sans-serif; margin: 20px; background: #f9f9f9; }
                            table { width: 100%; border-collapse: collapse; margin-top: 20px; box-shadow: 0 2px 5px rgba(0,0,0,0.1); }
                            th, td { padding: 10px; text-align: left; border-bottom: 1px solid #ddd; }
                            th { background-color: #333; color: white; }
                            .Information { color: #28a745; font-weight: bold; }
                            .Warning { color: #ffc107; font-weight: bold; }
                            .Error { color: #dc3545; font-weight: bold; }
                            .Critical { color: #721c24; background-color: #f8d7da; font-weight: bold; }
                        </style>
                    </head>
                    <body>
                        <h2>Execution Log Stream</h2>
                        <table>
                            <tr>
                                <th>Timestamp</th>
                                <th>Time</th>
                                <th>Level</th>
                                <th>Category</th>
                                <th>Message</th>
                                <th>Exception</th>
                            </tr>";
                File.WriteAllText(_filePath, header + Environment.NewLine);
            }
        }
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None; // || logLevel != LogLevel.

    public void Log<TState>(LogLevel logLevel, EventId eventId, 
        TState state, Exception? exception, 
        Func<TState, Exception?, string> formatter)
        //, TimeSpan timestamp )
    {
        if (!IsEnabled(logLevel)) return;

        string message = formatter(state, exception);
        // HTML encode values to prevent log-injection or breaking the HTML layout
        string safeMessage = System.Web.HttpUtility.HtmlEncode(message);
        string safeCategory = System.Web.HttpUtility.HtmlEncode(_categoryName);
        string safeException = exception != null ? System.Web.HttpUtility.HtmlEncode(exception.ToString()) : "";
        
        string logRow = $@"        <tr>
            
            <td>{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}</td>
            <td class='{logLevel}'>{logLevel}</td>
            <td>{safeCategory}</td>
            <td>{safeMessage}</td>
            <td><pre>{safeException}</pre></td>

        </tr>";

        lock (_lock)
        {
            File.AppendAllText(_filePath, logRow + Environment.NewLine);
        }
    }


    public void Dispose()
    {
        // This runs at the end of the 'using' statement block
        //if (_writer != null)
        //{
        //    _writer.WriteLine("</table>"); // Write closing table tag
        //    _writer.WriteLine("<p>Log session ended.</p>");
        //    _writer.Dispose();
        

        File.AppendAllText(_filePath, @"</table></body></html>");
    }
}

