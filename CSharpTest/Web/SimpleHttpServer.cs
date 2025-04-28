using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CSharpTest.Interfaces;
using CSharpTest.Properties;

namespace CSharpTest.Web;

/// <summary>
/// Simple HTTP server implementation
/// </summary>
public class SimpleHttpServer : IHttpServer
{
    private readonly IProcessService _processService;
    private readonly ILogService _logService;
    private readonly string _url;
    private HttpListener? _listener;
    private bool _isRunning;
    private Task? _listenTask;

    /// <summary>
    /// Initializes a new instance of the SimpleHttpServer class
    /// </summary>
    /// <param name="processService">The process service to use</param>
    /// <param name="logService">The log service to use</param>
    /// <param name="url">The URL to listen on</param>
    public SimpleHttpServer(IProcessService processService, ILogService logService, string url)
    {
        _processService = processService ?? throw new ArgumentNullException(nameof(processService));
        _logService = logService ?? throw new ArgumentNullException(nameof(logService));
        _url = url ?? throw new ArgumentNullException(nameof(url));
    }

    /// <inheritdoc/>
    public Task StartAsync()
    {
        if (_isRunning)
        {
            return Task.CompletedTask;
        }

        _listener = new HttpListener();
        _listener.Prefixes.Add(_url);

        try
        {
            _listener.Start();
            _isRunning = true;
            _listenTask = ListenAsync();
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logService.LogError($"HTTP server error: {ex.Message}");
            return Task.FromException(ex);
        }
    }

    /// <inheritdoc/>
    public async Task StopAsync()
    {
        if (!_isRunning || _listener == null)
        {
            return;
        }

        _isRunning = false;
        _listener.Stop();

        if (_listenTask != null)
        {
            await _listenTask;
        }

        _listener.Close();
    }

    /// <summary>
    /// Listens for incoming HTTP requests
    /// </summary>
    private async Task ListenAsync()
    {
        if (_listener == null)
        {
            return;
        }

        while (_isRunning)
        {
            try
            {
                var context = await _listener.GetContextAsync();

                var path = context.Request.Url?.AbsolutePath ?? "/";
                var method = context.Request.HttpMethod;

                switch (path)
                {
                    case "/":
                        if (method == "GET")
                            await ServeMainPageAsync(context);
                        else
                            Send404(context);
                        break;

                    case "/api/processes":
                        if (method == "GET")
                            await GetProcessesAsync(context);
                        else
                            Send404(context);
                        break;

                    case "/api/logs":
                        if (method == "GET")
                            await GetLogsAsync(context);
                        else
                            Send404(context);
                        break;

                    case "/api/boost":
                        if (method == "POST")
                            await BoostProcessAsync(context);
                        else
                            Send404(context);
                        break;

                    case "/api/log-top-processes":
                        if (method == "POST")
                            await LogTopProcessesAsync(context);
                        else
                            Send404(context);
                        break;

                    default:
                        Send404(context);
                        break;
                }
            }
            catch (HttpListenerException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logService.LogError($"Error handling request: {ex.Message}\n\n{ex.StackTrace}");
            }
        }
    }

    /// <summary>
    /// Serves the main HTML page
    /// </summary>
    private async Task ServeMainPageAsync(HttpListenerContext context)
    {
        var buffer = Encoding.UTF8.GetBytes(Resources.index);
        
        context.Response.ContentType = "text/html";
        context.Response.ContentLength64 = buffer.Length;
        await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        
        context.Response.Close();
    }

    /// <summary>
    /// Gets the list of processes as JSON
    /// </summary>
    private async Task GetProcessesAsync(HttpListenerContext context)
    {
        try
        {
            var processes = _processService.GetAllProcessesBy(x=>x.StartTime);
            var sb = new StringBuilder();
            sb.Append("[");

            var first = true;
            foreach (var proc in processes)
            {
                if (!first)
                    sb.Append(",");
                first = false;

                sb.Append("{");
                sb.AppendFormat("""
                                "id":{0},
                                """, proc.Id);
                sb.AppendFormat("""
                                "name":"{0}",
                                """, EscapeJson(proc.Name));
                sb.AppendFormat("""
                                "memoryUsageMb":{0},
                                """, proc.MemoryUsageMb);
                sb.AppendFormat("""
                                "cpuTime":{0}
                                """, proc.CpuTime.TotalSeconds);
                sb.Append("}");
            }

            sb.Append("]");

            var buffer = Encoding.UTF8.GetBytes(sb.ToString());
            context.Response.ContentType = "application/json";
            context.Response.ContentLength64 = buffer.Length;
            await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        }
        catch (Exception ex)
        {
            await SendErrorResponseAsync(context, ex.Message);
        }
        finally
        {
            context.Response.Close();
        }
    }

    /// <summary>
    /// Gets the contents of the log file
    /// </summary>
    private async Task GetLogsAsync(HttpListenerContext context)
    {
        try
        {
            var logContents = "No logs found.";

            if (File.Exists(_logService.LogFilePath))
            {
                logContents = File.ReadAllText(_logService.LogFilePath);
            }

            var buffer = Encoding.UTF8.GetBytes(logContents);
            
            context.Response.ContentType = "text/plain";
            context.Response.ContentLength64 = buffer.Length;
            await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        }
        catch (Exception ex)
        {
            var errorMessage = $"Error reading log file: {ex.Message}";
            var buffer = Encoding.UTF8.GetBytes(errorMessage);
            
            context.Response.ContentType = "text/plain";
            context.Response.ContentLength64 = buffer.Length;
            await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        }
        finally
        {
            context.Response.Close();
        }
    }

    /// <summary>
    /// Boosts a process priority
    /// </summary>
    private async Task BoostProcessAsync(HttpListenerContext context)
    {
        try
        {
            var pidParam = context.Request.QueryString["pid"];

            if (string.IsNullOrEmpty(pidParam) || !int.TryParse(pidParam, out var pid))
            {
                await SendJsonResponseAsync(context, false, "Invalid process ID");
                return;
            }

            var success = _processService.SetProcessPriority(pid, ProcessPriorityClass.High);

            if (success)
            {
                await SendJsonResponseAsync(context, true, $"Process {pid} priority set to High");
            }
            else
            {
                await SendJsonResponseAsync(context, false, $"Failed to set process {pid} priority");
            }
        }
        catch (Exception ex)
        {
            await SendJsonResponseAsync(context, false, ex.Message);
        }
        finally
        {
            context.Response.Close();
        }
    }

    /// <summary>
    /// Logs the top N processes by memory usage
    /// </summary>
    private async Task LogTopProcessesAsync(HttpListenerContext context)
    {
        try
        {
            var countParam = context.Request.QueryString["count"];
            var count = 5;

            if (!string.IsNullOrEmpty(countParam) && int.TryParse(countParam, out var requestedCount))
            {
                count = Clamp(requestedCount, 1, 20);
            }

            var topProcesses = _processService.GetAllProcessesBy(x => x.MemoryUsageMb).Take(count);
            _logService.LogProcesses(topProcesses);

            await SendJsonResponseAsync(context, true, $"Logged top {count} processes");
        }
        catch (Exception ex)
        {
            await SendJsonResponseAsync(context, false, ex.Message);
        }
        finally
        {
            context.Response.Close();
        }
    }

    /// <summary>
    /// Sends a JSON response
    /// </summary>
    private static async Task SendJsonResponseAsync(HttpListenerContext context, bool success, string message)
    {
        var json = $$"""{"success":{{(success ? "true" : "false")}},"message":"{{EscapeJson(message)}}"}""";
        var buffer = Encoding.UTF8.GetBytes(json);

        context.Response.ContentType = "application/json";
        context.Response.ContentLength64 = buffer.Length;
        await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
    }

    /// <summary>
    /// Sends an error response
    /// </summary>
    private static async Task SendErrorResponseAsync(HttpListenerContext context, string message)
    {
        try
        {
            var json = $$"""{"error":"{{EscapeJson(message)}}"}""";
            var buffer = Encoding.UTF8.GetBytes(json);

            Console.WriteLine(buffer.Length);

            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";
            context.Response.ContentLength64 = buffer.Length;
            await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        }
        catch
        {
            // Ignore any errors while sending the error response (e.g., if the connection is closed/aborted)
        }
    }

    /// <summary>
    /// Sends a 404 Not Found response
    /// </summary>
    private static void Send404(HttpListenerContext context)
    {
        context.Response.StatusCode = 404;
        context.Response.Close();
    }

    private static string EscapeJson(string str)
    {
        return str.Replace("\"", "\\\"").Replace("\\", "\\\\");
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int Clamp(int value, int min, int max)
    {
        if (min > max)
        {
            throw new ArgumentException("min must be less than or equal to max");
        }

        if (value < min)
        {
            return min;
        }

        if (value > max)
        {
            return max;
        }

        return value;
    }
}