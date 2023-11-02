using System.CommandLine.Invocation;

namespace qbdude.invocation.results;

public class ParseErrorResult : IInvocationResult
{
    private int _exitCode;

    public ParseErrorResult(int exitCode)
    {
        _exitCode = exitCode;
    }

    public void Apply(InvocationContext context)
    {
        context.ExitCode = _exitCode;
    }
}