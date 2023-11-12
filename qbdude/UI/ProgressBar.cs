namespace qbdude.ui;

/// <summary>
/// Will create a progress bar UI element in the console. Nothing should be printed to the to the console
/// while the progress bar is active. Contains a static "Start" method that will generate a ProgressBar
/// object and return it. Progress bar should be wrapped in a using block or "Disposed" when it is no longer
/// needed.
/// </summary>
public sealed class ProgressBar : IDisposable
{
    private readonly object _progressBarLocker = new object();

    private string _operationText = string.Empty;
    private string _emptyProgressContainer = "                                                  ";
    private string _filledProgressContainer = "";
    private int _timeColumnPosition = 0;
    private double _elaspedTime = 0;
    private long _itemsCompleted = 0;
    private long _itemsToComplete = 0;
    private long _previousPercentage = 0;
    private Timer? _progressTimer;
    private bool _isActive = false;
    private bool _disposed = false;

    /// <summary>
    /// Initializes  a new instance of a ProgressBar class and displays it in the console.
    /// The timer will be started and progress can be updated by calling ProgressBar.Update. 
    /// </summary>
    /// <param name="operationText">The operation that is being performed.</param>
    /// <param name="itemsToComplete">The total number of items that need to be completed.</param>
    public ProgressBar(string operationText, long itemsToComplete)
    {
        _operationText = operationText;
        _itemsToComplete = itemsToComplete;
    }

    /// <summary>
    /// Releases all resources used by the current instance of ProgressBar.
    /// </summary>
    public void Dispose()
    {
        _disposed = true;
        _isActive = false;
        _progressTimer?.Dispose();

        Console.Write("\r\n\r\n");
    }

    public async Task Start()
    {
        if (_isActive || _disposed)
            return;

        Console.Write($"{_operationText} | ");
        Console.Write($"{_emptyProgressContainer}", backgroundColor: ConsoleColor.Gray);
        Console.Write($" | 0%");
        _timeColumnPosition = Console.CursorColumnPosition + 4;

        await Task.Delay(1000);
        _progressTimer = new Timer(ProgressBarTimer, null, 0, 10);
        _isActive = true;
    }

    /// <summary>
    /// Updates the progress bar with the items that were completed. The value passed in will be added to the
    /// current to the previous ammount passed in.
    /// </summary>
    /// <param name="items">The number of new that were completed since the last Update call.</param>
    public void Update(long items)
    {
        if (!_isActive || _disposed)
            return;

        _itemsCompleted += items;

        long percentage = (long)(100 * _itemsCompleted / _itemsToComplete);

        if (percentage % 2 != 0 || percentage == _previousPercentage || percentage > 100)
            return;

        for (long i = _previousPercentage; i < percentage; i += 2)
        {
            _emptyProgressContainer = _emptyProgressContainer.Remove(0, 1);
            _filledProgressContainer = _filledProgressContainer.Insert(0, " ");
        }

        lock (_progressBarLocker)
        {
            Console.CursorColumnPosition = 0;
            Console.Write($@"{_operationText} | ");
            Console.Write($@"{_filledProgressContainer}", backgroundColor: ConsoleColor.Green);
            Console.Write($@"{_emptyProgressContainer}", backgroundColor: ConsoleColor.Gray);
            Console.Write($@" | {percentage}%");
        }

        _previousPercentage = percentage;
    }

    private void ProgressBarTimer(object? state)
    {
        lock (_progressBarLocker)
        {
            if (!_disposed)
            {
                Console.CursorColumnPosition = _timeColumnPosition;
                Console.Write($"{_elaspedTime:N2}s");
            }
        }

        _elaspedTime += 0.01;
    }
}