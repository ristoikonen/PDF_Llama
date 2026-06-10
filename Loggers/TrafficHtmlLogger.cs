using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OllamaSharp.Models;
using Spectre.Console;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Threading;

namespace Agent_Ollama.Loggers;

public sealed class ColorConsoleLoggerConfiguration
{
    public int EventId { get; set; }

    public Dictionary<LogLevel, ConsoleColor> LogLevelToColorMap { get; set; } = new()
    {
        [LogLevel.Information] = ConsoleColor.Green
    };
}

[UnsupportedOSPlatform("browser")]
[ProviderAlias("ColorConsole")]
public sealed class ColorConsoleLoggerProvider : ILoggerProvider
{
    private readonly IDisposable? _onChangeToken;
    private ColorConsoleLoggerConfiguration _currentConfig;
    public string _filepath = @"c:\tmp\" + Process.GetCurrentProcess().ProcessName + "_" + DateTime.Now.ToString("MMM_d_dd_hh_mm_ss") + @".html";
    private readonly ConcurrentDictionary<string, ColorConsoleLogger> _loggers =
        new(StringComparer.OrdinalIgnoreCase);

    public ColorConsoleLoggerProvider(
        IOptionsMonitor<ColorConsoleLoggerConfiguration> config)
    {
        _currentConfig = config.CurrentValue;
        _onChangeToken = config.OnChange(updatedConfig => _currentConfig = updatedConfig);
    }

    public ColorConsoleLoggerProvider(string filepath, int x = 0
    )
    {
        //this._filepath = filepath ?? _filepath;
        _currentConfig = new ColorConsoleLoggerConfiguration();
    }

    public ILogger CreateLogger(string categoryName) =>
        _loggers.GetOrAdd(categoryName, name => new ColorConsoleLogger(name, GetCurrentConfig));

    private ColorConsoleLoggerConfiguration GetCurrentConfig() => _currentConfig;

    public void Dispose()
    {
        _loggers.Clear();
        _onChangeToken?.Dispose();
    }
}


public sealed class ColorConsoleLogger(
    string name,
    Func<ColorConsoleLoggerConfiguration> getCurrentConfig) : ILogger
{

    public string _filepath = @"c:\tmp\" + Process.GetCurrentProcess().ProcessName + "_" + DateTime.Now.ToString("MMM_d_dd_hh_mm_ss") + @".html";

    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull => default!;

    public bool IsEnabled(LogLevel logLevel) =>
        getCurrentConfig().LogLevelToColorMap.ContainsKey(logLevel);

    public void LogDebug<TState>(
    LogLevel logLevel,
    EventId eventId,
    string message,
    TState state,
    Exception? exception,
    Func<TState, Exception?, string> formatter)
    {
        //if (!IsEnabled(logLevel))
        //{
        //    return;
        //}


        if (state is System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, object>> structuredData)
        {
            Console.WriteLine("--- Extracted Properties ---");
            foreach (var property in structuredData)
            {
                Console.WriteLine($" Property: {property.Key} = {property.Value}");

                // replace with message if it's the default format property
                string pkey = (property.Key == "{OriginalFormat}") ? message : property.Key;

                string html = $@"<div class=""max-w-2xl mx-auto p-6 bg-white rounded-lg shadow-sm border border-gray-100"">
                  <dl class=""grid grid-cols-1 gap-x-6 gap-y-4 sm:grid-cols-3"">
                    <!-- Row  -->
                    <dt class=""text-sm font-semibold text-gray-900 sm:col-span-1"">{pkey}</dt>
                    < dd class=""text-sm text-gray-600 sm:col-span-2 sm:mt-0"">{property.Value}</dd>
                  </dl>
                </div>";

                File.AppendAllText(_filepath, html + Environment.NewLine);
            }
        }
    }


    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        //if (!IsEnabled(logLevel))
        //{
        //    return;
        //}


        InitializeHtmlFile();

        if (state is System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, object>> structuredData)
        {
            Console.WriteLine("--- Extracted Properties ---");
            foreach (var property in structuredData)
            {
                // Skips the default full message text property string
                //if (property.Key == "{OriginalFormat}") continue;

                Console.WriteLine($" Property: {property.Key} = {property.Value}");
                string pkey = property.Key == "{OriginalFormat}" ? "CoinPrices Agent" : property.Key;

                string html = $@"<div class=""max-w-2xl mx-auto p-6 bg-white rounded-lg shadow-sm border border-gray-100"">
                  <dl class=""grid grid-cols-1 gap-x-6 gap-y-4 sm:grid-cols-3"">
                    <!-- Row  -->
                    <dt class=""text-sm font-semibold text-gray-900 sm:col-span-1"">{pkey}</dt>
                    <dd class=""text-sm text-gray-600 sm:col-span-2 sm:mt-0"">{property.Value}</dd>

                  </dl>
                </div>";

                File.AppendAllText(_filepath, html + Environment.NewLine);

            }
            Console.WriteLine("----------------------------");
        }

        ColorConsoleLoggerConfiguration config = getCurrentConfig();
        if (config.EventId == 0 || config.EventId == eventId.Id)
        {
            ConsoleColor originalColor = Console.ForegroundColor;

            //Console.ForegroundColor = config.LogLevelToColorMap[logLevel];
            //Console.WriteLine($"[{eventId.Id,2}: {logLevel,-12}]");

            //Console.ForegroundColor = originalColor;
            //Console.Write($"     {name} - ");

            //Console.ForegroundColor = config.LogLevelToColorMap[logLevel];
            //Console.Write($"{formatter(state, exception)}");

            //Console.ForegroundColor = originalColor;
            //Console.WriteLine();
        }
    }

    private void InitializeHtmlFile()
    {

            if (!File.Exists(_filepath))
            {
                string header = @"<!DOCTYPE html>
                    <html>
                    <head>
                        <title>Logs</title>
                        <script src=""https://cdn.tailwindcss.com""></script>
                    </head>
                    <body>
                        <h2 class=""text-lg font-medium text-gray-900"">Coin Log</h2>
                      
            ";

                //lock (_lock)
                //{
                    File.AppendAllText(_filepath, header + Environment.NewLine);
                //}
            }
        }
    //}
}


[ProviderAlias("TrafficHtml")]
public sealed class TrafficAgentHtmlLoggerProvider : ILoggerProvider
{
    private readonly string _filePath;
    private readonly ConcurrentDictionary<string, HtmlLogger> _loggers = new();

    public TrafficAgentHtmlLoggerProvider(string filePath)
    {
        _filePath = filePath;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return _loggers.GetOrAdd(categoryName, name => new HtmlLogger(name, _filePath));
    }

    public void Dispose()
    {
        File.AppendAllText(_filePath, @"</div></body></html>");
        _loggers.Clear();
    }
}


public  class TrafficAgentHtmlLogger : ILogger
{
    private readonly string _categoryName;
    private readonly string _filePath;
    private static readonly object _lock = new();

    //string Header3 = $@"<h3 style='background-color: #e9ecef; padding: 10px; border-radius: 5px;'>Traffic Log</h3>";
    //string table_start = $@"<table>
    //        <tr>
    //            <th>Timestamp</th>
    //            <th>Level</th>
    //            <th>Message</th>
    //            <th>Exception</th>
    //        </tr>";

    public TrafficAgentHtmlLogger(string categoryName, string filePath)
    {
        _categoryName = categoryName ?? "";
        //TODO YYYY missing
        var filepathnow = DateTime.Now.ToString("MMM_d_dd_hh_mm_ss"); 
        var finalFilePath = Path.Combine(filePath, Process.GetCurrentProcess().ProcessName + "_" + filepathnow + @".html");
        _filePath = finalFilePath;
        InitializeHtmlFile();
    }

    // this ILogger logger, 
    public void LogAgent(string agentName, string param, string category,
        [CallerMemberName] string methodName = "", [CallerLineNumber] int lineNumber = 0)
    {
        string logRow = $@"<tr>
                <td>{Environment.TickCount}</td>
                <td>{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}</td>
                <td >{agentName}</td>
                <td>{category}\t{param}</td>
                <td>{methodName}</td>
                <td><pre></pre></td>
                </tr>";
        //lock (_lock)
        //{
        File.AppendAllText(_filePath, logRow + Environment.NewLine);
        //}
    }

    public void LogInfo(
        string message,
        [CallerLineNumber] int lineNumber = 0,
        [CallerFilePath] string filePath = "",
        [CallerMemberName] string memberName = ""
        )
    {
        // Extract only the file name from the full path
        string fileName = System.IO.Path.GetFileName(filePath);

        Console.WriteLine($"[INFO] [{fileName}:{lineNumber} -> {memberName}] {message}");
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
                        <title>Traffic Logs - " + _filePath + @"</title>
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
                        <h2>Traffic Log</h2>
                        <h4>" + _categoryName + @"</h4>
                        <table>
                            <tr>
                                <th>TickCount</td></th>
                                <th>Time</th>
                                <th>Level</th>
                                <th>Category</th>
                                <th>Message</th>
                                <th>Exception</th>
                            </tr>";

                lock (_lock)
                {
                    File.AppendAllText(_filePath, header + Environment.NewLine);
                }
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
        //if (!IsEnabled(logLevel)) return;

        string message = formatter(state, exception);

        string safeMessage = System.Web.HttpUtility.HtmlEncode(message);
        string safeCategory = System.Web.HttpUtility.HtmlEncode(_categoryName);
        string safeException = exception != null ? System.Web.HttpUtility.HtmlEncode(exception.ToString()) : "";

        string logRow = "";

        if (logLevel == LogLevel.Information)
        {
            if(eventId.Id == 1)
            { 
                logRow = $@"<tr>
                <td>{Environment.TickCount}</td>
                <td>{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}</td>
                <td class='{logLevel}'>HEAVY TRAFFIC</td>
                <td>{safeCategory}</td>
                <td>{safeMessage}</td>
                <td><pre>{safeException}</pre></td>
                </tr>";
            }
            if (eventId.Id == 2)
            {
                logRow = $@"<tr>
                <td>{Environment.TickCount}</td>
                <td>{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}</td>
                <td class='{logLevel}'>WORKAROUND TRAFFIC</td>
                <td>{safeCategory}</td>
                <td>{safeMessage}</td>
                <td><pre>{safeException}</pre></td>
                </tr>";
            }
        }
        else
        {
            logRow = $@"<tr>
            <td>{Environment.TickCount}</td>
            <td>{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}</td>
            <td class='{logLevel}'>{logLevel}</td>
            <td>{safeCategory}</td>
            <td>{safeMessage}</td>
            <td><pre>{safeException}</pre></td>
            </tr>";
        }
        //lock (_lock)
        //{
            File.AppendAllText(_filePath, logRow + Environment.NewLine);
        //}
    }

    //public void HeaderTable(string message)
    //{
    //    string safeMessage = System.Web.HttpUtility.HtmlEncode(message);

        
    //    string tableHeader = $@"<tr>
    //        <td colspan='6' style='background-color: #e9ecef; font-weight: bold; text-align: center;'>START USAGE: {safeMessage}</td>
    //        </tr>";
    //    string logRow = $@"<tr>
    //        <td>{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}</td>
    //        <td class='Information'>START USAGE</td>
    //        <td>{safeMessage}</td>
    //        <td></td>
    //        </tr>";
    //    string table_end = @"</table>";

    //    lock (_lock)
    //    {
    //        File.AppendAllText(_filePath, table_start+ Environment.NewLine+ Header3 + Environment.NewLine);
    //        File.AppendAllText(_filePath, logRow + Environment.NewLine);
    //        File.AppendAllText(_filePath, table_end + Environment.NewLine);
    //        File.AppendAllText(_filePath, table_start + Environment.NewLine);
    //    }
    //} 


    //{
    //    //return $"TrafficAgentHtmlLogger for category '{_categoryName}' writing to '{_filePath}'";
    //}

    public void Dispose()
    {
        // This runs at the end of the 'using' statement block
        //if (_writer != null)
        //{
        //    _writer.WriteLine("</table>"); // Write closing table tag
        //    _writer.WriteLine("<p>Log session ended.</p>");
        //    _writer.Dispose();

        lock (_lock)
        {
            File.AppendAllText(_filePath, @"</table></body></html>");
        }
    }
}




public static class CoinAgentHtmlLogger
{

    private static  string _categoryName = "";
    private static  string _filePath = @"c:\tmp\" + Process.GetCurrentProcess().ProcessName + "_" + DateTime.Now.ToString("MMM_d_dd_hh_mm_ss") + @".html";
    private static readonly object _lock = new();

    //string Header3 = $@"<h3 style='background-color: #e9ecef; padding: 10px; border-radius: 5px;'>Traffic Log</h3>";
    //string table_start = $@"<table>
    //        <tr>
    //            <th>Timestamp</th>
    //            <th>Level</th>
    //            <th>Message</th>
    //            <th>Exception</th>
    //        </tr>";

    public static void InitCoinAgentHtmlLogger(this ILogger logger, string categoryName, string filePath)
    {
        _categoryName = categoryName ?? "";
        //TODO YYYY missing
        var filepathnow = DateTime.Now.ToString("MMM_d_dd_hh_mm_ss");
        var finalFilePath = Path.Combine(filePath, Process.GetCurrentProcess().ProcessName + "_" + filepathnow + @".html");
        //_filePath = finalFilePath;
        InitializeHtmlFile(logger);
    }

    // this ILogger logger, 
    public static void LogAgent(this ILogger logger, string agentName, string param, string category,
        [CallerMemberName] string methodName = "", [CallerLineNumber] int lineNumber = 0)
    {
        string logRow = $@"<tr>
                <td>{Environment.TickCount}</td>
                <td>{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}</td>
                <td >{agentName}</td>
                <td>{category}\t{param}</td>
                <td>{methodName}</td>
                <td><pre></pre></td>
                </tr>";
        //lock (_lock)
        //{
        File.AppendAllText(_filePath, logRow + Environment.NewLine);
        //}
    }

    public static void LogInfo(this ILogger logger,
        string message,
        [CallerLineNumber] int lineNumber = 0,
        [CallerFilePath] string filePath = "",
        [CallerMemberName] string memberName = ""
        )
    {
        // Extract only the file name from the full path
        string fileName = System.IO.Path.GetFileName(filePath);

        Console.WriteLine($"[INFO] [{fileName}:{lineNumber} -> {memberName}] {message}");
    }

    // Ensures the file exists with a basic HTML boiler template and a CSS-styled table
    private static void InitializeHtmlFile(this ILogger logger)
    {
        lock (_lock)
        {
            if (!File.Exists(_filePath))
            {
                string header = @"<!DOCTYPE html>
                    <html>
                    <head>
                        <title>Traffic Logs - " + _filePath + @"</title>
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
                        <h2>Traffic Log</h2>
                        <h4>" + _categoryName + @"</h4>
                        <table>
                            <tr>
                                <th>TickCount</td></th>
                                <th>Time</th>
                                <th>Level</th>
                                <th>Category</th>
                                <th>Message</th>
                                <th>Exception</th>
                            </tr>";

                lock (_lock)
                {
                    File.AppendAllText(_filePath, header + Environment.NewLine);
                }
            }
        }
    }

    public static IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public static bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None; // || logLevel != LogLevel.

    public static void Log<TState>(this ILogger logger, LogLevel logLevel, EventId eventId,
        TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    //, TimeSpan timestamp )
    {
        //if (!IsEnabled(logLevel)) return;

        string message = formatter(state, exception);

        string safeMessage = System.Web.HttpUtility.HtmlEncode(message);
        string safeCategory = System.Web.HttpUtility.HtmlEncode(_categoryName);
        string safeException = exception != null ? System.Web.HttpUtility.HtmlEncode(exception.ToString()) : "";

        string logRow = "";

        if (logLevel == LogLevel.Information)
        {
            if (eventId.Id == 1)
            {
                logRow = $@"<tr>
                <td>{Environment.TickCount}</td>
                <td>{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}</td>
                <td class='{logLevel}'>HEAVY TRAFFIC</td>
                <td>{safeCategory}</td>
                <td>{safeMessage}</td>
                <td><pre>{safeException}</pre></td>
                </tr>";
            }
            if (eventId.Id == 2)
            {
                logRow = $@"<tr>
                <td>{Environment.TickCount}</td>
                <td>{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}</td>
                <td class='{logLevel}'>WORKAROUND TRAFFIC</td>
                <td>{safeCategory}</td>
                <td>{safeMessage}</td>
                <td><pre>{safeException}</pre></td>
                </tr>";
            }
        }
        else
        {
            logRow = $@"<tr>
            <td>{Environment.TickCount}</td>
            <td>{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}</td>
            <td class='{logLevel}'>{logLevel}</td>
            <td>{safeCategory}</td>
            <td>{safeMessage}</td>
            <td><pre>{safeException}</pre></td>
            </tr>";
        }
        //lock (_lock)
        //{
        File.AppendAllText(_filePath, logRow + Environment.NewLine);
        //}
    }



    public static void Dispose(this ILogger logger)
    {
        // This runs at the end of the 'using' statement block
        //if (_writer != null)
        //{
        //    _writer.WriteLine("</table>"); // Write closing table tag
        //    _writer.WriteLine("<p>Log session ended.</p>");
        //    _writer.Dispose();

        lock (_lock)
        {
            File.AppendAllText(_filePath, @"</table></body></html>");
        }
    }
}
