using System.CommandLine.Invocation;

namespace qbdude.exceptions;

/// <summary>
/// Represents errors that occur during command execution.
/// </summary>
public class CommandException : Exception
{
    /// <summary>
    /// Invocation result
    /// </summary>
    public IInvocationResult InvocationResult { get; set; }

    /// <summary>
    /// Initializes a new instance of the CommandException class with a specified error
    /// message, exit code, and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="invocationResult">The result of a command handler invocation.</param>
    public CommandException(string message, IInvocationResult invocationResult) : base(message)
    {
        InvocationResult = invocationResult;
    }

    /// <summary>
    /// Initializes a new instance of the CommandException class with a specified error
    /// message, exit code, and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="invocationResult">The result of a command handler invocation.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
    public CommandException(string message, IInvocationResult invocationResult, Exception innerException) : base(message, innerException)
    {
        InvocationResult = invocationResult;
    }
}

/// <summary>
/// Exit code that represents the reason the program was exited.
/// </summary>
public enum ExitCode
{
    Success = 0,
    ParseError = 1,
    HexFileNotFound = 2,
    InvalidHexFile = 3,
    FailedToOpenCom = 4,
    UploadCanceled = 5,
    CommunicationError = 6,
    Error = 7
}