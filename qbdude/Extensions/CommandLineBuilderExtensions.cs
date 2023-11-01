using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Help;
using System.CommandLine.Invocation;
using qbdude.invocation.results;
using Spectre.Console;

namespace qbdude.extensions;

/// <summary>
/// Holds a collection of extentions methods for the <see cref="CommandLineBuilder" class/>
/// </summary>
public static class CommandLineBuilderExtensions
{   
    /// <summary>
    /// Configures the command line to write error information to standard error when
    /// there are errors parsing command line input. Errors will be printed after the help section
    /// </summary>
    /// <param name="commandLineBuilder">A command line builder.</param>
    /// <param name="errorExitCode">The exit code to use when parser errors occur.</param>
    /// <returns>The same instance of CommandLineBuilder.</returns>
    public static CommandLineBuilder AddParseErrorReport(this CommandLineBuilder commandLineBuilder, int errorExitCode)
    {
        commandLineBuilder.AddMiddleware(async delegate (InvocationContext context, Func<InvocationContext, Task> next)
        {
            if (context.ParseResult.Errors.Count > 0)
            {
                AnsiConsole.Write(new FigletText("QB.DUDE").Color(Color.Green1));
                context.InvocationResult = new ParseErrorResult(errorExitCode);
                await context.ParseResult.CommandResult.Command.InvokeAsync("-h");
                return;
            }

            await next(context);
        });

        return commandLineBuilder;
    }
    
    /// <summary>
    /// Configures the application to show help when one of the specified option aliases
    /// are used on the command line.
    /// </summary>
    /// <param name="commandLineBuilder">A command line builder.</param>
    /// <param name="helpAliases">The set of aliases that can be specified on the command line to request help</param>
    /// <returns>The same instance of CommandLineBuilder.</returns>
    public static CommandLineBuilder ConfigureHelp(this CommandLineBuilder commandLineBuilder, params string[] helpAliases)
    {
        commandLineBuilder.UseHelp(helpAliases).UseHelp(ctx =>
        {
            ctx.HelpBuilder.CustomizeLayout(x =>
            {
                var helpSectionDelegate = HelpBuilder.Default.GetLayout();

                helpSectionDelegate.Prepend(s =>
                {
                    AnsiConsole.Write(new FigletText("QB.DUDE").Color(Color.Green1));
                });

                return helpSectionDelegate;
            });
        });

        return commandLineBuilder;
    }
}