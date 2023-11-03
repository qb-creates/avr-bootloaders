using System.Text.RegularExpressions;
using qbdude.exceptions;

namespace qbdude.utilities;

/// <summary>
/// 
/// </summary>
public class HexReaderUtility
{
    private const string INTEL_EOF_RECORD = ":00000001FF";
    private readonly Regex byteMatcher = new Regex(@"[A-F0-9]{2}");

    private static HexReaderUtility? instance;

    public static HexReaderUtility Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new HexReaderUtility();
            }

            return instance;
        }
    }

    private HexReaderUtility() { }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public async Task<byte[]> ReadHexFile(string filePath)
    {
        Console.WriteLine($"Reading input file '{filePath}'\n");
        List<byte> hexFileData = new List<byte>();

        try
        {
            long totalBytes = new FileInfo(filePath).Length;

            using (StreamReader sr = new StreamReader(filePath))
            {
                await ProgressBarUtility.Instance.StartProgressBar("Reading", totalBytes);

                string? line = string.Empty;
                string? previousLine = string.Empty;
                while ((line = sr.ReadLine()) != null)
                {
                    previousLine = line;
                    line = line.Substring(9, (line.Length - 9 - 2));
                    byte[] result = byteMatcher.Matches(line).Select(match => Convert.ToByte(match.Value, 16)).ToArray();

                    hexFileData.AddRange(result);
                    ProgressBarUtility.Instance.UpdateProgressBar2(line.Length + 13);
                }

                await ProgressBarUtility.Instance.StopProgressBar();

                if (previousLine != INTEL_EOF_RECORD)
                {
                    throw new InvalidHexFileException("Hex file is not in the correct format. Upload canceled", ExitCode.FileNotFound);
                }
            }
        }
        catch (FileNotFoundException ex)
        {
            throw new HexFileNotFoundException($"\nqbdude: can't open file {filePath}: No such file or directory.\n", ExitCode.FileNotFound, ex);
        }
        catch (Exception ex)
        {
            throw new CommandException($"\n{ex.Message}.\n", ExitCode.FileNotFound, ex);
        }

        return hexFileData.ToArray();
    }
}