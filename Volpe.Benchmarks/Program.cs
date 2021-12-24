using System;
using System.Linq;
using System.Security.Cryptography;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Volpe.Evaluation;
using Volpe.LexicalAnalysis;
using Volpe.SyntaxAnalysis;

namespace Volpe.Benchmarks
{
    public static class EvaluatorUtils
    {
        public static Value EvaluateString(string v, Scope scope)
        {
            return new BlockEvaluatorContext(
                new Parser(new Lexer(v).GetTokenEnumerator()).GetExpressionEnumerator().ToArray(),
                scope).Evaluate();
        }
    }
    
    public class LexicalAnalysis
    {
        private Token[] _tokens1;
        
        public LexicalAnalysis()
        {
            _tokens1 = new Lexer("funcdef fib($n) { if ($n <= 1) { ret $n } ret fib($n - 1) + fib($n - 2) }").GetTokenEnumerator().ToArray();
        }

        [Benchmark]
        public void Parse1()
        {
            foreach (var _ in new Parser(_tokens1).GetExpressionEnumerator())
                continue;
        }
    }
    
    public class Recursion
    {
        private Scope _scope;
        
        public Recursion()
        {
            _scope = new Scope();

            EvaluatorUtils.EvaluateString("funcdef fib($n) { if ($n <= 1) { ret $n } ret fib($n - 1) + fib($n - 2) }", _scope);
        }
        
        [Benchmark]
        public void Fib10() => EvaluatorUtils.EvaluateString("fib 10", _scope);
        
        [Benchmark]
        public void Fib20() => EvaluatorUtils.EvaluateString("fib 20", _scope);
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            BenchmarkRunner.Run<LexicalAnalysis>();
        }
    }
}