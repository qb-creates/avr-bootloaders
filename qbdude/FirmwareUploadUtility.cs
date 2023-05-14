using System.IO.Ports;

namespace qbdude;

public sealed class FirmwareUploadUtility
{
    private SerialPort serialPort = new SerialPort();
    private string receivedData = string.Empty;
    private double totalBytes = 0;
    private int totalFlashBytes = 0;
    private double bytesTransmitted = 0;
    private Queue<byte[]> pageDataQueue = new Queue<byte[]>();
    private CancellationTokenSource source = new CancellationTokenSource();

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

    public void StartUpdate(byte[] hexFileData)
    {
        totalFlashBytes = hexFileData.Length;

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

        serialPort.Write("RTU\r");
    }

    private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        SerialPort serialPort = (SerialPort)sender;
        receivedData += serialPort.ReadExisting();

        if (receivedData.Contains("CTU"))
        {
            Console.WriteLine($"qbdude: Writing flash ({totalFlashBytes} bytes):\n");
            receivedData = string.Empty;

            ProgressBar.Instance.StartProgress(source.Token, "Writing");

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
            source.Cancel();
            Console.WriteLine($"\nqbdude done. Thank you.");
            receivedData = string.Empty;
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