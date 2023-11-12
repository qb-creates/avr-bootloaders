using System.CommandLine.Invocation;
using qbdude.exceptions;

namespace qbdude.invocation.results;

/// <summary>
/// Upload error invocation result. Result when there is an error uploading the program data to the microcontroller.
/// </summary>
public class UploadErrorResult : IInvocationResult
{
    private ExitCode _exitCode;

    /// <summary>
    /// Initializes a new instance of the UploadErrorResult class with a specified exit code.
    /// </summary>
    /// <param name="exitCode">Exit code that represents the reason the program was exited.</param>
    public UploadErrorResult(ExitCode exitCode)
    {
        _exitCode = exitCode;
    }

    public void Apply(InvocationContext context)
    {
        context.ExitCode = (int)_exitCode;
    }
}