using System.Text.RegularExpressions;

namespace qbdude;

/// <summary>
/// 
/// </summary>
public sealed class HexReaderUtility
{
    private const string INTEL_EOF_RECORD = ":00000001FF\r\n";
    private readonly Regex byteMatcher = new Regex(@"[A-F0-9]{2}");

    public byte[] HexFileData { get; private set; } = new byte[0];

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
    public async Task ReadHexFile(string filePath)
    {
        Console.WriteLine($"qbdude: Reading input file '{filePath}'\n");
        string errorMessage = String.Empty;

        try
        {
            using (StreamReader sr = new StreamReader(filePath))
            {
                var hexFileString = sr.ReadToEnd();

                if (!hexFileString.EndsWith(INTEL_EOF_RECORD))
                {
                    throw new Exception("Hex file is not in the correct format. Upload canceled");
                }

                // Remove the intel ending hex sequence from the string
                hexFileString = hexFileString.Remove(hexFileString.Length - INTEL_EOF_RECORD.Length);

                ProgressBar.Instance.StartProgressBar("Reading", hexFileString.Length);

                while (!String.IsNullOrEmpty(hexFileString))
                {
                    // Get the length of the first line of the hex data string
                    int lineLength = hexFileString.IndexOf("\r\n");

                    if (lineLength == -1)
                    {
                        throw new Exception("Hex file is not in the correct format. Upload canceled");
                    }

                    // Extract the code from the first line
                    string extractedCode = hexFileString.Substring(9, lineLength - 11);

                    // Convert the extracted code into a byte array
                    var result = byteMatcher.Matches(extractedCode).Select(match => Convert.ToByte(match.Value, 16));
                    HexFileData = HexFileData.Concat(result).ToArray();

                    hexFileString = hexFileString.Remove(0, lineLength + 2);
                    ProgressBar.Instance.UpdateProgressBar2(lineLength + 2);
                }

                await ProgressBar.Instance.StopProgressBar();
            }
        }
        catch (FileNotFoundException)
        {
            errorMessage = $"\nqbdude: can't open input file {filePath}: No such file or directory.\n";
        }
        catch (Exception e)
        {
            errorMessage = $"\n{e.Message}.\n";
        }

        if (!String.IsNullOrEmpty(errorMessage))
        {
            Console.WriteLine($"{errorMessage}qbdude done. Thank You.\n");
            Environment.Exit(0);
        }
    }
}