using System.Text.RegularExpressions;

namespace qbdude;

/// <summary>
/// 
/// </summary>
public sealed class HexReaderUtility
{
    private static HexReaderUtility? instance;

    public byte[] HexFileData { get; private set; } = new byte[0];

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
        try
        {
            Console.WriteLine($"qbdude: Reading input file '{filePath}'\n");
            double totalBytes = new FileInfo(filePath).Length;
            double bytesTransmitted = 0;
            double bytesTransmitted1 = 0;


            using (StreamReader sr = new StreamReader(filePath))
            {
                var a = sr.ReadToEnd();
                var total = a.Length;

                var b = a.Substring(0, a.IndexOf("\r\n") + 2);
                a = a.Remove(0, b.Length);
            }

            using (StreamReader sr = new StreamReader(filePath))
            {
                ProgressBar.Instance.StartProgress("Reading");

                string? line = string.Empty;
                byte[] hexFileData = new byte[0];

                while ((line = sr.ReadLine()) != null)
                {
                    if (line != null)
                    {
                        bytesTransmitted += line.Length;
                        line = line.Substring(9, (line.Length - 9 - 2));
                        byte[] result = Regex.Matches(line, @"[A-F0-9]{2}").Cast<Match>().Select(match => Convert.ToByte(match.Value, 16)).ToArray();

                        hexFileData = hexFileData.Concat(result).ToArray();
                        if (String.IsNullOrEmpty(line))
                        {
                            Console.Write("sdfds");
                        }
                        int percentage = (int)((bytesTransmitted / totalBytes) * 100);
                        ProgressBar.Instance.UpdateProgress(percentage);
                    }
                }

                HexFileData = hexFileData;
                await ProgressBar.Instance.StopProgressBar();
            }
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine($"qbdude: can't open input file {filePath}: No such file or directory.\n");
            Console.WriteLine($"qbdude done. Thank You.\n");
            Environment.Exit(0);
        }
        catch (Exception e)
        {
            Console.WriteLine($"{e}.\n");
            Console.WriteLine($"qbdude done. Thank You.\n");
            Environment.Exit(0);
        }
    }
}