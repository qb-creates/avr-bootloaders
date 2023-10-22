using System.IO.Ports;

namespace qbdude;

public sealed class FirmwareUploadUtility
{
    private readonly byte[] endingSequence = new byte[10] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
    private readonly byte[] endingSequence2 = new byte[10] { 0xFE, 0xFE, 0xFE, 0xFE, 0xFE, 0xFE, 0xFE, 0xFE, 0xFE, 0xFE };
    private SerialPort serialPort = new SerialPort();
    private string receivedData = string.Empty;
    private double totalBytes = 0;
    private double bytesTransmitted = 0;
    private Queue<byte[]> pageDataQueue = new Queue<byte[]>();
    private bool updating = false;

    private static FirmwareUploadUtility? instance;

    public static FirmwareUploadUtility Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new FirmwareUploadUtility();
            }

            return instance;
        }
    }

    private FirmwareUploadUtility() { }

    /// <summary>
    /// Opens the Com port for data communication.
    /// </summary>
    /// <param name="comPort">The COM port that we want to open</param>
    public void OpenComPort(string comPort)
    {
        Console.WriteLine($"qbdude: Opening {comPort}");

        serialPort.DataReceived += SerialPort_DataReceived;
        serialPort.PortName = comPort;
        serialPort.BaudRate = 115200;
        serialPort.Parity = Parity.None;
        serialPort.DataBits = 8;
        serialPort.StopBits = StopBits.One;

        try
        {
            serialPort.Open();
        }
        catch (ArgumentException)
        {
            Console.WriteLine("qbdude: The given port name does not start with COM/com or does not resolve to a valid serial port.");
            Console.WriteLine($"qbdude done. Thank You.\n");
            Environment.Exit(0);
        }
    }

    public async Task Update(byte[] hexFileData)
    {
        List<byte> tempByteList = new List<byte>();
        int pageCount = 0;
        double totalFlashBytes = hexFileData.Length;

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

                // tempByteList.AddRange((i == hexFileData.Length - 1) ? endingSequence2 : endingSequence);

                // tempByteList.Insert(0, Convert.ToByte((tempByteList.Count) & 0xFF));
                // tempByteList.Insert(0, Convert.ToByte((tempByteList.Count) >> 8));
                if (tempByteList.Count != 258)
                {
                    byte[] temp = new byte[258 - tempByteList.Count];
                    Array.Fill<byte>(temp, 0xFF);
                    tempByteList.AddRange(temp);
                    tempByteList.Add(0xFE);
                }
                else
                {
                    tempByteList.Add(0xFF);
                }
                // if (i == hexFileData.Length - 1)
                // {
                //     tempByteList.AddRange(endingSequence2);
                // }
                // else
                // {
                //     tempByteList.AddRange(endingSequence);
                // }

                pageDataQueue.Enqueue(tempByteList.ToArray());
                totalBytes += tempByteList.ToArray().Length;
                tempByteList.Clear();
            }
        }

        serialPort.Write("RTU\r");
        Console.WriteLine($"qbdude: Writing flash ({hexFileData.Length} bytes):\n");
        updating = true;

        while (updating)
        {
            await Task.Delay(10);
        }
    }

    // public void StartUpdate(byte[] hexFileData)
    // {
    //     List<byte> tempByteList = new List<byte>();
    //     int pageCount = 0;
    //     double totalFlashBytes = hexFileData.Length;

    //     for (int i = 0; i < hexFileData.Length; i++)
    //     {
    //         if (i % 256 == 0)
    //         {
    //             tempByteList.Add((byte)((pageCount >> 8) & 0xFF));
    //             tempByteList.Add((byte)(pageCount & 0xFF));
    //             pageCount++;
    //         }

    //         tempByteList.Add(hexFileData[i]);

    //         if ((i - 255) % 256 == 0 || i == hexFileData.Length - 1)
    //         {
    //             pageDataQueue.Enqueue(tempByteList.ToArray());
    //             totalBytes += tempByteList.ToArray().Length;
    //             tempByteList.Clear();
    //         }
    //     }

    //     serialPort.Write("RTU\r");
    //     Console.WriteLine($"qbdude: Writing flash ({hexFileData.Length} bytes):\n");
    // }

    private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        SerialPort serialPort = (SerialPort)sender;
        receivedData += serialPort.ReadExisting();

        if (receivedData.Contains("CTU"))
        {
            receivedData = string.Empty;

            ProgressBar.Instance.StartProgress("Writing");

            Thread t = new Thread(new ThreadStart(TransmitDataToMCU));
            t.Start();
        }
        else if (receivedData.Contains('\r'))
        {
            bytesTransmitted += receivedData.Length; ;
            receivedData = string.Empty;

            int percentage = (int)((bytesTransmitted / totalBytes) * 100);
            ProgressBar.Instance.UpdateProgress(percentage);
        }
        else if (receivedData.Contains("Page"))
        {
            receivedData = string.Empty;

            Thread t = new Thread(new ThreadStart(TransmitDataToMCU));
            t.Start();
        }
        else if (receivedData.Contains("Complete"))
        {
            receivedData = string.Empty;

            updating = false;
        }
    }

    private void TransmitDataToMCU()
    {
        if (pageDataQueue.Count > 0)
        {
            byte[] data = pageDataQueue.Dequeue();
            serialPort.Write(data, 0, data.Length);
        }
    }
}