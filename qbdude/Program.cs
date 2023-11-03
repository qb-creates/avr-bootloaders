﻿using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.IO.Ports;
using qbdude.exceptions;
using qbdude.extensions;
using qbdude.Models;
using qbdude.utilities;

namespace qbdude;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("Uploader for qb.creates' bootloaders");
        rootCommand.AddUploadCommand(StartUpload)
                   .AddComPortsCommand(PrintComPorts)
                   .AddPartNumbersCommand(PrintSupportedPartNumbers);

        var parser = new CommandLineBuilder(rootCommand)
                    .ConfigureHelp("-h")
                    .AddParseErrorReport(1)
                    .PrintHeaderForCommands()
                    .CancelOnProcessTermination()
                    .UseExceptionHandler((e, ctx) =>
                    {
                        var ex = (e as CommandException);
                        Console.WriteLine(ex.Message);

                        if (ex == null)
                        {
                            ctx.ExitCode = 1;
                            return;
                        }

                        ctx.ExitCode = (int)ex.ExitCode;
                    })
                    .Build();

        var exitCode = await parser.InvokeAsync(args);

        PrintSuccessBar(exitCode == 0);

        return exitCode;
    }


    private static void PrintComPorts()
    {
        Console.WriteLine("\nAvailable Com Ports:");
        var serialPorts = SerialPort.GetPortNames();

        foreach (string serialPort in serialPorts)
        {
            Console.WriteLine(serialPort);
        }
    }

    private static void PrintSupportedPartNumbers()
    {
        Console.WriteLine($"\n{"Name",-15}{"Part Number",-15}{"Flash Size",-20}{"Signature",-10}");

        foreach (KeyValuePair<string, Microcontroller> kvp in Microcontroller.DeviceDictionary)
        {
            var signature = String.Join("", kvp.Value.Signature);
            Console.WriteLine($"{kvp.Value.Name,-15}{kvp.Key,-15}{kvp.Value.FlashSize,-20}{signature,-20}");
        }
    }

    private static async Task<int> StartUpload(string partNumber, string com, string filepath, bool force)
    {
        var selectedMCU = Microcontroller.DeviceDictionary[partNumber];
        var hexFileData = await HexReaderUtility.Instance.ReadHexFile(filepath);

        if (hexFileData.Length > selectedMCU.FlashSize)
        {
            throw new Exception("Selected MCU does not have enough space for this program");
        }

        UploadUtility.Instance.OpenComPort(com);
        await UploadUtility.Instance.Update(hexFileData);
        await ProgressBarUtility.Instance.StopProgressBar();
        Console.WriteLine($"qbdude done. Thank you.\n");
        return 0;
    }

    private static void PrintSuccessBar(bool success)
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
}
