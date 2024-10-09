using RulesEngine.Models;

namespace RuleEngine.Examples;

public static class BoilerplateExtensions
{
    public static async Task<(bool Succeed, string? Errors)> RunRules(this RulesEngine.RulesEngine engine, string workflowName, params RuleParameter[] parameters)
    {
        var result = await engine.ExecuteAllRulesAsync(workflowName, parameters);

        var success = result.TrueForAll(r => r.IsSuccess);
        var error = success ? null : string.Join("; ", result.Select(r => r.Rule.ErrorMessage ?? r.ExceptionMessage));

        return (success, error);
    }

    public static RuleParameter GetParameter<T>(this T obj) => new(typeof(T).Name, obj);

    public static IEnumerable<RuleParameter> GetParameters<T>(this IEnumerable<T> objects) => objects.Select(o => new RuleParameter(typeof(T).Name, o));

    public static IEnumerable<(RuleParameter, T)> GetParametersWithObject<T>(this IEnumerable<T> objects) => objects.Select(o => (new RuleParameter(typeof(T).Name, o), o));
}