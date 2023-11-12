using System.CommandLine.Invocation;
using qbdude.exceptions;

namespace qbdude.invocation.results;

/// <summary>
/// Hex file error invocation result. Result when there is an error reading the hex file.
/// </summary>
public class HexFileErrorResult : IInvocationResult
{
    private ExitCode _exitCode;

    /// <summary>
    /// Initializes a new instance of the HexFileErrorResult class with a specified exit code.
    /// </summary>
    /// <param name="exitCode">Exit code that represents the reason the program was exited.</param>
    public HexFileErrorResult(ExitCode exitCode)
    {
        _exitCode = exitCode;
    }

    public void Apply(InvocationContext context)
    {
        context.ExitCode = (int)_exitCode;
    }
}