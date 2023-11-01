using System.CommandLine.Parsing;
using System.IO.Ports;
using qbdude.Models;

namespace qbdude;

public static class ValidatorUtility
{
    public static string ValidateComPort(ArgumentResult result)
    {
        var serialPorts = SerialPort.GetPortNames();
        var comPort = result.Tokens.Single().Value;

        if (!serialPorts.Contains(comPort))
        {
            result.ErrorMessage = "Valid comport was not entered.\r\n\r\nAvailable Com Ports:\r\n" + String.Join("\r\n", serialPorts);
            return null!;
        }

        return comPort;
    }

    public static string ValidatePartNumber(ArgumentResult result)
    {
        var partNumber = result.Tokens.Single().Value;

        if (!Microcontroller.DeviceDictionary.ContainsKey(partNumber))
        {
            result.ErrorMessage = "Please enter a supported part number. Use qbdude partnumber to get supported devices.";
            return null!;
        }

        return partNumber;
    }
}