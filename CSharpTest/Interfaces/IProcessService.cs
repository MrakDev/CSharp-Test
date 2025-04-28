using System;
using System.Collections.Generic;
using System.Diagnostics;
using CSharpTest.Models;

namespace CSharpTest.Interfaces;

/// <summary>
///     Defines operations for managing system processes
/// </summary>
public interface IProcessService
{
    /// <summary>
    ///     Gets all accessible running processes on the system
    /// </summary>
    /// <returns>A list of <see cref="ProcessInfo" /> objects</returns>
    IReadOnlyList<ProcessInfo> GetAllProcesses();

    /// <summary>
    ///     Gets all processes sorted by a specified selector.
    /// </summary>
    /// <param name="selector">A function that selects the value to sort processes by.</param>
    /// <returns>A list of <see cref="ProcessInfo" /> sorted by the selected value in descending order.</returns>
    IReadOnlyList<ProcessInfo> GetAllProcessesBy<T>(Func<ProcessInfo, T> selector);

    /// <summary>
    ///     Sets the priority of a process
    /// </summary>
    /// <param name="pid">Process ID</param>
    /// <param name="priority">New priority</param>
    /// <returns>True if successful, false otherwise</returns>
    bool SetProcessPriority(int pid, ProcessPriorityClass priority);
}