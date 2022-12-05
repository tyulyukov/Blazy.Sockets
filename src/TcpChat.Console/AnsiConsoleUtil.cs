using Spectre.Console;

namespace TcpChat.Console;

public static class AnsiConsoleUtil
{
    public static void ClearToBeginning()
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(
            new FigletText("WELCOME TO TCP CHAT")
                .Centered()
                .Color(Color.Teal));
    }
}