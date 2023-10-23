using System.Text.RegularExpressions;

namespace qbdude;

public class OptionFilter
{
    private const string FilePathIdentifier = "-F";
    private const string ComPortIdentifier = "-P";
    
    public string filePath = string.Empty;
    public string comPort = string.Empty;

    private static OptionFilter? instance;

    public static OptionFilter Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new OptionFilter();
            }

            return instance;
        }
    }

    public void FilterOptions(string[] options)
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