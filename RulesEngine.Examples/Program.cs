
using RuleEngine.Examples;

Console.WriteLine("Example 1 - simple rule:");
await Example1.Run();

Console.WriteLine("\n\nExample 2 - two rules:");
await Example2.Run();

Console.WriteLine("\n\nExample 3 - types injections:");
await Example3_TypesInjection.Run();

Console.WriteLine("\n\nExample 4 - custom actions:");
await Example4_CustomActions.Run();

Console.WriteLine("\n\nExample 5 - dependency loaders:");
await Example5_DependencyLoader.Run();

Console.ReadLine();