using System.CommandLine.Invocation;

namespace qbdude.exceptions;

/// <summary>
/// The exception that is thrown when the Com Port fails to open.
/// </summary>
public class ComPortTimeoutException : CommandException
{
    /// <summary>
    /// Initializes a new instance of the ComPortTimeoutException class with a specified error
    /// message, exit code, and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="invocationResult">The result of a command handler invocation.</param>
    public ComPortTimeoutException(string message, IInvocationResult invocationResult) : base(message, invocationResult)
    {

    }

    /// <summary>
    /// Initializes a new instance of the ComPortTimeoutException class with a specified error
    /// message, exit code, and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="invocationResult">The result of a command handler invocation.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
    public ComPortTimeoutException(string message, IInvocationResult invocationResult, Exception innerException) : base(message, invocationResult, innerException)
    {

    }
}