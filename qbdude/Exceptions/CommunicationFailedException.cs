using System.CommandLine.Invocation;

namespace qbdude.exceptions;

/// <summary>
/// The exception that is thrown when their is a communication error with the microcontroller.
/// </summary>
public class CommunicationFailedException : CommandException
{
    private const string DEFAULT_MESSAGE = "Lost communication with th emicrocontroller.";

    /// <summary>
    /// Initializes a new instance of the CommunicationFailedException class with a specified error
    /// message, exit code, and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="invocationResult">The result of a command handler invocation.</param>
    public CommunicationFailedException(IInvocationResult invocationResult) : base(DEFAULT_MESSAGE, invocationResult)
    {

    }

    /// <summary>
    /// Initializes a new instance of the CommunicationFailedException class with a specified error
    /// message, exit code, and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="invocationResult">The result of a command handler invocation.</param>
    public CommunicationFailedException(string message, IInvocationResult invocationResult) : base(message, invocationResult)
    {

    }

    /// <summary>
    /// Initializes a new instance of the CommunicationFailedException class with a specified error
    /// message, exit code, and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="invocationResult">The result of a command handler invocation.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
    public CommunicationFailedException(string message, IInvocationResult invocationResult, Exception innerException) : base(message, invocationResult, innerException)
    {

    }
}