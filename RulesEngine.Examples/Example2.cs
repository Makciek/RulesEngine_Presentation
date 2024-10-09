namespace RuleEngine.Examples;

using RulesEngine.Models;

public class Example2
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
                    ErrorMessage = "Description is required for entries longer than 5 minutes",
                },
                new Rule()
                {
                    RuleName = "Allow no description for non-billable entries",
                    Expression = "!string.IsNullOrEmpty(Description) || DurationMinutes <= 5",
                    ErrorMessage = "Description is required for entries longer than 5 minutes",
                },
            ]
        };

        var rulesEngine = new RulesEngine.RulesEngine([configuredWorkflow]);

        List<TimeEntry> timeEntriesParams = [
            new (5, null, true),
            new (10, "Billable entry", true),
            new (10, "Non-billable", false),
        ];
        foreach (var (timeEntryParam, timeEntry) in timeEntriesParams.GetParametersWithObject())
        {
            var result = await rulesEngine.RunRules(configuredWorkflow.WorkflowName, timeEntryParam);
            Console.Write($"Entry with description: \"{timeEntry.Description ?? "null"}\" and duration: {timeEntry.DurationMinutes} Result: ");
            Console.WriteLine(result.Succeed ? "Succeed" : $"Failed with errors: {result.Errors}");
        }
    }

    record TimeEntry(int DurationMinutes, string? Description, bool Billable);
}