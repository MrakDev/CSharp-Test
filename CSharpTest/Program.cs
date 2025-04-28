using System.Threading.Tasks;
using CSharpTest.Services;
using CSharpTest.UI;
using CSharpTest.Web;

namespace CSharpTest;

internal static class Program
{
    public const int HttpServerPort = 8080;

    private static async Task Main()
    {
        var logService = new FileLogService("boost_log.txt");
        var processService = new WindowsProcessService(logService);

        var httpServer = new SimpleHttpServer(processService, logService, $"http://localhost:{HttpServerPort}/");
        var serverTask = httpServer.StartAsync();

        var consoleUi = new ConsoleUserInterface(processService, logService);
        await consoleUi.RunAsync();

        await httpServer.StopAsync();
        await serverTask;
    }
}