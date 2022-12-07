using Spectre.Console;

namespace TcpChat.Client.App;

public static class AnsiConsoleUtil
{
    public static string Title { get; set; } = "WELCOME TO TCP CHAT";
    
    public static void ClearToBeginning()
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(
            new FigletText(Title)
                .Centered()
                .Color(Color.Teal));
    }
}