using System;
using System.Diagnostics;
using System.Threading.Tasks;
using CSharpTest.Interfaces;

namespace CSharpTest.UI;

/// <summary>
/// Console-based user interface
/// </summary>
public class ConsoleUserInterface : IUserInterface
{
    private const int ProcessNameDisplayLength = 27;
    private readonly IProcessService _processService;
    private readonly ILogService _logService;
    private bool _keepRunning = true;

    /// <summary>
    /// Initializes a new instance of the ConsoleUserInterface class
    /// </summary>
    /// <param name="processService">The process service to use</param>
    /// <param name="logService">The log service to use</param>
    public ConsoleUserInterface(IProcessService processService, ILogService logService)
    {
        _processService = processService ?? throw new ArgumentNullException(nameof(processService));
        _logService = logService ?? throw new ArgumentNullException(nameof(logService));
    }

    /// <inheritdoc/>
    public async Task RunAsync()
    {
        Console.WriteLine(@"Process Manager Application");
        Console.WriteLine(@"==========================");
        Console.WriteLine(@$"HTTP server started. Visit http://localhost:{Program.HttpServerPort}/ in your browser.");

        while (_keepRunning)
        {
            try
            {
                DisplayProcessList();
                await ProcessUserInputAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(@$"Error: {ex.Message}");
                _logService.LogError(ex.Message);
            }

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
            Console.Clear();
        }
    }

    /// <summary>
    /// Displays the list of running processes
    /// </summary>
    private void DisplayProcessList()
    {
        var separator = new string('-', 111);

        
        Console.WriteLine("\nCurrent Running Processes:");
        Console.WriteLine(separator);
        Console.WriteLine("| {0,-30} | {1,-10} | {2,-15} | {3,-20} | {4,-20} |",
            "Process Name", "PID", "Memory (MB)", "Total CPU Time", "Start Time");
        Console.WriteLine(separator);

        var processes = _processService.GetAllProcessesBy(x => x.StartTime);
        foreach (var process in processes)
        {
            Console.WriteLine(@"| {0,-30} | {1,-10} | {2,-15:N2} | {3,-20:hh\:mm\:ss\.fff} | {4,-20:yyyy-MM-dd HH:mm:ss} |",
                process.Name.Length > ProcessNameDisplayLength + 3
                    ? process.Name[..ProcessNameDisplayLength] + "..."
                    : process.Name,
                process.Id,
                process.MemoryUsageMb, process.CpuTime, process.StartTime);
        }

        Console.WriteLine(separator);
    }

    /// <summary>
    /// Processes user input
    /// </summary>
    private async Task ProcessUserInputAsync()
    {
        Console.WriteLine("\nEnter a PID to boost priority (or 'q' to quit): ");
        var input = Console.ReadLine() ?? string.Empty;

        if (input.Equals("q", StringComparison.CurrentCultureIgnoreCase))
        {
            _keepRunning = false;
            return;
        }

        if (int.TryParse(input, out var pid))
        {
            var success = _processService.SetProcessPriority(pid, ProcessPriorityClass.High);
            
            if (success)
            {
                Console.WriteLine(@$"Successfully boosted process with PID: {pid} to High priority");
                Console.WriteLine(@"Action logged to boost_log.txt");
            }
            else
            {
                Console.WriteLine(@$"Failed to boost process with PID: {pid}");
            }
        }
        else
        {
            Console.WriteLine(@"Invalid PID format. Please enter a valid number.");
        }

        await Task.CompletedTask;
    }
}