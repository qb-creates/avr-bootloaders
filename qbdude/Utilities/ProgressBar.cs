namespace qbdude.utilities;

/// <summary>
/// Will create a progress bar UI element in the console. Nothing should be printed to the to the console
/// while the progress bar is active. Contains a static "Start" method that will generate a ProgressBar
/// object and return it. Progress bar should be wrapped in a using block or "Disposed" when it is no longer
/// needed.
/// </summary>
public sealed class ProgressBar : IDisposable
{
    private static readonly object s_progressBarLocker = new object();
    private static List<ProgressBar> s_progressBarList = new List<ProgressBar>();
    private static int s_cursorLastPosition = 0;

    /// <summary>
    /// Instantiates a new instance of a progress bar and displays it in the console.
    /// The timer will be started and progress can be updated by calling ProgressBar.Update.
    /// </summary>
    /// <param name="operationText">The operation that is being performed.</param>
    /// <param name="itemsToComplete">The total number of items that need to be completed.</param>
    /// <returns>Returns a new instance of a progress bar.</returns>
    public static ProgressBar Instantiate(string operationText, long itemsToComplete)
    {
        lock (s_progressBarLocker)
        {
            ProgressBar progressBar = new ProgressBar(operationText, itemsToComplete);
            s_progressBarList.Add(progressBar);

            return progressBar;
        }
    }

    private string _operationText = string.Empty;
    private string _emptyProgressContainer = "                                                  ";
    private string _filledProgressContainer = "";
    private double _elaspedTime = 0;
    private long _itemsCompleted = 0;
    private long _itemsToComplete = 0;
    private long _previousPercentage = 0;
    private readonly int _cursorTop = 0;
    private readonly int _cursorLeft = 0;
    private Timer? _progressTimer;

    private ProgressBar(string operationText, long itemsToComplete)
    {
        Console.CursorVisible = false;
        Console.Write($@"{operationText} | {_emptyProgressContainer} | 0%");

        s_cursorLastPosition = Console.CursorTop + 2;
        _cursorTop = Console.CursorTop;
        _cursorLeft = Console.CursorLeft + 4;
        _operationText = operationText;
        _itemsToComplete = itemsToComplete;
        _progressTimer = new Timer(ProgressBarTimer, null, 0, 10);
    }

    /// <summary>
    /// Releases all resources used by the current instance of ProgressBar.
    /// </summary>
    public void Dispose()
    {
        if (_progressTimer == null)
        {
            return;
        }

        lock (s_progressBarLocker)
        {
            _progressTimer.Dispose();
            s_progressBarList.Remove(this);

            if (s_progressBarList.Count == 0)
            {
                Console.SetCursorPosition(0, s_cursorLastPosition);
                Console.CursorVisible = true;
            }
        }
    }

    /// <summary>
    /// Updates the progress bar with the items that were completed. The value passed in will be added to the
    /// current to the previous ammount passed in.
    /// </summary>
    /// <param name="items">The number of new that were completed since the last Update call.</param>
    public void Update(long items)
    {
        _itemsCompleted += items;

        long percentage = (long)(100 * _itemsCompleted / _itemsToComplete);

        if (percentage % 2 != 0 || percentage == _previousPercentage || percentage > 100)
        {
            return;
        }

        for (long i = _previousPercentage; i < percentage; i += 2)
        {
            _emptyProgressContainer = _emptyProgressContainer.Remove(0, 1);
            _filledProgressContainer = _filledProgressContainer.Insert(0, " ");
        }

        lock (s_progressBarLocker)
        {
            Console.SetCursorPosition(0, _cursorTop);
            Console.Write($@"{_operationText} | ");
            Console.BackgroundColor = ConsoleColor.Green;
            Console.Write($@"{_filledProgressContainer}");
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Write($@"{_emptyProgressContainer} | {percentage}%");
        }

        _previousPercentage = percentage;
    }

    private void ProgressBarTimer(object? state)
    {
        lock (s_progressBarLocker)
        {
            Console.SetCursorPosition(_cursorLeft, _cursorTop);
            Console.Write("{0:N2}s", _elaspedTime);
        }

        _elaspedTime += 0.01;
    }
}