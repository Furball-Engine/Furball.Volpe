using System;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using Volpe.Exceptions;

namespace Volpe.Evaluation
{
    public static class DefaultBuiltins
    {
        public static (string, int, Func<EvaluatorContext, Value[], Value>)[] Math = new (string, int, Func<EvaluatorContext, Value[], Value>)[]
        {
            new ("abs", 1, (context, values) =>
            {
                if (values[0] is not Value.Number n)
                    throw new InvalidValueTypeException(
                        typeof(Value.Number), values[0].GetType(), context.Expression.PositionInText);

                return new Value.Number(System.Math.Abs(n.Value));
            }),
            
            new ("cos", 1, (context, values) =>
            {
                if (values[0] is not Value.Number n)
                    throw new InvalidValueTypeException(
                        typeof(Value.Number), values[0].GetType(), context.Expression.PositionInText);

                return new Value.Number(System.Math.Cos(n.Value));
            }),
            
            new ("sin", 1, (context, values) =>
            {
                if (values[0] is not Value.Number n)
                    throw new InvalidValueTypeException(
                        typeof(Value.Number), values[0].GetType(), context.Expression.PositionInText);

                return new Value.Number(System.Math.Sin(n.Value));
            }),
            
            new ("tan", 1, (context, values) =>
            {
                if (values[0] is not Value.Number n)
                    throw new InvalidValueTypeException(
                        typeof(Value.Number), values[0].GetType(), context.Expression.PositionInText);

                return new Value.Number(System.Math.Tan(n.Value));
            }),
            
            new ("log", 1, (context, values) =>
            {
                if (values[0] is not Value.Number n)
                    throw new InvalidValueTypeException(
                        typeof(Value.Number), values[0].GetType(), context.Expression.PositionInText);

                return new Value.Number(System.Math.Log(n.Value));
            }),
            
            new ("log2", 1, (context, values) =>
            {
                if (values[0] is not Value.Number n)
                    throw new InvalidValueTypeException(
                        typeof(Value.Number), values[0].GetType(), context.Expression.PositionInText);

                return new Value.Number(System.Math.Log2(n.Value));
            }),
            
            new ("sqrt", 1, (context, values) =>
            {
                if (values[0] is not Value.Number(var n))
                    throw new InvalidValueTypeException(
                        typeof(Value.Number), values[0].GetType(), context.Expression.PositionInText);

                return new Value.Number(System.Math.Sqrt(n));
            }),
            
            new ("pow", 2, (context, values) =>
            {
                if (values[0] is not Value.Number(var x))
                    throw new InvalidValueTypeException(
                        typeof(Value.Number), values[0].GetType(), context.Expression.PositionInText);
                
                if (values[1] is not Value.Number(var n))
                    throw new InvalidValueTypeException(
                        typeof(Value.Number), values[1].GetType(), context.Expression.PositionInText);


                return new Value.Number(System.Math.Pow(x, n));
            }),
            
            new ("ceil", 1, (context, values) =>
            {
                if (values[0] is not Value.Number(var x))
                    throw new InvalidValueTypeException(
                        typeof(Value.Number), values[0].GetType(), context.Expression.PositionInText);

                return new Value.Number(System.Math.Ceiling(x));
            }),
            
            new ("floor", 1, (context, values) =>
            {
                if (values[0] is not Value.Number(var x))
                    throw new InvalidValueTypeException(
                        typeof(Value.Number), values[0].GetType(), context.Expression.PositionInText);

                return new Value.Number(System.Math.Floor(x));
            }),
            
            new ("round", 1, (context, values) =>
            {
                if (values[0] is not Value.Number(var x))
                    throw new InvalidValueTypeException(
                        typeof(Value.Number), values[0].GetType(), context.Expression.PositionInText);

                return new Value.Number(System.Math.Round(x));
            }),
        };

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
            
            new ("string", 1, (context, values) =>/**/
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