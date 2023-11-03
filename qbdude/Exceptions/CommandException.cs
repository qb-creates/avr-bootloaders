namespace qbdude.exceptions;

/// <summary>
/// Represents errors that occur during command execution.
/// </summary>
public class CommandException : Exception
{
    /// <summary>
    /// Exit code that represents the reason the program was exited.
    /// </summary>
    public ExitCode ExitCode { get; set; }

    /// <summary>
    /// Initializes a new instance of the CommandException class with a specified error
    /// message, exit code, and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="exitCode">Exit code that represents the reason the program was exited.</param>
    public CommandException(string message, ExitCode exitCode) : base(message)
    {
        ExitCode = exitCode;
    }

    /// <summary>
    /// Initializes a new instance of the CommandException class with a specified error
    /// message, exit code, and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="exitCode">Exit code that represents the reason the program was exited.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
    public CommandException(string message, ExitCode exitCode, Exception innerException) : base(message, innerException)
    {
        ExitCode = exitCode;
    }
}

/// <summary>
/// Exit code that represents the reason the program was exited.
/// </summary>
public enum ExitCode
{
    Success = 0,
    FileNotFound = 3
}