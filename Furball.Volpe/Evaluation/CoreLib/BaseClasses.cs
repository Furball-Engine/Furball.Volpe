using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
                    Value.String str = (Value.String)parameters[0];
                    return new Value.Number(str.Value.Length);
                }, 0)),
                ("to_upper", new Function.Builtin((context, parameters) => {
                        Value.String str = (Value.String)parameters[0];
                        return new Value.String(str.Value.ToUpper());
                    }, 0)),
                ("to_lower", new Function.Builtin((context, parameters) => {
                        Value.String str = (Value.String)parameters[0];
                        return new Value.String(str.Value.ToLower());
                    }, 0)),
                ("substr", new Function.Builtin((context, parameters) => {
                        Value.String str = (Value.String)parameters[0];
                        
                        if (parameters[1] is not Value.Number(var begin))
                            throw new InvalidValueTypeException(typeof(Value.Number), parameters[1].GetType(), context.Expression.PositionInText);
                        if (parameters[2] is not Value.Number(var length))
                            throw new InvalidValueTypeException(typeof(Value.Number), parameters[2].GetType(), context.Expression.PositionInText);

                        string substring = str.Value.Substring((int) begin, (int) length);

                        return new Value.String(substring);
                    }, 3)),
                ("split", new Function.Builtin((context, parameters) => {
                        Value.String str = (Value.String)parameters[0];
                        if (parameters[1] is not Value.String(var delimiter))
                            throw new InvalidValueTypeException(typeof(Value.String), parameters[1].GetType(), context.Expression.PositionInText);

                        List<CellSwap<Value>> split = new();

                        foreach (string s in str.Value.Split(delimiter)) {
                            split.Add(new CellSwap<Value>(new Value.String(s)));
                        }

                        return new Value.Array(split);
                    }, 2)),
                ("contains", new Function.Builtin((context, parameters) => {
                        Value.String str = (Value.String)parameters[0];
                        if (parameters[1] is not Value.String(var what))
                            throw new InvalidValueTypeException(typeof(Value.String), parameters[1].GetType(), context.Expression.PositionInText);

                        return new Value.Boolean(str.Value.Contains(what));

                    }, 1)),
                ("replace", new Function.Builtin((context, parameters) => {
                        Value.String str = (Value.String)parameters[0];
                        if (parameters[1] is not Value.String(var what))
                            throw new InvalidValueTypeException(typeof(Value.String), parameters[1].GetType(), context.Expression.PositionInText);
                        if (parameters[2] is not Value.String(var with))
                            throw new InvalidValueTypeException(typeof(Value.String), parameters[2].GetType(), context.Expression.PositionInText);

                        return new Value.String(str.Value.Replace(what, with));
                    }, 2)),
                ("trim", new Function.Builtin((context, parameters) => {
                        Value.String str = (Value.String)parameters[0];
                        return new Value.String(str.Value.Trim());
                    }, 0)),
            }) { }
        }
    
        public class ArrayClass : Class
        {
            public static readonly ArrayClass Default = new ArrayClass();
        
            public ArrayClass() : base("array", new (string, Function)[]
            {
                ("length", new Function.Builtin((context, parameters) => {
                        Value.Array arr = (Value.Array)parameters[0];

                        return new Value.Number(arr.Value.Count);
                    }, 0)),
                ("append", new Function.Builtin((context, parameters) => {
                        Value.Array arr = (Value.Array)parameters[0];

                        arr.Value.Add(new CellSwap<Value>(parameters[1]));

                        return new Value.Void();
                    }, 1)),
                ("iter", new Function.Builtin((context, parameters) =>
                {
                    Value.Array arr = (Value.Array)parameters[0];
                    ArrayIteratorInstance.TryGetConstructor(out var function);
                    Value value = function!.Invoke(context, new Value[] {arr});
                    value.Class = ArrayIteratorInstance;
                    return value;
                }, 1)),
                ("append_range", new Function.Builtin((context, parameters) => {
                        Value.Array arr = (Value.Array)parameters[0];

                        if (parameters[1] is not Value.Array(var toAdd))
                            throw new InvalidValueTypeException(typeof(Value.Array), parameters[1].GetType(), context.Expression.PositionInText);

                        arr.Value.AddRange(toAdd);

                        return new Value.Void();
                    }, 1)),
                ("remove_range", new Function.Builtin((context, parameters) => {
                        Value.Array arr = (Value.Array)parameters[0];

                        if (parameters[1] is not Value.Array(var toRemove))
                            throw new InvalidValueTypeException(typeof(Value.Array), parameters[1].GetType(), context.Expression.PositionInText);

                        foreach (CellSwap<Value> value in toRemove) {
                            int index = arr.Value.FindIndex(x=> x.Value == value.Value);
                            arr.Value.RemoveAt(index);
                        }

                        return new Value.Void();
                    }, 1)),
                ("remove_at", new Function.Builtin((context, parameters) => {
                        Value.Array arr = (Value.Array)parameters[0];

                        if (parameters[1] is not Value.Number(var fIndex))
                            throw new InvalidValueTypeException(typeof(Value.Number), parameters[1].GetType(), context.Expression.PositionInText);

                        int index = (int) fIndex;

                        if (index >= arr.Value.Count || index < 0)
                            throw new IndexOutOfBoundsException(arr.Value, index, context.Expression.PositionInText);

                        Value oldValue = arr.Value[index];
                        arr.Value.RemoveAt(index);

                        return new Value.Void();
                    }, 1)),
                ("remove", new Function.Builtin((context, parameters) => {
                        Value.Array arr = (Value.Array)parameters[0];

                        Value value = (Value) parameters[1];
                        int index = arr.Value.FindIndex(x=> x.Value == value);
                        arr.Value.RemoveAt(index);

                        return new Value.Void();
                    }, 1)),
                ("insert_at", new Function.Builtin((context, parameters) =>
                {
                    Value.Array arr = (Value.Array) parameters[0];

                        if (parameters[1] is not Value.Number(var fIndex))
                            throw new InvalidValueTypeException(typeof(Value.Number), parameters[1].GetType(), context.Expression.PositionInText);

                        int index = (int) fIndex;

                        if (index >= arr.Value.Count || index < 0)
                            throw new IndexOutOfBoundsException(arr.Value, index, context.Expression.PositionInText);

                        arr.Value.Insert(index, new CellSwap<Value>(parameters[2]));

                        return new Value.Void();
                    }, 2)),
                ("map", new Function.Builtin((context, parameters) => {
                        Value.Array arr = (Value.Array)parameters[0];

                        if (parameters[1] is not Value.FunctionReference(_, var function))
                            throw new InvalidValueTypeException(typeof(Value.FunctionReference), parameters[1].GetType(), context.Expression.PositionInText);

                        List<CellSwap<Value>> newArray = new List<CellSwap<Value>>(arr.Value.Count);

                        for (int i = 0; i < arr.Value.Count; i++)
                        {
                            Value value = function.Invoke(context, new Value[] {arr.Value[i].Value});
                            newArray.Add(new CellSwap<Value>(value));
                        }

                        return new Value.Array(newArray);
                    }, 1)),
                ("concat", new Function.Builtin((context, parameters) => {
                        Value.Array arr = (Value.Array)parameters[0];

                        if (parameters[1] is not Value.Array(var toConcat))
                            throw new InvalidValueTypeException(typeof(Value.Array), parameters[1].GetType(), context.Expression.PositionInText);

                        return new Value.Array(arr.Value.Concat(toConcat).ToList());
                    }, 1)),
            }) { }
        }

        public class Iterator : Class
        {
            public Iterator() : base("iterator", new (string, Function)[]
            {
                ("next", new Function.Builtin((context, parameters) =>
                {
                    return Value.DefaultVoid;
                }, 0))
            }) {}
        }

        public class ArrayIterator : Class
        {
            public ArrayIterator() : base("iterator", new (string, Function)[]
            {
                ("init", new Function.Builtin((context, parameters) =>
                {
                    if (parameters[0] is not Value.Array array)
                        throw new InvalidValueTypeException(typeof(Value.Array), parameters[0].GetType(), context.Expression.PositionInText);

                    return new Value.Object(new Dictionary<string, CellSwap<Value>>
                    {
                        { "_position", new CellSwap<Value>(new Value.Number(0)) },
                        { "_array", new CellSwap<Value>(array) }
                    });
                }, 1)),

                ("next", new Function.Builtin((context, parameters) =>
                {
                    Value.Object obj = (Value.Object)parameters[0];

                    Value.Number number = (Value.Number)obj.Value["_position"].Value;
                    Value.Array array = (Value.Array)obj.Value["_array"].Value;

                    if (number.Value >= array.Value.Count)
                        return Value.DefaultVoid;

                    Value value = array.Value[(int)number.Value];

                    obj.Value["_position"].Swap(new Value.Number(number.Value + 1));
                    return value;
                }, 1))
            },IteratorClassInstance)
            { }
        }


        public static Iterator IteratorClassInstance = new Iterator();
        public static ArrayIterator ArrayIteratorInstance = new ArrayIterator();

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