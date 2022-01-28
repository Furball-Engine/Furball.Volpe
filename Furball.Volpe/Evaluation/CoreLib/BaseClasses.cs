using System.Collections.Generic;
using System.Globalization;
using Furball.Volpe.Exceptions;
using Furball.Volpe.Memory;

namespace Furball.Volpe.Evaluation.CoreLib
{
    public static class BaseClasses
    {
        public class NumberClass : Class
        {
            public static readonly NumberClass Default = new NumberClass();
        
            public NumberClass() : base("number", new (string, Function)[]
            {
                ("to_string", new Function.Builtin((context, values) => 
                    new Value.String(((Value.Number)values[0]).Value.ToString(CultureInfo.InvariantCulture)), 1)),
            }) { }
        }
    
        public class StringClass : Class
        {
            public static readonly StringClass Default = new StringClass();
        
            public StringClass() : base("string", new (string, Function)[]
            {
                ("format", new Function.Builtin((context, values) => {
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
                }, 2)),
                ("length", new Function.Builtin((context, parameters) => {
                    Value.String str = parameters[0] as Value.String;

                    return new Value.Number(str.Value.Length);
                }, 0)),
                ("to_upper", new Function.Builtin((context, parameters) => {
                        Value.String str = parameters[0] as Value.String;

                        return new Value.String(str.Value.ToUpper());
                    }, 0)),
                ("to_lower", new Function.Builtin((context, parameters) => {
                        Value.String str = parameters[0] as Value.String;

                        return new Value.String(str.Value.ToLower());
                    }, 0)),
                ("substr", new Function.Builtin((context, parameters) => {
                        Value.String str = parameters[0] as Value.String;

                        if (parameters[1] is not Value.Number(var begin))
                            throw new InvalidValueTypeException(typeof(Value.Number), parameters[1].GetType(), context.Expression.PositionInText);
                        if (parameters[2] is not Value.Number(var length))
                            throw new InvalidValueTypeException(typeof(Value.Number), parameters[2].GetType(), context.Expression.PositionInText);

                        string substring = str.Value.Substring((int) begin, (int) length);

                        return new Value.String(substring);
                    }, 3)),
                ("split", new Function.Builtin((context, parameters) => {
                        Value.String str = parameters[0] as Value.String;

                        if (parameters[1] is not Value.String(var delimiter))
                            throw new InvalidValueTypeException(typeof(Value.Number), parameters[1].GetType(), context.Expression.PositionInText);

                        List<CellSwap<Value>> split = new();

                        foreach (string s in str.Value.Split(delimiter)) {
                            split.Add(new CellSwap<Value>(new Value.String(s)));
                        }

                        return new Value.Array(split);
                    }, 2)),
                ("contains", new Function.Builtin((context, parameters) => {
                        Value.String str = parameters[0] as Value.String;

                        if (parameters[1] is not Value.String(var what))
                            throw new InvalidValueTypeException(typeof(Value.Number), parameters[1].GetType(), context.Expression.PositionInText);

                        return new Value.Boolean(str.Value.Contains(what));

                    }, 1)),
                ("replace", new Function.Builtin((context, parameters) => {
                        Value.String str = parameters[0] as Value.String;

                        if (parameters[1] is not Value.String(var what))
                            throw new InvalidValueTypeException(typeof(Value.Number), parameters[1].GetType(), context.Expression.PositionInText);
                        if (parameters[2] is not Value.String(var with))
                            throw new InvalidValueTypeException(typeof(Value.Number), parameters[2].GetType(), context.Expression.PositionInText);

                        return new Value.String(str.Value.Replace(what, with));
                    }, 2)),
                ("trim", new Function.Builtin((context, parameters) => {
                        Value.String str = parameters[0] as Value.String;

                        return new Value.String(str.Value.Trim());
                    }, 0)),
            }) { }
        }
    
        public class ArrayClass : Class
        {
            public static readonly ArrayClass Default = new ArrayClass();
        
            public ArrayClass() : base("array", new (string, Function)[]
            {
            
            }) { }
        }
    
        public class ObjectClass : Class
        {
            public static readonly ObjectClass Default = new ObjectClass();
        
            public ObjectClass() : base("object", new (string, Function)[]
            {
            
            }) { }
        }
        
        public class BooleanClass : Class
        {
            public static readonly BooleanClass Default = new BooleanClass();
        
            public BooleanClass() : base("bool", new (string, Function)[]
            {
                
            }) { }
        }
    }
}