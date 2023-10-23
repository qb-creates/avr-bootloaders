namespace qbdude;

public sealed class ProgressBar
{
    private CancellationTokenSource source = new CancellationTokenSource();
    private string progressBar = "                                                  ";
    private int previousPercentage = 0;
    private int elaspedTime = 0;
    private string operationText = string.Empty;
    private bool isActive;
    private static ProgressBar? instance;

    public static ProgressBar Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new ProgressBar();
            }

            return instance;
        }
    }
    private ProgressBar() { }

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

    private int startingValue = 0;
    public void StartProgressBar(string operationText, int startingValue)
    {
        if (!isActive)
        {
            this.startingValue = startingValue;
            tempValue = 0;
            source = new CancellationTokenSource();
            this.operationText = operationText;
            isActive = true;

            Thread t = new Thread(() => ProgressBarTimer(source.Token));
            t.Start();
        }
    }

    public void UpdateProgressBar(int percentage)
    {
        if (isActive)
        {
            if (percentage % 2 == 0 && percentage != previousPercentage && percentage <= 100)
            {
                for (int i = previousPercentage / 2; i < percentage / 2; i++)
                {
                    progressBar = progressBar.Remove(i, 1).Insert(i, "#");
                }

                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write($@"{operationText} | {progressBar} | {percentage}%");
                previousPercentage = percentage;
            }
        }
    }
    private int tempValue = 0;
    public void UpdateProgressBar2(int add)
    {
        tempValue += add;
        if (isActive)
        {
            int percentage = (int)(tempValue / startingValue * 100);
            if (percentage % 2 == 0 && percentage != previousPercentage && percentage <= 100)
            {
                for (int i = previousPercentage / 2; i < percentage / 2; i++)
                {
                    progressBar = progressBar.Remove(i, 1).Insert(i, "#");
                }

                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write($@"{operationText} | {progressBar} | {percentage}%");
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