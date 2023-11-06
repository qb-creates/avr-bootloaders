namespace qbdude.ui;

/// <summary>
/// Static wrapper for the System.Console class. Has custom Write/WriteLine methods where
/// the text color and background color can be set.
/// </summary>
public static class Console
{    
    /// <summary>
    /// Gets or sets a value indicating whether the cursor is visible.
    /// </summary>
    /// <returns> Returns true if the cursor is visible; otherwise, false.</returns>
    public static bool CursorVisible
    {
        get
        {
            if (OperatingSystem.IsWindows())
            {
                return System.Console.CursorVisible;
            }

            return false;
        }
        set
        {
            System.Console.CursorVisible = value;
        }
    }

    /// <summary>
    /// Gets or sets the row position of the cursor within the buffer area.
    /// </summary>
    /// <returns>The current position, in rows, of the cursor.</returns>
    public static int CursorRowPosition
    {
        get
        {
            return System.Console.CursorTop;
        }
        set
        {
            System.Console.CursorTop = value;
        }
    }

    /// <summary>
    /// Gets or sets the column position of the cursor within the buffer area.
    /// </summary>
    /// <returns>The current position, in columns, of the cursor.</returns>
    public static int CursorColumnPosition
    {
        get
        {
            return System.Console.CursorLeft;
        }
        set
        {
            System.Console.CursorLeft = value;
        }
    }

    /// <summary>
    /// Writes the specified string value to the standard output stream.
    /// </summary>
    /// <param name="value">The value to write.</param>
    /// <param name="textColor">The color of the text.</param>
    /// <param name="backgroundColor">The background color of the text.</param>
    public static void Write(string value, ConsoleColor textColor = ConsoleColor.White, ConsoleColor backgroundColor = ConsoleColor.Black)
    {
        SetConsoleColor(textColor, backgroundColor);
        System.Console.Write(value);
    }

    /// <summary>
    /// Writes the specified string value, followed by the current line terminator, to the standard output stream. 
    /// </summary>
    /// <param name="value">The value to write.</param>
    /// <param name="textColor">The color of the text.</param>
    /// <param name="backgroundColor">The background color of the text.</param>
    public static void WriteLine(string value, ConsoleColor textColor = ConsoleColor.White, ConsoleColor backgroundColor = ConsoleColor.Black)
    {
        SetConsoleColor(textColor, backgroundColor);
        System.Console.WriteLine(value);
    }

    /// <summary>
    /// Sets the position of the cursor.
    /// </summary>
    /// <param name="left">The column position of the cursor. Columns are numbered from left to right starting at 0.</param>
    /// <param name="top">The row position of the cursor. Rows are numbered from top to bottom starting at 0.</param>
    public static void SetCursorPosition(int left, int top)
    {
        System.Console.SetCursorPosition(left, top);
    }

    private static void SetConsoleColor(ConsoleColor textColor, ConsoleColor backgroundColor)
    {
        System.Console.ForegroundColor = textColor;
        System.Console.BackgroundColor = backgroundColor;
    }
}