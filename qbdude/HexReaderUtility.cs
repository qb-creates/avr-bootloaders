using System.Text.RegularExpressions;

namespace qbdude;

public sealed class HexReaderUtility
{
    private Queue<byte[]> pageDataQueue = new Queue<byte[]>();
    private double totalBytes = 0;

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

    public int TotalFlash { get; set; }

    private HexReaderUtility() { }

    public void ReadHexFile(string filePath)
    {
        try
        {
            Console.WriteLine($"qbdude: Reading input file '{filePath}'");

            using (StreamReader sr = new StreamReader(filePath))
            {
                string? line = string.Empty;
                byte[] hexFileData = new byte[0];

                while ((line = sr.ReadLine()) != null)
                {
                    line = line?.Substring(9, (line.Length - 9 - 2));

                    if (!string.IsNullOrEmpty(line))
                    {
                        byte[] result = Regex.Matches(line, @"[A-F0-9]{2}").Cast<Match>().Select(item =>
                        {
                            return Convert.ToByte(item.Value, 16);
                        }).ToArray();

                        hexFileData = hexFileData.Concat(result).ToArray();
                    }
                }

                TotalFlash = hexFileData.Length;
                BuildPageQueue(hexFileData);
            }
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine($"qbdude: can't open input file {filePath}: No such file or directory.");
            Environment.Exit(0);
        }

    }

    public void BuildPageQueue(byte[] hexFileData)
    {
        List<byte> tempByteList = new List<byte>();

        int pageCount = 0;

        for (int i = 0; i < hexFileData.Length; i++)
        {
            if (i % 256 == 0)
            {
                tempByteList.Add((byte)((pageCount >> 8) & 0xFF));
                tempByteList.Add((byte)(pageCount & 0xFF));
                pageCount++;
            }

            tempByteList.Add(hexFileData[i]);

            if ((i - 255) % 256 == 0 || i == hexFileData.Length - 1)
            {
                pageDataQueue.Enqueue(tempByteList.ToArray());
                totalBytes += tempByteList.ToArray().Length;
                tempByteList.Clear();
            }
        }
    }
}