using System.Text.RegularExpressions;

namespace qbdude;
class Program
{
    static async Task Main(string[] options)
    {
        // options = new string[] { "-P", "COM3", "-F", "F:\\portfolio\\avr-bootloaders\\.hex\\firmware.hex" };
        
        if (options.Length == 0)
        {
            Console.WriteLine("TODO: Enter help text");
            return;
        }

        OptionFilter.Instance.FilterOptions(options);
        await HexReaderUtility.Instance.ReadHexFile(OptionFilter.Instance.filePath);
        FirmwareUploadUtility.Instance.OpenComPort(OptionFilter.Instance.comPort);
        await FirmwareUploadUtility.Instance.Update(HexReaderUtility.Instance.HexFileData);
        await ProgressBar.Instance.StopProgressBar();
        Console.WriteLine($"qbdude done. Thank you.\n");
    }
}
