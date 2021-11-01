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
            new ("int", 1, (context, values) =>
            {
                Value value = values[0];

                if (value is Value.Number)
                    return value;
                
                if (value is Value.String str && double.TryParse(str.Value, out double converted))
                    return new Value.Number(converted);

                throw new TypeConversionException(value, typeof(Value.Number),
                    context.RootExpression.PositionInText);
            }),
            new ("string", 1, (context, values) =>
            {
                Value value = values[0];

                if (value is Value.String)
                    return value;
                
                if (value is Value.Number n)
                    return new Value.String(n.Value.ToString(CultureInfo.InvariantCulture));

                throw new TypeConversionException(value, typeof(Value.Number),
                    context.RootExpression.PositionInText);
            }),
            new ("repr", 1, (context, values) => new Value.String(values[0].Representation))
        };
    }
}