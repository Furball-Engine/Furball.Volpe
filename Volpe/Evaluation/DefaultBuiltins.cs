using System;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using Volpe.Exceptions;

namespace Volpe.Evaluation
{
    public static class DefaultBuiltins
    {
        public static (string, int, Func<EvaluatorContext, Value[], Value>)[] Core = new (string, int, Func<EvaluatorContext, Value[], Value>)[]
        {
            new ("int", 1, (context, values) =>
            {
                Value value = values[0];

                if (value is Value.Number)
                    return value;
                
                if (value is Value.String str && double.TryParse(str.Value, out double converted))
                    return new Value.Number(converted);

                throw new TypeConversionException(value, typeof(Value.Number),
                    context.Expression.PositionInText);
            }),
            new ("string", 1, (context, values) =>
            {
                Value value = values[0];

                if (value is Value.String)
                    return value;
                
                if (value is Value.Number n)
                    return new Value.String(n.Value.ToString(CultureInfo.InvariantCulture));

                throw new TypeConversionException(value, typeof(Value.Number),
                    context.Expression.PositionInText);
            }),
            new ("repr", 1, (context, values) => new Value.String(values[0].Representation)),
            new ("invoke", 1, (context, values) =>
            {
                if (values[0] is not Value.FunctionReference functionReference)
                    throw new InvalidValueTypeException(
                        typeof(Value.FunctionReference), values[0].GetType(),
                        context.Expression.PositionInText);

                if (values.Length - 1 < functionReference.Function.ParameterCount)
                        throw new ParamaterCountMismatchException(functionReference.Name, 
                            functionReference.Function.ParameterCount, 
                            values.Length - 1, context.Expression.PositionInText);

                return functionReference.Function.Invoke(context, values[1..]);
            })
        };
    }
}