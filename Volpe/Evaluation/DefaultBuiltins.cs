using System;
using System.Globalization;
using System.Runtime.InteropServices;
using Volpe.Exceptions;

namespace Volpe.Evaluation
{
    public static class DefaultBuiltins
    {
        private static string GetValueRepresentation(Value v)
        {
            return v switch
            {
                Value.Number(var number) => number.ToString(CultureInfo.InvariantCulture),
                Value.Void => "void",
                Value.String(var value) => value, 
                Value.FunctionReference(var name, var function) => $"<Function \"{name}\", {function.GetHashCode()}>",
                
                _ => throw new ArgumentOutOfRangeException(nameof(v), v, null)
            };
        }
        
        public static (string, int, Func<Evaluator.Context, Value[], Value>)[] Io = new (string, int, Func<Evaluator.Context, Value[], Value>)[]
        {
            new ("println", 1, (context, values) =>
            {
                string output = GetValueRepresentation(values[0]);
                
                Console.WriteLine(output);
                
                return Value.DefaultVoid;
            }),
            
            new ("repr", 1, (context, values) => new Value.String(GetValueRepresentation(values[0])))
        };
    }
}