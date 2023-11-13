using System.Runtime.InteropServices;

namespace qbdude.ui;

/// <summary>
/// Static wrapper for the System.Console class. Has custom Write/WriteLine methods where
/// the text color and background color can be set.
/// </summary>
public static class Console
{
    private const int MF_BYCOMMAND = 0x00000000;
    private const int SC_MAXIMIZE = 0xF030;
    private const int SC_SIZE = 0xF000;

    [DllImport("kernel32.dll", ExactSpelling = true)]
    private static extern IntPtr GetConsoleWindow();

    [DllImport("user32.dll")]
    public static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);

    [DllImport("user32.dll")]
    private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

    /// <summary>
    /// Resize the console window to where all content printed to the console can be scene.
    /// </summary>
    public static void ResizeConsoleWindow()
    {
        System.Console.CursorVisible = false;

        if (OperatingSystem.IsWindows())
        {
            if (System.Console.WindowWidth < 110)
            {
                System.Console.BufferWidth = 110;
                System.Console.WindowWidth = 110;
            }
        }
    }

    /// <summary>
    /// Will enable/disable the resize and feature of the console window.
    /// </summary>
    /// <param name="bRevert">True to enable window resize and maximize. False to disable window resize and maximize.</param>
    public static void DisableResizeMenuOptions()
    {
        IntPtr handle = GetConsoleWindow();
        IntPtr sysMenu = GetSystemMenu(handle, false);

        if (handle != IntPtr.Zero)
        {
            DeleteMenu(sysMenu, SC_MAXIMIZE, MF_BYCOMMAND);
            DeleteMenu(sysMenu, SC_SIZE, MF_BYCOMMAND);//resize
        }
    }

    /// <summary>
    /// Resets the menu for the console back to it's default state
    /// </summary>
    public static void ResetConsoleMenu()
    {
        IntPtr handle = GetConsoleWindow();
        GetSystemMenu(handle, true);
        System.Console.CursorVisible = true;
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