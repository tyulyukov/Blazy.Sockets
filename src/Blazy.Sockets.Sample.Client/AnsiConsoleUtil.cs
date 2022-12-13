using Spectre.Console;

namespace Blazy.Sockets.Sample.Client;

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