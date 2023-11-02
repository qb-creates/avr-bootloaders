using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Help;
using System.CommandLine.Invocation;
using System.Reflection;
using qbdude.invocation.results;
using Spectre.Console;

namespace qbdude.extensions;

/// <summary>
/// Holds a collection of extentions methods for the <see cref="CommandLineBuilder" class/>
/// </summary>
public static class CommandLineBuilderExtensions
{
    /// <summary>
    /// Prints the header when commands are triggered. 
    /// </summary>
    /// <param name="commandLineBuilder">A command line builder.</param>
    /// <returns></returns>
    public static CommandLineBuilder PrintHeaderForCommands(this CommandLineBuilder commandLineBuilder)
    {
        commandLineBuilder.AddMiddleware(async delegate (InvocationContext context, Func<InvocationContext, Task> next)
         {
             PrintHeader();
             await next(context);
         }, MiddlewareOrder.Configuration);

        return commandLineBuilder;
    }

    /// <summary>
    /// Prints the footer after commands are triggered. 
    /// </summary>
    /// <param name="commandLineBuilder">A command line builder.</param>
    /// <returns></returns>
    public static CommandLineBuilder PrintFooterForCommands(this CommandLineBuilder commandLineBuilder)
    {
        commandLineBuilder.AddMiddleware(async delegate (InvocationContext context, Func<InvocationContext, Task> next)
         {
             PrintFooter(context.ExitCode == 0);
             await next(context);
         }, MiddlewareOrder.ErrorReporting);

        return commandLineBuilder;
    }

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
                await context.ParseResult.CommandResult.Command.InvokeAsync(new string[] { "upload", "-h" });                
                context.InvocationResult = new ParseErrorResult(errorExitCode);

                PrintError(context);
                PrintFooter(false);
                return;
            }

            await next(context);
        });

        return commandLineBuilder;
    }

    /// <summary>
    /// Configures the application to show help when one of the specified option aliases
    /// are used on the command line. Prints the header before printing the help text.
    /// </summary>
    /// <param name="commandLineBuilder">A command line builder.</param>
    /// <param name="helpAliases">The set of aliases that can be specified on the command line to request help</param>
    /// <returns>The same instance of CommandLineBuilder.</returns>
    public static CommandLineBuilder ConfigureHelp(this CommandLineBuilder commandLineBuilder, params string[] helpAliases)
    {
        commandLineBuilder.UseHelp(helpAliases).UseHelp(ctx =>
        {
            ctx.HelpBuilder.CustomizeLayout(ctx =>
            {
                var helpSectionDelegate = HelpBuilder.Default.GetLayout()
                .Append(ctx => PrintFooter(true))
                .Prepend(ctx => PrintHeader());

                return helpSectionDelegate;
            });
        });

        return commandLineBuilder;
    }

    private static void PrintHeader()
    {
        var fontPath = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location); 
        var font = FigletFont.Load($"{fontPath}\\Assets\\small.flf");
        AnsiConsole.Write(new FigletText(font, "QB.DUDE").Color(Color.Green1));
    }

    private static void PrintFooter(bool success)
    {
        var textColor = success ? ConsoleColor.Green : ConsoleColor.Red;
        var successText = success ? "SUCCESS" : "FAILURE";

        // Make sure the text color is white
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write($"\r\n==============================[");

        // Update the text color of the success string
        Console.ForegroundColor = textColor;
        Console.Write($"{successText}");

        // Make sure the text color is white
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($"]====================================");
    }

    private static void PrintError(InvocationContext context)
    {
        Console.ForegroundColor = ConsoleColor.Red;

        foreach (var error in context.ParseResult.Errors)
        {
            Console.WriteLine(error);
        }

        Console.ForegroundColor = ConsoleColor.White;
    }
}