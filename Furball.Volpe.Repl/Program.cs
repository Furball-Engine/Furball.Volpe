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
    class Program {
        public static StreamReader InputStream;
        public static StreamWriter OutputStream;
        
        static void Main(string[] args) {
            InputStream  = new(Console.OpenStandardInput());
            OutputStream = new(Console.OpenStandardOutput());

            OutputStream.AutoFlush = true;

            bool replMode = args.Length == 0;

            string s = "";
            //TODO: Add logic to check for people sending files through stdin
            //maybe if args[0] == "-"? that seems to be a standard convention in Unix

            Environment environment = new(DefaultBuiltins.GetAll().Concat(Builtins.Funcs).ToArray());
            
            if(replMode) {
                Console.WriteLine($"Volpe Language - REPL");

                for (;;) {
                    Console.Write(">> ");
                    string input = Console.ReadLine();

                    try {
                        Parser parser = new Parser(new Lexer(input!).GetTokenEnumerator());
                        Value[] results = parser.GetExpressionEnumerator()
                                                .Select(expr => new EvaluatorContext(expr, environment).Evaluate()).ToArray();

                        for (int i = 0; i < results.Length; i++)
                            Console.WriteLine($"[{i}] {results[i].RepresentationWithClass}");
                    }
                    catch (VolpeException ex) {
#if DEBUG
                        Console.WriteLine(ex);
#else
                    Console.WriteLine(ex.Message);
#endif
                    }
                }
            }
            else {
                if (string.IsNullOrEmpty(s))
                    s = File.ReadAllText(args[0]);
                
                IEnumerable<Token> tokenStream = new Lexer(s).GetTokenEnumerator();

                Parser parser = new(tokenStream);
                
                while (parser.TryParseNextExpression(out Expression expression)) {
                    new EvaluatorContext(expression!, environment).Evaluate();
                }
            }
        }
    }
}