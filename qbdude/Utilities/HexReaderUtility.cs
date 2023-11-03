using System.Text.RegularExpressions;
using qbdude.exceptions;

namespace qbdude.utilities;

/// <summary>
/// 
/// </summary>
public static class HexReaderUtility
{
    private const string INTEL_EOF_RECORD = ":00000001FF";
    private const int HEX_DATA_STARTING_INDEX = 9;
    private static readonly Regex byteMatcher = new Regex(@"[A-F0-9]{2}");

    /// <summary>
    /// 
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static async Task<byte[]> ReadHexFile(string filePath)
    {
        Console.WriteLine($"Reading input file '{filePath}'\n");

        List<byte> hexFileData = new List<byte>();
        long totalBytes = new FileInfo(filePath).Length;
        string? line = string.Empty;
        bool invalidHexFIle = false;

        try
        {
            using (StreamReader streamReader = new StreamReader(filePath))
            {
                await ProgressBarUtility.Instance.StartProgressBar("Reading", totalBytes);

                while ((line = streamReader.ReadLine()) != INTEL_EOF_RECORD)
                {
                    if ((streamReader.EndOfStream && line != INTEL_EOF_RECORD) || line.Length < INTEL_EOF_RECORD.Length)
                    {
                        invalidHexFIle = true;
                        break;
                    }

                    string byteCode = line.Substring(HEX_DATA_STARTING_INDEX, (line.Length - 9 - 2));
                    byte[] result = byteMatcher.Matches(byteCode).Select(match => Convert.ToByte(match.Value, 16)).ToArray();

                    hexFileData.AddRange(result);
                    ProgressBarUtility.Instance.UpdateProgressBar2(line.Length + 13);
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
        finally
        {
            await ProgressBarUtility.Instance.StopProgressBar();
        }

        if (invalidHexFIle)
        {
            throw new InvalidHexFileException("Hex file is not in the correct format. Upload canceled", ExitCode.FileNotFound);
        }

        return hexFileData.ToArray();
    }
}