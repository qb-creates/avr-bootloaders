using System.Text.RegularExpressions;
using qbdude.exceptions;
using qbdude.invocation.results;
using qbdude.ui;
using Console = qbdude.ui.Console;

namespace qbdude.utilities;

/// <summary>
/// Utility that is used to read a .hex file and extract the program data records from it.
/// </summary>
public static class HexReaderUtility
{
    private static readonly Regex s_dataMatcher = new Regex(@"[A-F0-9]{2}");
    private const string INTEL_EOF_RECORD = ":00000001FF";
    private const int HEX_RECORD_MINIMUM_LENGTH = 11;
    private const int PROGRAM_DATA_FIELD_INDEX = 9;
    private const int EOL_Length = 2;
    
    /// <summary>
    /// Will extract the program data from the specified hex file.
    /// </summary>
    /// <param name="filePath">The path to the hex file.</param>
    /// <returns>Returns an array of the extracted program data.</returns>
    public static async Task<List<byte>> ExtractProgramData(string filePath, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Reading input file '{filePath}'\r\n");
        
        if (!File.Exists(filePath))
        {
            throw new HexFileNotFoundException($"Can't open file {filePath}: No such file or directory.\r\n", new ParseErrorResult(ExitCode.HexFileNotFound), new FileNotFoundException());
        }

        List<byte> programData = new List<byte>();
        long totalBytes = new FileInfo(filePath).Length;

        using (ProgressBar progressBar = new ProgressBar("Reading", totalBytes))
        {
            await progressBar.Start();
            string[] fileRecords = File.ReadAllLines(filePath);
            
            if (fileRecords.Last() != INTEL_EOF_RECORD || fileRecords.Any(line => line.Length < HEX_RECORD_MINIMUM_LENGTH))
            {
                throw new InvalidHexFileException(new HexFileErrorResult(ExitCode.InvalidHexFile));
            }

            // Extract program data from each record
            foreach (string record in fileRecords)
            {
                cancellationToken.ThrowIfCancellationRequested();
                string dataString = record.Substring(PROGRAM_DATA_FIELD_INDEX, (record.Length - HEX_RECORD_MINIMUM_LENGTH));

                // Throw exception if the data substring is an odd number
                if (dataString.Length % 2 != 0)
                {
                    throw new InvalidHexFileException(new HexFileErrorResult(ExitCode.InvalidHexFile));
                }

                byte[] dataArray = s_dataMatcher.Matches(dataString).Select(match => Convert.ToByte(match.Value, 16)).ToArray();

                programData.AddRange(dataArray);
                progressBar.Update(record.Length + EOL_Length);
            }
        }

        return programData;
    }
}