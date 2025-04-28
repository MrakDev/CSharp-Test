using System;

namespace CSharpTest.Models;

/// <summary>
/// Represents information about a system process
/// </summary>
public class ProcessInfo
{
    /// <summary>
    /// The process identifier
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// The name of the process
    /// </summary>
    public string Name { get; set; } = null!;
    
    /// <summary>
    /// Memory usage in megabytes
    /// </summary>
    public double MemoryUsageMb { get; set; }

    /// <summary>
    /// Total CPU time used by the process
    /// </summary>
    public TimeSpan CpuTime { get; set; }
    
    /// <summary>
    /// Start time of the process
    /// </summary>
    public DateTime StartTime { get; set; }
}