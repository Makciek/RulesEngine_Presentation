
using RuleEngine.Examples;

Console.WriteLine("Example 1 - simple rule:");
await Example1.Run();

Console.WriteLine("\n\nExample 2 - two rules:");
await Example2.Run();

Console.WriteLine("\n\nExample 3 - types injections:");
await Example3_TypesInjection.Run();

Console.WriteLine("\n\nExample 4 - Operators:");
await Example4_CustomActions.Run();

Console.ReadLine();