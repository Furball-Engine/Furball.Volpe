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
                ("format", new Function.Builtin((context, values) =>
                {
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
                }, 2))
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
        
            public BooleanClass() : base("string", new (string, Function)[]
            {
                
            }) { }
        }
    }
}