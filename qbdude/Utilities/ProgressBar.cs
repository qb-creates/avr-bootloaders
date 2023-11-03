namespace qbdude.utilities;

public sealed class ProgressBarUtility
{
    private CancellationTokenSource source = new CancellationTokenSource();
    private string progressBar = "                                                  ";
    private string progressBarSymbol = "";
    private long previousPercentage = 0;
    private int elaspedTime = 0;
    private string operationText = string.Empty;
    private bool isActive;
    private static ProgressBarUtility? instance;

    public static ProgressBarUtility Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new ProgressBarUtility();
            }

            return instance;
        }
    }
    private ProgressBarUtility() { }

    public void StartProgressBar(string operationText)
    {
        if (!isActive)
        {
            source = new CancellationTokenSource();
            this.operationText = operationText;
            isActive = true;

            Thread t = new Thread(() => ProgressBarTimer(source.Token));
            t.Start();
        }
    }

    private long startingValue = 0;
    public async Task StartProgressBar(string operationText, long startingValue)
    {
        if (!isActive)
        {
            this.startingValue = startingValue;
            tempValue = 0;
            source = new CancellationTokenSource();
            this.operationText = operationText;
            isActive = true;

            Console.Write($@"{operationText} | {progressBar} | 0%");
            await Task.Delay(1200);
            Thread t = new Thread(() => ProgressBarTimer(source.Token));
            t.Start();
        }
    }

    public void UpdateProgressBar(int percentage)
    {
        try
        {
            if (isActive)
            {
                if (percentage % 2 == 0 && percentage != previousPercentage && percentage <= 100)
                {
                    for (long i = previousPercentage / 2; i < percentage / 2; i++)
                    {
                        progressBar = progressBar.Remove(0, 1);
                        progressBarSymbol = progressBarSymbol.Insert(0, " ");
                    }

                    Console.SetCursorPosition(0, Console.CursorTop);
                    Console.Write($@"{operationText} | ");
                    Console.BackgroundColor = ConsoleColor.Green;
                    Console.Write($@"{progressBarSymbol}");
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.Write($@"{progressBar}");
                    Console.Write($@" | {percentage}%");
                    previousPercentage = percentage;
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    private long tempValue = 0;
    public void UpdateProgressBar2(long add)
    {
        tempValue += add;
        if (isActive)
        {
            long percentage = (long)((tempValue * 100) / startingValue);
            if (percentage % 2 == 0 && percentage != previousPercentage && percentage <= 100)
            {
                for (long i = previousPercentage / 2; i < percentage / 2; i++)
                {
                    progressBar = progressBar.Remove(0, 1);
                    progressBarSymbol = progressBarSymbol.Insert(0, " ");
                }

                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write($@"{operationText} | ");
                Console.BackgroundColor = ConsoleColor.Green;
                Console.Write($@"{progressBarSymbol}");
                Console.BackgroundColor = ConsoleColor.Black;
                Console.Write($@"{progressBar}");
                Console.Write($@" | {percentage}%");
                previousPercentage = percentage;
                previousPercentage = percentage;
            }
        }
    }

    public async Task StopProgressBar()
    {
        source.Cancel();

        while (isActive)
        {
            await Task.Delay(1);
        }
    }

    private void ProgressBarTimer(CancellationToken token)
    {
        while (true)
        {
            if (token.IsCancellationRequested)
            {
                progressBar = "                                                  ";
                previousPercentage = 0;
                elaspedTime = 0;
                isActive = false;
                Console.WriteLine("\n");
                return;
            }

            Console.CursorVisible = false;
            Console.SetCursorPosition(69, Console.CursorTop);
            Console.Write($@"{elaspedTime}s");
            Thread.Sleep(1000);
            elaspedTime++;
        }
    }
}