using System.IO.Ports;

namespace qbdude;

public sealed class FirmwareUploadUtility
{
    private SerialPort serialPort = new SerialPort();
    private static DateTime startTime;
    private static string progressBar = "                                                  ";
    private int previousPercentage = 0;
    private string receivedData = string.Empty;
    private double totalBytes = 0;
    private int totalFlashBytes = 0;
    private double bytesTransmitted = 0;
    private Queue<byte[]> myByteQueue = new Queue<byte[]>();

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
            Environment.Exit(0);
        }
    }

    public void StartUpdate()
    {
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
            startTime = DateTime.Now;
            Console.CursorVisible = false;
            Thread t = new Thread(new ThreadStart(TransmitDataToMCU));
            t.Start();
        }
        else if (receivedData.Contains("Page Written"))
        {
            receivedData = string.Empty;
            Thread t = new Thread(new ThreadStart(TransmitDataToMCU));
            t.Start();
        }
        else if (receivedData.Contains("Update Complete"))
        {
            Console.WriteLine($@"\nqbdude done. Thank you.");
            receivedData = string.Empty;
        }
    }

    private void TransmitDataToMCU()
    {
        if (myByteQueue.Count > 0)
        {
            byte[] data = myByteQueue.Dequeue();

            for (int i = 0; i < data.Length; i++)
            {
                serialPort.Write(data, i, 1);
                bytesTransmitted++;

                int currentPercentage = (int)((bytesTransmitted / totalBytes) * 100);

                if (currentPercentage % 2 == 0 && currentPercentage != previousPercentage)
                {
                    int nextEmptySpace = progressBar.IndexOf(" ");

                    progressBar = progressBar.Remove(nextEmptySpace, 1).Insert(nextEmptySpace, "#");
                    previousPercentage = currentPercentage;
                }

                int elaspedTime = (DateTime.Now - startTime).Seconds;
                Console.Write($@"Writing | {progressBar} | {currentPercentage}%  {elaspedTime}s");
                Console.SetCursorPosition(0, Console.CursorTop);
            }
        }
    }
}