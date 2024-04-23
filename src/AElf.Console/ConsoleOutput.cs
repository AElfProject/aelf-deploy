using Spectre.Console;

namespace AElf.Console;

public static class ConsoleOutput
{
    public static void StartAlert(params string[] outputs)
    {
        WriteStringsToConsole(outputs.Select(o => $"[yellow]{o}[/]"));
        AnsiConsole.WriteLine(
            "--------------------------------------------------------------------------------------------");
    }

    public static void StandardAlert(params string[] outputs)
    {
        WriteStringsToConsole(outputs.Select(o => $"[white]{o}[/]"));
    }

    public static void GenerateAlert(params string[] outputs)
    {
        WriteStringsToConsole(outputs.Select(o => $"[deepskyblue1]{o}[/]"));
    }

    public static void SuccessAlert(params string[] outputs)
    {
        System.Console.WriteLine(
            "--------------------------------------------------------------------------------------------");
        WriteStringsToConsole(outputs.Select(o => $"[green]{o}[/]"));
    }

    public static void WarningAlert(params string[] outputs)
    {
        WriteStringsToConsole(outputs.Select(o => $"[yellow]{o}[/]"));
    }

    public static void ErrorAlert(params string[] outputs)
    {
        WriteStringsToConsole(outputs.Select(o => $"[red]{o}[/]"));
        Environment.Exit(0);
    }

    private static void WriteStringsToConsole(IEnumerable<string> outputs)
    {
        foreach (var output in outputs)
        {
            AnsiConsole.MarkupLine(output);
        }
    }
}