using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Furball.Volpe.Evaluation;
using Furball.Volpe.Exceptions;
using Furball.Volpe.LexicalAnalysis;
using Furball.Volpe.SyntaxAnalysis;
using Environment = Furball.Volpe.Evaluation.Environment;

namespace Furball.Volpe.Repl
{
    class Program
    {
        static void Main(string[] args)
        {
            Stream inputStream = Console.OpenStandardInput();
            Stream outputStream = Console.OpenStandardOutput();
            
            Console.WriteLine($"Volpe Language - REPL");
            
            Environment environment = new Environment(DefaultBuiltins.GetAll().Concat(new BuiltinFunction[]
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
                
                new("timeit", 2, (context, values) =>
                {
                    if (values[0] is not Value.FunctionReference(_, var function))
                        throw new InvalidValueTypeException(
                            typeof(Value.FunctionReference), values[0].GetType(), context.Expression.PositionInText);
                    
                    if (values[1] is not Value.Number(var loops))
                        throw new InvalidValueTypeException(
                            typeof(Value.FunctionReference), values[0].GetType(), context.Expression.PositionInText);

                    long lLoops = (long) loops;
                    double totalMilliseconds = 0;

                    for (long i = 0; i < lLoops; i++)
                    {
                        Stopwatch stopwatch = new Stopwatch();

                        stopwatch.Start();
                        function.Invoke(context, Array.Empty<Value>());
                        stopwatch.Stop();

                        totalMilliseconds += stopwatch.Elapsed.TotalMilliseconds;
                    }
                    
                    return new Value.Number(totalMilliseconds / loops);
                })
            }).ToArray());
            
            for (;;)
            {
                Console.Write(">> ");
                string input = Console.ReadLine();

                try
                {
                    Parser parser = new Parser(new Lexer(input!).GetTokenEnumerator());
                    Value[] results = parser.GetExpressionEnumerator()
                        .Select(expr => new EvaluatorContext(expr, environment).Evaluate()).ToArray();

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