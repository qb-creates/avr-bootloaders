using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.IO.Ports;
using qbdude.extensions;
using qbdude.Models;

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
                    .UseVersionOption("-v")
                    .ConfigureHelp("-h")
                    .AddParseErrorReport(1)
                    .PrintHeaderForCommands()
                    .UseExceptionHandler()
                    .CancelOnProcessTermination()
                    .Build();

        return await parser.InvokeAsync(args);
    }

    static void PrintComPorts()
    {
        Console.WriteLine("\nAvailable Com Ports:");
        var serialPorts = SerialPort.GetPortNames();

        foreach (string serialPort in serialPorts)
        {
            Console.WriteLine(serialPort);
        }
    }

    static void PrintSupportedPartNumbers()
    {
        Console.WriteLine($"\n{"Name",-15}{"Part Number",-15}{"Flash Size",-20}{"Signature",-10}");

        foreach (KeyValuePair<string, Microcontroller> kvp in Microcontroller.DeviceDictionary)
        {
            var signature = String.Join("", kvp.Value.Signature);
            Console.WriteLine($"{kvp.Value.Name,-15}{kvp.Key,-15}{kvp.Value.FlashSize,-20}{signature,-20}");
        }
    }

    static async void StartUpload(string partNumber, string com, string filepath, bool force)
    {
        var selectedMCU = Microcontroller.DeviceDictionary[partNumber];
        var hexFileData = await HexReaderUtility.Instance.ReadHexFile(filepath);

        if (hexFileData.Length > selectedMCU.FlashSize)
        {
            Console.WriteLine("Selected MCU does not have enough space for this program");
            Environment.Exit(1);
        }

        FirmwareUploadUtility.Instance.OpenComPort(com);
        await FirmwareUploadUtility.Instance.Update(hexFileData);
        await ProgressBar.Instance.StopProgressBar();
        Console.WriteLine($"qbdude done. Thank you.\n");
    }
}
