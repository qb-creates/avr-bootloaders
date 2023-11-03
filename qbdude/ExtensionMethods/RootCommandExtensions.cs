using System.CommandLine;
using static qbdude.validators.OptionValidator;

namespace qbdude.extensions;

/// <summary>
/// Holds a collection of extentions methods for the RootCommand class/>
/// </summary>
public static class RootCommandExtensions
{
    /// <summary>
    /// Adds a comport sub command.
    /// </summary>
    /// <param name="rootCommand">Reference to the root command</param>
    /// <param name="comPortsAction">Action to invoke when the comport command is passed in.</param>
    /// <returns>The same instance RootCommand.</returns>
    public static RootCommand AddComPortsCommand(this RootCommand rootCommand, Action comPortsAction)
    {
        var getComPortsCommand = new Command("comport", "Get a list of available com ports.");

        getComPortsCommand.SetHandler(() => comPortsAction());
        rootCommand.AddCommand(getComPortsCommand);

        return rootCommand;
    }

    /// <summary>
    /// Adds a partnumber sub command.
    /// </summary>
    /// <param name="rootCommand">Reference to the root command</param>
    /// <param name="partNumbersAction">Action to invoke when the partnumber command is passed in.</param>
    /// <returns>The same instance RootCommand.</returns>
    public static RootCommand AddPartNumbersCommand(this RootCommand rootCommand, Action partNumbersAction)
    {
        var getPartNumbersCommand = new Command("partnumber", "Get a list of supported avr microcontrollers and their part number.");

        getPartNumbersCommand.SetHandler(() => partNumbersAction());
        rootCommand.AddCommand(getPartNumbersCommand);

        return rootCommand;
    }

    /// <summary>
    /// Adds an upload sub command.
    /// </summary>
    /// <param name="rootCommand">Reference to the root command</param>
    /// <param name="uploadFunc">Action to invoke when the upload command is passed in.</param>
    /// <returns>The same instance RootCommand.</returns>
    public static RootCommand AddUploadCommand(this RootCommand rootCommand, Func<string, string, string, bool, Task> uploadFunc)
    {
        var forceUploadOption = new Option<bool>("-f", "Will force upload for invalid signatures.");

        var partNumberOption = new Option<string>(name: "-p", description: "The Part Number of the microcontroller.", parseArgument: OnValidatePartNumber)
        {
            IsRequired = true,
            ArgumentHelpName = "PARTNUMBER"
        };

        var comportOption = new Option<string>(name: "-C", description: "The com port that will be opened.", parseArgument: OnValidateComPort)
        {
            IsRequired = true,
            ArgumentHelpName = "COMPORT"
        };

        var filePathOption = new Option<string>("-F", "The Filepath to the hex file.")
        {
            IsRequired = true
        };

        var uploadCommand = new Command("upload", "This command will upload the program to the microcontroller")
        {
            partNumberOption,
            comportOption,
            filePathOption,
            forceUploadOption
        };

        uploadCommand.SetHandler(async (partNumber, com, filepath, force) =>
        {
            await uploadFunc(partNumber, com, filepath, force);
        }, partNumberOption, comportOption, filePathOption, forceUploadOption);

        rootCommand.AddCommand(uploadCommand);

        return rootCommand;
    }
}