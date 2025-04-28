using System.Collections.Generic;
using System.Diagnostics;
using CSharpTest.Models;

namespace CSharpTest.Interfaces;

/// <summary>
/// Defines operations for logging information
/// </summary>
public interface ILogService
{
    /// <summary>
    /// Path to the log file
    /// </summary>
    string LogFilePath { get; set; }
    
    /// <summary>
    /// Logs an informational message
    /// </summary>
    /// <param name="message">The message to log</param>
    void LogInfo(string message);

    /// <summary>
    /// Logs an error message
    /// </summary>
    /// <param name="message">The error message to log</param>
    void LogError(string message);

    /// <summary>
    /// Logs information about a collection of processes
    /// </summary>
    /// <param name="processes">The processes to log</param>
    void LogProcesses(IEnumerable<ProcessInfo> processes);
}