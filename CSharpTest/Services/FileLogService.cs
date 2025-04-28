using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CSharpTest.Interfaces;
using CSharpTest.Models;

namespace CSharpTest.Services;

/// <summary>
///     Service for logging to a file
/// </summary>
public class FileLogService : ILogService
{
    private readonly object _lockObject = new();

    /// <summary>
    ///     Initializes a new instance of the FileLogService class
    /// </summary>
    /// <param name="logFilePath">Path to the log file</param>
    public FileLogService(string logFilePath)
    {
        LogFilePath = logFilePath ?? throw new ArgumentNullException(nameof(logFilePath));
    }

    public string LogFilePath { get; set; }

    /// <inheritdoc />
    public void LogInfo(string message)
    {
        WriteToLog($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - INFO: {message}");
    }

    /// <inheritdoc />
    public void LogError(string message)
    {
        WriteToLog($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - ERROR: {message}");
    }

    /// <inheritdoc />
    public void LogProcesses(IEnumerable<ProcessInfo> processes)
    {
        var processInfos = processes.ToList();

        var builder = new StringBuilder();
        builder.AppendLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Top {processInfos.Count} processes by memory usage:");

        foreach (var process in processInfos)
            builder.AppendLine($"  - {process.Name} (PID: {process.Id}): {process.MemoryUsageMb:N2} MB");

        WriteToLog(builder.ToString());
    }

    /// <summary>
    ///     Writes a message to the log file
    /// </summary>
    /// <param name="logEntry">The message to write</param>
    private void WriteToLog(string logEntry)
    {
        try
        {
            lock (_lockObject)
            {
                File.AppendAllText(LogFilePath, logEntry + Environment.NewLine);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(@$"Error writing to log file: {ex.Message}");
        }
    }
}