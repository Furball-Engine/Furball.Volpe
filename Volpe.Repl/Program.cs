using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Volpe.Evaluation;
using Volpe.Exceptions;
using Volpe.LexicalAnalysis;
using Volpe.SyntaxAnalysis;

namespace Volpe.Repl
{
    class Program
    {
        
        
        static void Main(string[] args)
        {
            Console.WriteLine($"Volpe Language - REPL");

            Scope scope = new Scope(DefaultBuiltins.Core.Concat(DefaultBuiltins.Math).Concat(new BuiltinFunction[]
            {
                new BuiltinFunction("clear", 0, (_, _) =>
                {
                    Console.Clear();
                    return Value.DefaultVoid;
                }),
                
                new BuiltinFunction("println", 1, (context, values) =>
                {
                    if (values[0] is not Value.String(var str))
                        throw new InvalidValueTypeException(
                            typeof(Value.String), values[0].GetType(), context.Expression.PositionInText);
                    
                    Console.WriteLine(str);
                    
                    return Value.DefaultVoid;
                }),
            }).ToArray());
            
            for (;;)
            {
                Console.Write(">> ");
                string input = Console.ReadLine();

                try
                {
                    Parser parser = new Parser(new Lexer(input!).GetTokenEnumerator());
                    Value[] results = parser.GetExpressionEnumerator()
                        .Select(expr => new EvaluatorContext(expr, scope).Evaluate()).ToArray();

                    for (int i = 0; i < results.Length; i++)
                        Console.WriteLine($"[{i}] {results[i].Representation}");
                }
                catch (VolpeException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}