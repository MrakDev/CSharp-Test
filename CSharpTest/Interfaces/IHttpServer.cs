using System.Threading.Tasks;

namespace CSharpTest.Interfaces;

/// <summary>
/// Defines operations for an HTTP server
/// </summary>
public interface IHttpServer
{
    /// <summary>
    /// Starts the HTTP server
    /// </summary>
    /// <returns>A task representing the asynchronous operation</returns>
    Task StartAsync();

    /// <summary>
    /// Stops the HTTP server
    /// </summary>
    /// <returns>A task representing the asynchronous operation</returns>
    Task StopAsync();
}