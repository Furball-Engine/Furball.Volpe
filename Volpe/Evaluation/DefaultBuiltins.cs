using System;
using System.Globalization;
using System.Runtime.InteropServices;
using Volpe.Exceptions;

namespace Volpe.Evaluation
{
    public static class DefaultBuiltins
    {
        public static (string, int, Func<Evaluator.Context, Value[], Value>)[] Io = new (string, int, Func<Evaluator.Context, Value[], Value>)[]
        {
            new ("println", 1, (context, values) =>
            {
                string output = values[0].Representation;
                
                Console.WriteLine(output);
                
                return Value.DefaultVoid;
            }),
            
            new ("repr", 1, (context, values) => new Value.String(values[0].Representation))
        };
    }
}