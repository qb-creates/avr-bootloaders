using System.Diagnostics;
using System.IO.Ports;
using qbdude.exceptions;
using qbdude.invocation.results;
using qbdude.ui;
using Console = qbdude.ui.Console;

namespace qbdude.utilities;

/// <summary>
/// Utility that is used to upload program data to a microcontroller.
/// </summary>
public static class UploadUtility
{
    private const string READY_TO_UPDATE_AKNOWLEDGEMENT = "CTU";
    private const string COMPLETE_AKNOWLEDGEMENT = "Complete";
    private const string PAGE_AKNOWLEDGEMENT = "Page";
    private const char BYTE_AKNOWLEDGEMENT = '\r';

    private static long _totalBytes = 0;
    private static Queue<byte[]> _pageDataQueue = new Queue<byte[]>();
    
    /// <summary>
    /// Will upload the program data to the microcontroller
    /// </summary>
    /// <param name="comPort"></param>
    /// <param name="hexData"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task UploadProgramData(string comPort, byte[] hexData, CancellationToken cancellationToken)
    {
        using (SerialPort serialPort = new SerialPort(comPort, 115200, Parity.None, 8, StopBits.One))
        {
            BuildPageDataQueue(hexData);
            OpenComPort(serialPort, cancellationToken);
            Console.WriteLine($"{serialPort.PortName} open: Writing flash ({hexData.Length} bytes)\r\n");
            await TransmitData(serialPort, cancellationToken);
        }
    }

    private static void BuildPageDataQueue(byte[] hexFileData)
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

                _pageDataQueue.Enqueue(tempByteList.ToArray());
                _totalBytes += tempByteList.ToArray().Length;
                tempByteList.Clear();
            }
        }
    }

    private static void OpenComPort(SerialPort serialPort, CancellationToken cancellationToken)
    {
        int openAttempts = 3;
        while (!serialPort.IsOpen)
        {
            Console.Write($"Opening {serialPort.PortName}: {openAttempts} attempts remaining.\r");

            try
            {
                serialPort.Open();
            }
            catch
            {
                --openAttempts;
            }

            if (openAttempts == 0)
            {
                throw new ComPortTimeoutException($"Failed to open {serialPort.PortName}.", new UploadErrorResult(ExitCode.FailedToOpenCom));
            }

            cancellationToken.ThrowIfCancellationRequested();
        }
    }

    private static async Task TransmitData(SerialPort serialPort, CancellationToken cancellationToken)
    {
        byte[] data = new byte[0];
        string receivedData = string.Empty;
        serialPort.Write("RTU\r");

        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        
        using (ProgressBar progressBar = new ProgressBar("Writing", _totalBytes))
        {
            await progressBar.Start();

            while (!receivedData.Contains(COMPLETE_AKNOWLEDGEMENT))
            {
                cancellationToken.ThrowIfCancellationRequested();
                receivedData += serialPort.ReadExisting();

                if (receivedData.Contains(BYTE_AKNOWLEDGEMENT))
                {
                    var byteAckLength = receivedData.Count(f => f == BYTE_AKNOWLEDGEMENT);

                    progressBar.Update(byteAckLength);
                    receivedData = receivedData.Replace(BYTE_AKNOWLEDGEMENT, '\x00');
                    stopwatch.Restart();
                }

                if (receivedData.Contains(READY_TO_UPDATE_AKNOWLEDGEMENT) || receivedData.Contains(PAGE_AKNOWLEDGEMENT))
                {
                    if (_pageDataQueue.Count > 0)
                    {
                        data = _pageDataQueue.Dequeue();
                        serialPort.Write(data, 0, data.Length);
                    }

                    receivedData = string.Empty;
                    stopwatch.Restart();
                }

                if (stopwatch.Elapsed.Seconds > 5)
                {
                    throw new CommunicationFailedException(new UploadErrorResult(ExitCode.CommunicationError));
                }
            }
        }
    }
}