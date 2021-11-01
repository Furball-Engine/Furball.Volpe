using System;
using System.Globalization;
using System.Runtime.InteropServices;
using Volpe.Exceptions;

namespace Volpe.Evaluation
{
    public static class DefaultBuiltins
    {
        public static (string, Func<Evaluator.Context, Value[], Value>)[] Io = new (string name, Func<Evaluator.Context, Value[], Value> del)[]
        {
            new ("print", (context, values) =>
            {
                if (values.Length != 1)
                    throw new ParamaterCountMismatchException(context.RootExpression.PositionInText);
                
                string output = values[0] switch
                {
                    Value.Number(var number) => number.ToString(CultureInfo.InvariantCulture),
                    Value.Void => "void",
                    Value.String(var value) => value, 
                    Value.FunctionReference(var name, var function) => $"<Function \"{name}\", {function.GetHashCode()}>"
                };
                
                Console.WriteLine(output);
                
                return Value.DefaultVoid;
            })
        };
    }
}