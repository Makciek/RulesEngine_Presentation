namespace RuleEngine.Examples;

using RulesEngine.Models;

public class Example2
{
    public static async Task Run()
    {
        List<TimeEntry> timeEntries = [
            new (5, null, true),
            new (5, null, false),
            new (5, "Billable entry", true),
            new (10, null, false),
        ];
        var configuredWorkflow = new Workflow()
        {
            WorkflowName = "Validate time entry description",
            Rules = [
                new Rule()
                {
                    RuleName = "Entries longer than 5 minutes must have description",
                    Expression = "TimeEntry.DurationMinutes < 6 || !string.IsNullOrEmpty(TimeEntry.Description)",
                    ErrorMessage = "Description is required for entries longer than 5 minutes",
                },
                new Rule()
                {
                    RuleName = "Billable entries must have description",
                    Expression = "!TimeEntry.Billable || !string.IsNullOrEmpty(TimeEntry.Description)",
                    ErrorMessage = "Description is required for Billable entries",
                },
            ]
        };

        var rulesEngine = new RulesEngine.RulesEngine([configuredWorkflow]);
        foreach (var (timeEntryParam, timeEntry) in timeEntries.GetParametersWithObject())
        {
            var result = await rulesEngine.RunRules(configuredWorkflow.WorkflowName, timeEntryParam);
            Console.Write($"Entry with description: \"{timeEntry.Description ?? "null"}\", duration: {timeEntry.DurationMinutes}, Billable: {timeEntry.Billable}\nResult: ");
            Console.WriteLine(result.Succeed ? "Succeed\n" : $"Failed, Errors: {result.Errors}\n");
        }
    }

    record TimeEntry(int DurationMinutes, string? Description, bool Billable);
}