using System.Threading.Tasks;

namespace CSharpTest.Interfaces;

/// <summary>
///     Defines operations for a user interface
/// </summary>
public interface IUserInterface
{
    /// <summary>
    ///     Runs the user interface
    /// </summary>
    /// <returns>A task representing the asynchronous operation</returns>
    Task RunAsync();
}