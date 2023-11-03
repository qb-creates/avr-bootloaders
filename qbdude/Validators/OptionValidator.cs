using System.CommandLine.Parsing;
using System.IO.Ports;
using qbdude.Models;

namespace qbdude.validators;

/// <summary>
/// This class holds methoads that will be used to validate option arguments.
/// </summary>
public static class OptionValidator
{

    /// <summary>
    /// Will validate the argument passed in for the Part Number option.
    /// </summary>
    /// <param name="result">The argument result passed in for the Part Number option.</param>
    /// <returns>Returns null if the validation fails.</returns>
    public static string OnValidatePartNumber(ArgumentResult result)
    {
        var partNumber = result.Tokens.Single().Value;

        if (!Microcontroller.DeviceDictionary.ContainsKey(partNumber))
        {
            result.ErrorMessage = "Please enter a supported part number. Use qbdude partnumber to get supported devices.";
            return null!;
        }

        return partNumber;
    }

    /// <summary>
    /// Will validate the argument passed in for the Com Port option.
    /// </summary>
    /// <param name="result">The argument result passed in for the Com Port option.</param>
    /// <returns>Returns null if the validation fails.</returns>
    public static string OnValidateComPort(ArgumentResult result)
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
}