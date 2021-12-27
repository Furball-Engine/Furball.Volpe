using System;
using Furball.Volpe.Exceptions;

namespace Furball.Volpe.Evaluation.CoreLib {
    public class Math : CoreLibExtension {

        public override BuiltinFunction[] FunctionExports() => new BuiltinFunction[]
        {
            new BuiltinFunction ("abs", 1, (context, values) =>
            {
                if (values[0] is not Value.Number n)
                    throw new InvalidValueTypeException(
                        typeof(Value.Number), values[0].GetType(), context.Expression.PositionInText);

                return new Value.Number(System.Math.Abs(n.Value));
            }),

            new BuiltinFunction ("cos", 1, (context, values) =>
            {
                if (values[0] is not Value.Number n)
                    throw new InvalidValueTypeException(
                        typeof(Value.Number), values[0].GetType(), context.Expression.PositionInText);

                return new Value.Number(System.Math.Cos(n.Value));
            }),

            new BuiltinFunction ("sin", 1, (context, values) =>
            {
                if (values[0] is not Value.Number n)
                    throw new InvalidValueTypeException(
                        typeof(Value.Number), values[0].GetType(), context.Expression.PositionInText);

                return new Value.Number(System.Math.Sin(n.Value));
            }),

            new BuiltinFunction ("tan", 1, (context, values) =>
            {
                if (values[0] is not Value.Number n)
                    throw new InvalidValueTypeException(
                        typeof(Value.Number), values[0].GetType(), context.Expression.PositionInText);

                return new Value.Number(System.Math.Tan(n.Value));
            }),

            new BuiltinFunction ("log", 1, (context, values) =>
            {
                if (values[0] is not Value.Number n)
                    throw new InvalidValueTypeException(
                        typeof(Value.Number), values[0].GetType(), context.Expression.PositionInText);

                return new Value.Number(System.Math.Log(n.Value));
            }),

            new BuiltinFunction ("log2", 1, (context, values) =>
            {
                if (values[0] is not Value.Number n)
                    throw new InvalidValueTypeException(
                        typeof(Value.Number), values[0].GetType(), context.Expression.PositionInText);

                return new Value.Number(System.Math.Log2(n.Value));
            }),

            new BuiltinFunction ("sqrt", 1, (context, values) =>
            {
                if (values[0] is not Value.Number(var n))
                    throw new InvalidValueTypeException(
                        typeof(Value.Number), values[0].GetType(), context.Expression.PositionInText);

                return new Value.Number(System.Math.Sqrt(n));
            }),

            new BuiltinFunction ("pow", 2, (context, values) =>
            {
                if (values[0] is not Value.Number(var x))
                    throw new InvalidValueTypeException(
                        typeof(Value.Number), values[0].GetType(), context.Expression.PositionInText);

                if (values[1] is not Value.Number(var n))
                    throw new InvalidValueTypeException(
                        typeof(Value.Number), values[1].GetType(), context.Expression.PositionInText);

                return new Value.Number(System.Math.Pow(x, n));
            }),

            new BuiltinFunction ("ceil", 1, (context, values) =>
            {
                if (values[0] is not Value.Number(var x))
                    throw new InvalidValueTypeException(
                        typeof(Value.Number), values[0].GetType(), context.Expression.PositionInText);

                return new Value.Number(System.Math.Ceiling(x));
            }),

            new BuiltinFunction ("floor", 1, (context, values) =>
            {
                if (values[0] is not Value.Number(var x))
                    throw new InvalidValueTypeException(
                        typeof(Value.Number), values[0].GetType(), context.Expression.PositionInText);

                return new Value.Number(System.Math.Floor(x));
            }),

            new BuiltinFunction ("round", 1, (context, values) =>
            {
                if (values[0] is not Value.Number(var x))
                    throw new InvalidValueTypeException(
                        typeof(Value.Number), values[0].GetType(), context.Expression.PositionInText);

                return new Value.Number(System.Math.Round(x));
            }),

            new BuiltinFunction("random", 0, (_, _) => new Value.Number(new Random().NextDouble())),

            new BuiltinFunction("random_range", 2, (context, values) => {
                if (values[0] is not Value.Number(var first))
                    throw new InvalidValueTypeException(typeof(Value.Number), values[0].GetType(), context.Expression.PositionInText);
                if (values[1] is not Value.Number(var second))
                    throw new InvalidValueTypeException(typeof(Value.Number), values[1].GetType(), context.Expression.PositionInText);

                return new Value.Number(new Random().NextDouble() * (second - first) + first);
            }),
        };
    }
}
