using RulesEngine.Actions;

namespace RuleEngine.Examples;

using RulesEngine.Models;

public class Example4_CustomActions
{
    private class CorpoGodAction : ActionBase
    {
        public override ValueTask<object> Run(ActionContext context, RuleParameter[] ruleParameters)
        {
            var overtimeStartHour = int.Parse(context.GetContext<string>("OvertimeStartHour"));
            var overtime = (ruleParameters[0].Value as TimeEntry).Ended.Hour - overtimeStartHour;

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(overtime < 1 ? "Go back to work!!!" : "Take it easy, just 3 hours left!");
            Console.ForegroundColor = ConsoleColor.White;
            return new ValueTask<object>(Task.CompletedTask);
        }
    }

    public static async Task Run()
    {
        var actions = new Dictionary<string, Func<ActionBase>>() { { nameof(CorpoGodAction), () => new CorpoGodAction() } };
        var reSettings = new ReSettings() { CustomActions = actions };

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
                    Expression = "timeEntry.Billable && timeEntry.Description.Length > 10 && timeEntry.Ended.Hour > 18",
                    ErrorMessage = "Go back to work! Your work must be billable, described and on the overtime!",
                    Actions = new RuleActions()
                    {
                        OnFailure = new ActionInfo()
                        {
                            Name = nameof(CorpoGodAction),
                            Context = new Dictionary<string, object>()
                            {
                                { "OvertimeStartHour", "18" }
                            }
                        }
                    }
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