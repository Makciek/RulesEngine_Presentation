using RulesEngine.Models;

namespace RuleEngine.Examples;

public class Example1
{
    record TimeEntry(int DurationMinutes, string? Description);

    public static async Task Run()
    {
        var configuredWorkflow = new Workflow()
        {
            WorkflowName = "Validate time entry",
            Rules = [
                new Rule()
                {
                    RuleName = "Entries longer than 5 minutes must have description",
                    Expression = "TimeEntry.DurationMinutes < 6 || !string.IsNullOrEmpty(TimeEntry.Description)",
                    ErrorMessage = "Description is required for entries longer than 5 minutes",
                },
            ]
        };

        var rulesEngine = new RulesEngine.RulesEngine([configuredWorkflow]);

        List<TimeEntry> timeEntries = [
            new (5, null),
            new (10, "Description"),
            new (10, null),
        ];
        foreach (var (timeEntryParam, timeEntry) in timeEntries.Select(te => (new RuleParameter(nameof(TimeEntry), te), te)))
        {
            var result = await rulesEngine.ExecuteAllRulesAsync(configuredWorkflow.WorkflowName, timeEntryParam);
            var success = result.TrueForAll(r => r.IsSuccess);
            var error = success ? null : string.Join("; ", result.Where(r => !r.IsSuccess)
                .Select(r => r.Rule.ErrorMessage ?? r.ExceptionMessage));
            Console.Write($"Entry with description: \"{timeEntry.Description ?? "null"}\" and duration: {timeEntry.DurationMinutes}\nResult: ");
            Console.WriteLine(success ? "Succeed\n" : $"Failed with errors: {error}\n");
        }
    }
}