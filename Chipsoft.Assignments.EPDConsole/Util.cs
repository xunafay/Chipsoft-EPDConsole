using Spectre.Console;

namespace Chipsoft.Assignments.EPDConsole
{
    public class Util
    {
        public static string WeekdayToString(DateTime date)
        {
            return date.DayOfWeek switch
            {
                DayOfWeek.Sunday => "Zondag",
                DayOfWeek.Monday => "Maandag",
                DayOfWeek.Tuesday => "Dinsdag",
                DayOfWeek.Wednesday => "Woensdag",
                DayOfWeek.Thursday => "Donderdag",
                DayOfWeek.Friday => "Vrijdag",
                DayOfWeek.Saturday => "Zaterdag",
                _ => "",
            };
        }

        public static T? SearchAndSelect<T>(List<T> values, string searchPrompt, string selectPrompt, Func<T, string> converter)
        where T : class
        {
            var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, args) =>
            {
                args.Cancel = true;
                cts.Cancel();
            };

            AnsiConsole.Clear();

            var query = AnsiConsole.Prompt<string>(
                new TextPrompt<string>($"{searchPrompt} [green](leeg laten voor alle)[/]: ")
                .AllowEmpty()
            );

            if (!string.IsNullOrWhiteSpace(query))
            {
                values = values.Where(p => converter(p).Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (values.Count == 0)
            {
                AnsiConsole.Confirm("[bold red]Geen resultaten gevonden[/]");
                return default;
            }

            try
            {
                var value = new SelectionPrompt<T>()
                    .Title(selectPrompt)
                    .PageSize(3)
                    .UseConverter(converter)
                    .AddChoices(values)
                    .ShowAsync(AnsiConsole.Console, cts.Token)
                    .Result;

                return value;
            }
            catch (OperationCanceledException)
            {
                AnsiConsole.Confirm("[bold red]Geannuleerd[/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.Confirm($"[bold red]Er is iets fout gegaan: {ex.Message}[/]");
            }

            return default;
        }
    }
}