namespace RuleEngine.Examples;

using System.Linq;
using RulesEngine.Models;

public class Example5_DependencyLoader
{
    record TimeEntry(int DurationMinutes, string? Description, bool Billable);

    record DynamicConfiguration(int MinimalDuration);

    public static async Task Run()
    {
        List<TimeEntry> timeEntries = [
            new (5, "Billable entry", true),
            new (10, "Another billable entry", true),
        ];
        var configuredWorkflow = new Workflow()
        {
            WorkflowName = "Validate time entry",
            Rules = [
                new Rule()
                {
                    RuleName = "Billable entry must be longer than minimal configured duration",
                    Expression = "!TimeEntry.Billable || TimeEntry.DurationMinutes > DynamicConfiguration.MinimalDuration",
                    ErrorMessage = "Time entry is shorter than configured duration",
                    Properties = new Dictionary<string, object>()
                    {
                        { "dependencies", new List<string> { "DynamicConfiguration" } }
                    }
                },
            ]
        };

        var rulesEngine = new RulesEngine.RulesEngine([configuredWorkflow]);
        foreach (var (timeEntryParam, timeEntry) in timeEntries.GetParametersWithObject())
        {
            var dependencies = configuredWorkflow.Rules.SelectMany(r =>
            {
                var ruleDependencies = r.Properties?.GetValueOrDefault("dependencies") as List<string>;
                // load dependencies - here we just return a new instance of DynamicConfiguration for simplicity
                return new[] { new RuleParameter(ruleDependencies.Single(), new DynamicConfiguration(8)) };
            });

            await RunRules(rulesEngine, configuredWorkflow.WorkflowName, timeEntry, [timeEntryParam, .. dependencies]);
        }
    }

    private static async Task RunRules(RulesEngine.RulesEngine rulesEngine, string workflowName, TimeEntry timeEntry, RuleParameter[] parameters)
    {
        var result = await rulesEngine.RunRules(workflowName, parameters);
        Console.Write($"Entry with description: \"{timeEntry.Description ?? "null"}\", duration: {timeEntry.DurationMinutes}, Billable: {timeEntry.Billable}\nResult: ");
        Console.WriteLine(result.Succeed ? "Succeed\n" : $"Failed, Errors: {result.Errors}\n");
    }
}