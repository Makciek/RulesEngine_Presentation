namespace RuleEngine.Examples;

using RulesEngine.Models;

public class Example3_TypesInjection
{
    private static class CustomRuleChecker
    {
        public static bool CheckIfCorpoGodsApproveThis(TimeEntry timeEntry)
            => timeEntry.Billable && timeEntry.Description.Length > 10 && timeEntry.Ended.Hour > 18; // Created in an overtime
    }

    public static async Task Run()
    {
        var reSettings = new ReSettings() { CustomTypes = [typeof(CustomRuleChecker)] };

        List<TimeEntry> timeEntries = [
            new (5, "╮（╯＿╰）╭", true, DateTime.Today.AddHours(13)),
            new (50, "I'm a good employee!", true, DateTime.Today.AddHours(19)),
        ];
        var configuredWorkflow = new Workflow()
        {
            WorkflowName = "Validate The Corporation Gods approval",
            Rules = [
                new Rule()
                {
                    RuleName = "CorpoGods approval",
                    Expression = "CustomRuleChecker.CheckIfCorpoGodsApproveThis(TimeEntry)",
                    ErrorMessage = "Go back to work! Your work must be billable, described and on the overtime!",
                },
            ]
        };

        var rulesEngine = new RulesEngine.RulesEngine([configuredWorkflow], reSettings);

        foreach (var (timeEntryParam, timeEntry) in timeEntries.GetParametersWithObject())
        {
            var result = await rulesEngine.RunRules(configuredWorkflow.WorkflowName, timeEntryParam);
            Console.Write($"Entry with description: \"{timeEntry.Description ?? "null"}\", duration: {timeEntry.DurationMinutes}, Billable: {timeEntry.Billable}\nResult: ");
            Console.WriteLine(result.Succeed ? "Succeed\n" : $"Failed, Errors: {result.Errors}\n");
        }
    }

    record TimeEntry(int DurationMinutes, string? Description, bool Billable, DateTime Ended);
}