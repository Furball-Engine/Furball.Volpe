using System;
using System.Collections.Generic;
using Furball.Volpe.Exceptions;
using Furball.Volpe.Memory;

namespace Furball.Volpe.Evaluation.CoreLib; 

public class Strings : CoreLibExtension {

    public override BuiltinFunction[] FunctionExports() => new BuiltinFunction[] {
        new BuiltinFunction("str_format", 2, (context, values) => {
            if(values[0] is not Value.String(var formatString))
                throw new InvalidValueTypeException(typeof(Value.String), values[0].GetType(), context.Expression.PositionInText);
            if(values[1] is not Value.Array(var paramArray))
                throw new InvalidValueTypeException(typeof(Value.Array), values[1].GetType(), context.Expression.PositionInText);

            List<object> parameters = new();

            foreach (CellSwap<Value> value in paramArray) {
                switch (value.Value) {
                    case Value.Boolean boolean: {
                        parameters.Add(boolean.Value);
                        break;
                    }
                    case Value.Number number: {
                        parameters.Add(number.Value);
                        break;
                    }
                    case Value.String str: {
                        parameters.Add(str.Value);
                        break;
                    }
                }
            }

            return new Value.String(string.Format(formatString, parameters.ToArray()));
        }),
        new BuiltinFunction("str_len", 1, (context, values) => {
            if(values[0] is not Value.String(var str))
                throw new InvalidValueTypeException(typeof(Value.String), values[0].GetType(), context.Expression.PositionInText);

            return new Value.Number(str.Length);
        }),
        new BuiltinFunction("str_toupper", 1, (context, values) => {
            if(values[0] is not Value.String(var str))
                throw new InvalidValueTypeException(typeof(Value.String), values[0].GetType(), context.Expression.PositionInText);

            return new Value.String(str.ToUpper());
        }),
        new BuiltinFunction("str_tolower", 1, (context, values) => {
            if(values[0] is not Value.String(var str))
                throw new InvalidValueTypeException(typeof(Value.String), values[0].GetType(), context.Expression.PositionInText);

            return new Value.String(str.ToLower());
        }),
        new BuiltinFunction("str_substr", 3, (context, values) => {
            if(values[0] is not Value.String(var str))
                throw new InvalidValueTypeException(typeof(Value.String), values[0].GetType(), context.Expression.PositionInText);
            if(values[1] is not Value.Number(var begin))
                throw new InvalidValueTypeException(typeof(Value.Number), values[1].GetType(), context.Expression.PositionInText);
            if(values[2] is not Value.Number(var length))
                throw new InvalidValueTypeException(typeof(Value.Number), values[2].GetType(), context.Expression.PositionInText);

            return new Value.String(str.Substring((int)begin, (int)length));
        }),
        new BuiltinFunction("str_split", 2, (context, values) => {
            if(values[0] is not Value.String(var str))
                throw new InvalidValueTypeException(typeof(Value.String), values[0].GetType(), context.Expression.PositionInText);
            if(values[1] is not Value.String(var delimiter))
                throw new InvalidValueTypeException(typeof(Value.String), values[1].GetType(), context.Expression.PositionInText);

            List<CellSwap<Value>> split = new();

            foreach (string s in str.Split(new []{delimiter}, StringSplitOptions.None)) {
                split.Add(new CellSwap<Value>(new Value.String(s)));
            }

            return new Value.Array(split);
        }),
        new BuiltinFunction("str_contains", 2, (context, values) => {
            if(values[0] is not Value.String(var str))
                throw new InvalidValueTypeException(typeof(Value.String), values[0].GetType(), context.Expression.PositionInText);
            if(values[1] is not Value.String(var what))
                throw new InvalidValueTypeException(typeof(Value.String), values[1].GetType(), context.Expression.PositionInText);

            return new Value.Boolean(str.Contains(what));
        }),
        new BuiltinFunction("str_replace", 3, (context, values) => {
            if(values[0] is not Value.String(var str))
                throw new InvalidValueTypeException(typeof(Value.String), values[0].GetType(), context.Expression.PositionInText);
            if(values[1] is not Value.String(var what))
                throw new InvalidValueTypeException(typeof(Value.String), values[1].GetType(), context.Expression.PositionInText);
            if(values[2] is not Value.String(var with))
                throw new InvalidValueTypeException(typeof(Value.String), values[2].GetType(), context.Expression.PositionInText);

            return new Value.String(str.Replace(what, with));
        }),
        new BuiltinFunction("str_trim", 1, (context, values) => {
            if(values[0] is not Value.String(var str))
                throw new InvalidValueTypeException(typeof(Value.String), values[0].GetType(), context.Expression.PositionInText);

            return new Value.String(str.Trim());
        }),
    };
}