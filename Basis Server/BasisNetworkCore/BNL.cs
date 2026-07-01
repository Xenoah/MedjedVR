using System;

/// <summary>
/// console color 対応の Basis network logger。
/// </summary>
public static class BNL
{
    /// <summary>
    /// ログOutputを保持します。型は Action<string> で、関連処理から共有される値です。
    /// </summary>
    public static Action<string> LogOutput;
    /// <summary>
    /// ログWarningOutputを保持します。型は Action<string> で、関連処理から共有される値です。
    /// </summary>
    public static Action<string> LogWarningOutput;
    /// <summary>
    /// ログエラーOutputを保持します。型は Action<string> で、関連処理から共有される値です。
    /// </summary>
    public static Action<string> LogErrorOutput;
    /// <summary>
    /// ログを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
    /// </summary>
    public static void Log(string message)
    {
        string formattedMessage = message;

        if (LogOutput != null)
        {
            LogOutput.Invoke(formattedMessage);
        }
        else
        {
            WriteWithColor(formattedMessage, ConsoleColor.White); // info は白
        }
    }

    /// <summary>
    /// ログWarningを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
    /// </summary>
    public static void LogWarning(string message)
    {
        if (LogWarningOutput != null)
        {
            LogWarningOutput.Invoke(message);
        }
        else
        {
            WriteWithColor(message, ConsoleColor.Yellow); // warning は黄色
        }
    }

    /// <summary>
    /// ログエラーを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
    /// </summary>
    public static void LogError(string message)
    {
        if (LogErrorOutput != null)
        {
            LogErrorOutput.Invoke(message);
        }
        else
        {
            WriteWithColor(message, ConsoleColor.Red); // error は赤
        }
    }
    /// <summary>
    /// WriteWithColorを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
    /// </summary>
    private static void WriteWithColor(string message, ConsoleColor color)
    {
        ConsoleColor originalColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(message);
        Console.ForegroundColor = originalColor;
    }
    /// <summary>
    /// ClearConsoleを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
    /// </summary>
    public static void ClearConsole()
    {
        Console.Clear();
    }
}
