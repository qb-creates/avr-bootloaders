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

namespace MCUProgrammingTool
{
    class Program
    {
        private static SerialPort serialPort;
        private static List<string> myList = new List<string>();
        private static List<byte[]> myByteList = new List<byte[]>();
        private static Queue<byte[]> myByteQueue = new Queue<byte[]>();
        private static string dataReceived = string.Empty;
        private static byte[] page = new byte[2] { 0x00, 0x00 };

        static void Main(string[] args)
        {

            String line = string.Empty;
            try
            {
                //Pass the file path and file name to the StreamReader constructor
                StreamReader sr = new StreamReader(@"C:\Users\qbake\Desktop\firmware.hex");

                //Continue to read until you reach end of file
                while (line != null)
                {
                    //write the line to console window
                    //Read the next line
                    line = sr.ReadLine();
                    line = line?.Substring(9, (line.Length - 9 - 2));

                    if (!string.IsNullOrEmpty(line))
                    {
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

                List<byte> test = new List<byte>();
                for (int i = 0; i < a.Length; i++)
                {
                    if (i != 0 && i % 256 == 0)
                    {
                        byte[] adding = page.Concat(test.ToArray()).ToArray();
                        myByteQueue.Enqueue(adding);
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

            serialPort = new SerialPort();
            serialPort.DataReceived += SerialPort_DataReceived;
            serialPort.PortName = "COM3";
            serialPort.BaudRate = 9600;
            serialPort.Parity = Parity.None;
            serialPort.DataBits = 8;
            serialPort.StopBits = StopBits.Two;
            serialPort.Open();
            serialPort.Write("update\r");
            Console.ReadLine();
        }

        private static void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            dataReceived += (sender as SerialPort).ReadExisting();

            if (dataReceived.Contains("Entering Firmware Update Mode"))
            {
                Debug.WriteLine(dataReceived);
                dataReceived = dataReceived.Replace("Entering Firmware Update Mode", "").Replace("\r", "").Replace("\n", "");
                Thread t = new Thread(new ThreadStart(FlashCode));
                t.Start();
            }
            else if (dataReceived.Contains("Page Written"))
            {
                Debug.WriteLine(dataReceived);
                dataReceived = dataReceived.Replace("Page Written", "").Replace("\r","").Replace("\n","");
                Thread t = new Thread(new ThreadStart(FlashCode));
                t.Start();
            }
            else if (dataReceived.Contains("Update Complete"))
            {
                Debug.WriteLine(dataReceived);
                dataReceived = dataReceived.Replace("Update Complete", "");
            }
        }

        private static void FlashCode()
        {
            if (myByteQueue.Count > 0)
            {
                byte[] data = myByteQueue.Dequeue();
                Debug.WriteLine("Flashing page {0}", data[1]);
                serialPort.Write(data, 0, data.Length);
            }
        }
    }
}
