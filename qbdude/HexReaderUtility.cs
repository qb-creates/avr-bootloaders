using System.Text.RegularExpressions;

namespace qbdude;

public sealed class HexReaderUtility
{
    private CancellationTokenSource source = new CancellationTokenSource();
    private double bytesTransmitted = 0;
    private double totalBytes = 0;

    private static HexReaderUtility? instance;

    public byte[] HexFileData { get; set; } = new Byte[0];

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


    public void Timer(CancellationToken cToken)
    {
        int counter = 0;

        while (true)
        {
            if (cToken.IsCancellationRequested)
            {
                return;
            }

            Console.CursorVisible = false;
            Console.SetCursorPosition(100, Console.CursorTop);
            Console.Write($@"{counter}s");
            counter++;
            Thread.Sleep(1000);
        }
    }
    public void ReadHexFile(string filePath)
    {
        try
        {
            Console.WriteLine($"qbdude: Reading input file '{filePath}'\n");
            totalBytes = new FileInfo(filePath).Length;

            using (StreamReader sr = new StreamReader(filePath))
            {
                string? line = string.Empty;
                HexFileData = new byte[0];

                ProgressBar.Instance.StartProgress(source.Token, "Reading");

                while ((line = sr.ReadLine()) != null)
                {
                    if (line != null)
                    {
                        bytesTransmitted += line.Length;
                    }
                    line = line?.Substring(9, (line.Length - 9 - 2));

                    if (!string.IsNullOrEmpty(line))
                    {
                        byte[] result = Regex.Matches(line, @"[A-F0-9]{2}").Cast<Match>().Select(item =>
                        {
                            return Convert.ToByte(item.Value, 16);
                        }).ToArray();

                        HexFileData = HexFileData.Concat(result).ToArray();

                        bytesTransmitted += line.Length;

                        int percentage = (int)((bytesTransmitted / totalBytes) * 100);
                        ProgressBar.Instance.UpdateProgress(percentage);
                    }
                    source.Cancel();
                }
            }
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine($"qbdude: can't open input file {filePath}: No such file or directory.\n");
            Console.WriteLine($"qbdude done. Thank You.\n");
            Environment.Exit(0);
        }
    }
}