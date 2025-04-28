using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CSharpTest.Interfaces;
using CSharpTest.Models;

namespace CSharpTest.Services;

public class WindowsProcessService : IProcessService
{
    private const double BytesToMegabytesConversionFactor = 1024.0 * 1024.0;
    private readonly ILogService _logService;

    /// <summary>
    ///     Initializes a new instance of the WindowsProcessService class
    /// </summary>
    /// <param name="logService">The log service to use</param>
    public WindowsProcessService(ILogService logService)
    {
        _logService = logService ?? throw new ArgumentNullException(nameof(logService));
    }

    /// <inheritdoc />
    public IReadOnlyList<ProcessInfo> GetAllProcesses()
    {
        var processes = new List<ProcessInfo>();

        try
        {
            foreach (var process in Process.GetProcesses())
                try
                {
                    processes.Add(new ProcessInfo
                    {
                        Name = process.ProcessName,
                        Id = process.Id,
                        MemoryUsageMb = process.WorkingSet64 / BytesToMegabytesConversionFactor,
                        CpuTime = process.TotalProcessorTime,
                        StartTime = process.StartTime
                    });
                }
                catch
                {
                    // Skip processes we don't have access to
                }
                finally
                {
                    process.Dispose();
                }
        }
        catch (Exception ex)
        {
            _logService.LogError($"Error getting process list: {ex.Message}");
        }

        return processes;
    }

    /// <inheritdoc />
    public IReadOnlyList<ProcessInfo> GetAllProcessesBy<T>(Func<ProcessInfo, T> selector)
    {
        var processes = GetAllProcesses();
        var sortedProcesses = processes.OrderByDescending(selector).ToList();
        return sortedProcesses;
    }

    /// <inheritdoc />
    public bool SetProcessPriority(int pid, ProcessPriorityClass priority)
    {
        Process? process = null;

        try
        {
            process = Process.GetProcessById(pid);
            process.PriorityClass = priority;

            _logService.LogInfo($"Changed process PID: {pid} ({process.ProcessName}) priority to {priority}");
            return true;
        }
        catch (ArgumentException)
        {
            _logService.LogError($"No process with PID {pid} was found");
            return false;
        }
        catch (Exception ex)
        {
            _logService.LogError($"Error changing process priority: {ex.Message}");
            return false;
        }
        finally
        {
            process?.Dispose();
        }
    }
}