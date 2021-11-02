using System;
using System.Collections.Immutable;
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

            Scope scope = new Scope(DefaultBuiltins.Core.Concat(DefaultBuiltins.Math).ToArray());
            
            for (;;)
            {
                Console.Write(">> ");
                string input = Console.ReadLine();

                try
                {
                    Parser parser = new Parser(new Lexer(input).GetTokenEnumerator().ToImmutableArray());
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