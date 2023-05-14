using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Linq;

namespace qbdude;
class Program
{
    private const string FilePathIdentifier = "-F";
    private const string ComPortIdentifier = "-P";

    private static SerialPort serialPort = new SerialPort();
    private static List<byte[]> myByteList = new List<byte[]>();
    private static Queue<byte[]> myByteQueue = new Queue<byte[]>();
    private static byte[] page = new byte[2] { 0x00, 0x00 };
    private static string line = string.Empty;
    private static string filePath = string.Empty;
    private static string comPort = string.Empty;

    static void Main(string[] options)
    {
        options = new string[] { "-P", "COM3", "-F", "C:\\Users\\qbake\\Desktop\\firmware.hex" };
        if (options.Length == 0)
        {
            Console.WriteLine("TODO: Enter help text");
            return;
        }

        FilterOptions(options);
        HexReaderUtility.Instance.ReadHexFile(filePath);
        FirmwareUploadUtility.Instance.OpenComPort(comPort);
        FirmwareUploadUtility.Instance.StartUpdate(HexReaderUtility.Instance.HexFileData);
        Console.ReadLine();

        serialPort.Write("update\r");
        Console.ReadLine();
    }

    public static void FilterOptions(string[] options)
    {
        int comPortIdentifierIndex = Array.FindIndex(options, option => option == ComPortIdentifier);
        int comPortIndex = comPortIdentifierIndex + 1;

        if (comPortIdentifierIndex == -1 || comPortIndex == options.Length || Regex.IsMatch(options[comPortIndex], @"^-[FP]$"))
        {
            Console.WriteLine("No ComPort has been specified on the command line.\n Specify a ComPort using the -P option and try again.\n");
            Environment.Exit(0);
        }

        comPort = options[comPortIndex];

        int filePathIdentifierIndex = Array.FindIndex(options, option => option == FilePathIdentifier);
        int filePathIndex = filePathIdentifierIndex + 1;

        if (filePathIdentifierIndex == -1 || filePathIndex == options.Length || Regex.IsMatch(options[filePathIndex], @"^-[FP]$"))
        {
            Console.WriteLine("A file path to a hex file has not been specified on the command line.\n Specify a file path using the -F option and try again.\n");
            Environment.Exit(0);
        }

        filePath = options[filePathIndex];
    }
}
