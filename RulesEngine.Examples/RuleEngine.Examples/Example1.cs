using RulesEngine.Models;

namespace RuleEngine.Examples;

public class Example1
{
    public static async Task Run()
    {
        var configuredWorkflow = new Workflow()
        {
            WorkflowName = "Validate time entry",
            Rules = [
                new Rule()
                {
                    RuleName = "Allow no description for entries shorter or equal to 5 minutes",
                    Expression = "!string.IsNullOrEmpty(Description) || DurationMinutes <= 5",
                    ErrorMessage = "Description is required for entries longer than 5 minutes"
                }
            ]
        };

        var rulesEngine = new RulesEngine.RulesEngine([configuredWorkflow]);

        List<TimeEntry> timeEntriesParams = [
            new (5, null),
            new (10, "Description"),
            new (10, null),
        ];
        foreach (var (timeEntryParam, timeEntry) in timeEntriesParams.Select(te => (new RuleParameter(nameof(TimeEntry), te), te)))
        {
            var result = await rulesEngine.ExecuteAllRulesAsync(configuredWorkflow.WorkflowName, timeEntryParam);
            var success = result.TrueForAll(r => r.IsSuccess);
            var error = success ? null : string.Join("; ", result.Select(r => r.Rule.ErrorMessage ?? r.ExceptionMessage));
            Console.Write($"Entry with description: \"{timeEntry.Description ?? "null"}\" and duration: {timeEntry.DurationMinutes} Result: ");
            Console.WriteLine(success ? "Succeed" : $"Failed with errors: {error}");
        }
    }

    record TimeEntry(int DurationMinutes, string? Description);
}