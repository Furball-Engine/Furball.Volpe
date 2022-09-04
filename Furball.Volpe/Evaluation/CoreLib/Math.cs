using System;
using Furball.Volpe.Exceptions;

namespace Furball.Volpe.Evaluation.CoreLib; 

public class Math : CoreLibExtension {

    public override BuiltinFunction[] FunctionExports() => new BuiltinFunction[]
    {
        new("abs", 1, (context, values) =>
        {
            if (values[0] is not Value.Number n)
                throw new InvalidValueTypeException(
                typeof(Value.Number), values[0].GetType(), context.Expression.PositionInText);

            return new Value.Number(System.Math.Abs(n.Value));
        }),

        new("cos", 1, (context, values) =>
        {
            if (values[0] is not Value.Number n)
                throw new InvalidValueTypeException(
                typeof(Value.Number), values[0].GetType(), context.Expression.PositionInText);

            return new Value.Number(System.Math.Cos(n.Value));
        }),

        new("sin", 1, (context, values) =>
        {
            if (values[0] is not Value.Number n)
                throw new InvalidValueTypeException(
                typeof(Value.Number), values[0].GetType(), context.Expression.PositionInText);

            return new Value.Number(System.Math.Sin(n.Value));
        }),

        new("tan", 1, (context, values) =>
        {
            if (values[0] is not Value.Number n)
                throw new InvalidValueTypeException(
                typeof(Value.Number), values[0].GetType(), context.Expression.PositionInText);

            return new Value.Number(System.Math.Tan(n.Value));
        }),

        new("log", 1, (context, values) =>
        {
            if (values[0] is not Value.Number n)
                throw new InvalidValueTypeException(
                typeof(Value.Number), values[0].GetType(), context.Expression.PositionInText);

            return new Value.Number(System.Math.Log(n.Value));
        }),

        new("log2", 1, (context, values) =>
        {
            if (values[0] is not Value.Number n)
                throw new InvalidValueTypeException(
                typeof(Value.Number), values[0].GetType(), context.Expression.PositionInText);

            return new Value.Number(System.Math.Log(n.Value, 2));
        }),

        new("sqrt", 1, (context, values) =>
        {
            if (values[0] is not Value.Number(var n))
                throw new InvalidValueTypeException(
                typeof(Value.Number), values[0].GetType(), context.Expression.PositionInText);

            return new Value.Number(System.Math.Sqrt(n));
        }),

        new("pow", 2, (context, values) =>
        {
            if (values[0] is not Value.Number(var x))
                throw new InvalidValueTypeException(
                typeof(Value.Number), values[0].GetType(), context.Expression.PositionInText);
            if (values[1] is not Value.Number(var n))
                throw new InvalidValueTypeException(
                typeof(Value.Number), values[1].GetType(), context.Expression.PositionInText);

            return new Value.Number(System.Math.Pow(x, n));
        }),

        new("ceil", 1, (context, values) =>
        {
            if (values[0] is not Value.Number(var x))
                throw new InvalidValueTypeException(
                typeof(Value.Number), values[0].GetType(), context.Expression.PositionInText);

            return new Value.Number(System.Math.Ceiling(x));
        }),

        new("floor", 1, (context, values) =>
        {
            if (values[0] is not Value.Number(var x))
                throw new InvalidValueTypeException(
                typeof(Value.Number), values[0].GetType(), context.Expression.PositionInText);

            return new Value.Number(System.Math.Floor(x));
        }),

        new("round", 1, (context, values) =>
        {
            if (values[0] is not Value.Number(var x))
                throw new InvalidValueTypeException(
                typeof(Value.Number), values[0].GetType(), context.Expression.PositionInText);

            return new Value.Number(System.Math.Round(x));
        }),

        new("random", 0, (_, _) => new Value.Number(new Random().NextDouble())),

        new("random_range", 2, (context, values) => {
            if (values[0] is not Value.Number(var first))
                throw new InvalidValueTypeException(typeof(Value.Number), values[0].GetType(), context.Expression.PositionInText);
            if (values[1] is not Value.Number(var second))
                throw new InvalidValueTypeException(typeof(Value.Number), values[1].GetType(), context.Expression.PositionInText);

            return new Value.Number(new Random().NextDouble() * (second - first) + first);
        }),
    };
}