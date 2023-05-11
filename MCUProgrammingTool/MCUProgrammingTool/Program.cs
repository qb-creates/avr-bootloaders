using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Linq;


namespace MCUProgrammingTool
{
    class Program
    {
        private const string FilePathIdentifier = "-F";
        private const string ComPortIdentifier = "-P";

        private static SerialPort serialPort = new SerialPort();
        private static List<byte[]> myByteList = new List<byte[]>();
        private static Queue<byte[]> myByteQueue = new Queue<byte[]>();
        private static string receivedData = string.Empty;
        private static byte[] page = new byte[2] { 0x00, 0x00 };
        private static string line = string.Empty;
        private static double totalBytes = 0;
        private static int totalFlashBytes = 0;
        private static double bytesTransmitted = 0;
        private static int previousPercentage = 0;
        private static DateTime startTime;
        private static string progressBar = "                                                  ";
        private static string filePath = string.Empty;
        private static string comPort = string.Empty;

        static void Main(string[] options)
        {
            if (options.Length == 0)
            {
                Console.WriteLine("TODO: Enter help text");
                return;
            }

            FilterOptions(options, out string error);

            if (!string.IsNullOrEmpty(error))
            {
                Console.WriteLine(error);
                return;
            }

            ConfigureComPort();

            try
            {
                //Pass the file path and file name to the StreamReader constructor
                StreamReader sr = new StreamReader(filePath);

                while (line != null)
                {
                    line = sr.ReadLine();
                    line = line?.Substring(9, (line.Length - 9 - 2));

                    if (!string.IsNullOrEmpty(line))
                    {
                        Console.WriteLine(line);
                        byte[] result = Regex.Matches(line, @"[A-Z0-9]{2}").Cast<Match>().Select(item =>
                        {
                            return Convert.ToByte(item.Value, 16);
                        }).ToArray();

                        myByteList.Add(result);
                    }
                }

                byte[] a = new byte[0];

                foreach (byte[] data in myByteList)
                {
                    a = a.Concat(data).ToArray();
                }

                totalFlashBytes = a.Length;

                List<byte> test = new List<byte>();

                for (int i = 0; i < a.Length; i++)
                {
                    if (i != 0 && i % 256 == 0)
                    {
                        byte[] adding = page.Concat(test.ToArray()).ToArray();
                        myByteQueue.Enqueue(adding);
                        totalBytes += adding.Length;
                        test.Clear();

                        if (page[1] != 255)
                        {
                            page[1]++;
                        }
                        else
                        {
                            page[0]++;
                        }
                    }

                    test.Add(a[i]);

                    if (i == a.Length - 1)
                    {
                        byte[] adding = page.Concat(test.ToArray()).ToArray();
                        myByteQueue.Enqueue(adding);
                        totalBytes += adding.Length;
                    }
                }
                //close the file
                sr.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }

            Console.WriteLine("Start update?");
            Console.ReadLine();

         
            serialPort.Open();
            serialPort.Write("update\r");
            Console.ReadLine();
        }

        private static void FilterOptions(string[] options, out string error)
        {
            int comPortIdentifierIndex = Array.FindIndex(options, option => option == ComPortIdentifier) + 1;

            if (comPortIdentifierIndex == options.Length || Regex.IsMatch(options[comPortIdentifierIndex], @"^-[FP]$"))
            {
                error = "No ComPort has been specified on the command line.\n Specify a ComPort using the -P option and try again.\n";
                return;
            }

            comPort = options[comPortIdentifierIndex];

            int filePathIdentifierIndex = Array.FindIndex(options, option => option == FilePathIdentifier) + 1;

            if (filePathIdentifierIndex == options.Length || Regex.IsMatch(options[filePathIdentifierIndex], @"^-[FP]$"))
            {
                error = "A file path to a hex file has not been specified on the command line.\n Specify a file path using the -F option and try again.\n";
                return;
            }
            Console.WriteLine(options[filePathIdentifierIndex]);
            filePath = options[filePathIdentifierIndex];
            error = string.Empty;
        }

        private static void ConfigureComPort()
        {
            serialPort.DataReceived += SerialPort_DataReceived;
            serialPort.PortName = comPort;
            serialPort.BaudRate = 115200;
            serialPort.Parity = Parity.None;
            serialPort.DataBits = 8;
            serialPort.StopBits = StopBits.One;
        }

        private static void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            receivedData += (sender as SerialPort).ReadExisting();

            if (receivedData.Contains("Entering Firmware Update Mode"))
            {
                Console.WriteLine($@"Reading input file 'C:\Users\qbake\Desktop\firmware.hex'");
                Console.WriteLine($"Writing flash ({totalFlashBytes} bytes):\n");
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
                Console.WriteLine($@"\n{receivedData}");
                receivedData = receivedData.Replace("Update Complete", "");
            }
        }

        private static void TransmitDataToMCU()
        {
            if (myByteQueue.Count > 0)
            {
                byte[] data = myByteQueue.Dequeue();
                //Console.WriteLine("Flashing page {0}", data[1]);
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
}
