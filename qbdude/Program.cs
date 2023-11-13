using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.IO.Ports;
using qbdude.exceptions;
using qbdude.extensions;
using qbdude.invocation.results;
using qbdude.Models;
using qbdude.utilities;
using Console = qbdude.ui.Console;

namespace qbdude;

class Program
{
    static async Task<int> Main(string[] args)
    {
        Console.ResizeConsoleWindow();
        Console.DisableResizeMenuOptions();

        var rootCommand = new RootCommand("Uploader for qb.creates' bootloaders");
        rootCommand.AddUploadCommand(StartUpload)
                   .AddComPortsCommand(PrintComPorts)
                   .AddPartNumbersCommand(PrintSupportedPartNumbers);

        var parser = new CommandLineBuilder(rootCommand)
                    .ConfigureHelp("-h")
                    .AddParseErrorReport(ExitCode.ParseError)
                    .PrintHeaderForCommands()
                    .CancelOnProcessTermination()
                    .UseExceptionHandler((e, ctx) =>
                    {
                        var ex = (e as CommandException);

                        ctx.InvocationResult = ex != null ? ex.InvocationResult : new ErrorResult();
                        Console.WriteLine(e?.Message!);
                    })
                    .Build();

        var exitCode = await parser.InvokeAsync(args);

        PrintSuccessBar(exitCode == 0);
        Console.ResetConsoleMenu();

        return exitCode;
    }

    private static async Task<ExitCode> StartUpload(string partNumber, string com, string filepath, bool force, CancellationToken token)
    {
        try
        {
            var selectedMCU = Microcontroller.DeviceDictionary[partNumber];
            var programData = await HexReaderUtility.ExtractProgramData(filepath, token);

            if (programData.Count > selectedMCU.FlashSize)
            {
                throw new Exception("Selected MCU does not have enough space for this program");
            }

            await UploadUtility.UploadProgramData(com, programData, selectedMCU, token);
            Console.WriteLine($"qbdude done. Thank you.");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Exiting");
            return ExitCode.UploadCanceled;
        }

        return ExitCode.Success;
    }

    private static void PrintComPorts()
    {
        Console.WriteLine("\r\nAvailable Com Ports:");

        foreach (string serialPort in SerialPort.GetPortNames())
        {
            Console.WriteLine(serialPort);
        }
    }

    private static void PrintSupportedPartNumbers()
    {
        Console.WriteLine($"\r\n{"Name",-15}{"Part Number",-15}{"Flash Size",-20}{"Signature",-10}");

        foreach (KeyValuePair<string, Microcontroller> kvp in Microcontroller.DeviceDictionary)
        {
            var signature = String.Join("", kvp.Value.Signature);
            Console.WriteLine($"{kvp.Value.Name,-15}{kvp.Key,-15}{kvp.Value.FlashSize,-20}{signature,-20}");
        }
    }

    private static void PrintSuccessBar(bool success)
    {
        var textColor = success ? ConsoleColor.Green : ConsoleColor.Red;
        var successText = success ? "SUCCESS" : "FAILURE";

        Console.Write($"\r\n==============================[");
        Console.Write($"{successText}", textColor: textColor);
        Console.WriteLine($"]====================================\r\n");
    }
}
