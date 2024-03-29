using System.Collections.Generic;
using System.Linq;
using Furball.Volpe.Exceptions;

namespace Furball.Volpe.Evaluation.CoreLib; 

public class Arrays : CoreLibExtension {
    public override BuiltinFunction[] FunctionExports() => new BuiltinFunction[] {
        new("arr_len", 1, (context, values) => {
            if (values[0] is not Value.Array(var arr))
                throw new InvalidValueTypeException(typeof(Value.Array), values[0].GetType(), context.Expression.PositionInText);

            return new Value.Number(arr.Count);
        }),
        new("arr_append", 2, (context, values) => {
            if (values[0] is not Value.Array(var arr))
                throw new InvalidValueTypeException(typeof(Value.Array), values[0].GetType(), context.Expression.PositionInText);

            arr.Add(values[1]);
            return values[0];
        }),
            
        new("arr_append_range", 2, (context, values) => {
            if (values[0] is not Value.Array(var first))
                throw new InvalidValueTypeException(typeof(Value.Array), values[0].GetType(), context.Expression.PositionInText);
            if (values[1] is not Value.Array(var second))
                throw new InvalidValueTypeException(typeof(Value.Array), values[1].GetType(), context.Expression.PositionInText);

            first.AddRange(second);

            return values[0];
        }),
            
        new("arr_remove_range", 2, (context, values) => {
            if (values[0] is not Value.Array(var first))
                throw new InvalidValueTypeException(typeof(Value.Array), values[0].GetType(), context.Expression.PositionInText);
            if (values[1] is not Value.Array(var second))
                throw new InvalidValueTypeException(typeof(Value.Array), values[1].GetType(), context.Expression.PositionInText);

            foreach (Value value in second) {
                int index = first.FindIndex(x=> x == value);
                first.RemoveAt(index);
            }

            return values[0];
        }),
            
        new("arr_remove_at", 2, (context, values) => {
            if (values[0] is not Value.Array(var arr))
                throw new InvalidValueTypeException(typeof(Value.Array), values[0].GetType(), context.Expression.PositionInText);
            if (values[1] is not Value.Number(var fIndex))
                throw new InvalidValueTypeException(typeof(Value.Number), values[1].GetType(), context.Expression.PositionInText);

            int index = (int) fIndex;

            if (index >= arr.Count || index < 0)
                throw new IndexOutOfBoundsException(arr, index, context.Expression.PositionInText);

            Value oldValue = arr[index];
            arr.RemoveAt(index);

            return oldValue;
        }),
            
        new("arr_remove", 2, (context, values) => {
            if (values[0] is not Value.Array(var first))
                throw new InvalidValueTypeException(typeof(Value.Array), values[0].GetType(), context.Expression.PositionInText);

            int index = first.FindIndex(x=> x == values[1]);
            first.RemoveAt(index);
                
            return values[0];
        }),
            
        new("arr_insert_at", 3, (context, values) => {
            if (values[0] is not Value.Array(var arr))
                throw new InvalidValueTypeException(typeof(Value.Array), values[0].GetType(), context.Expression.PositionInText);

            if (values[1] is not Value.Number(var fIndex))
                throw new InvalidValueTypeException(typeof(Value.Number), values[1].GetType(), context.Expression.PositionInText);

            int index = (int) fIndex;

            if (index >= arr.Count || index < 0)
                throw new IndexOutOfBoundsException(arr, index, context.Expression.PositionInText);

            arr.Insert(index, values[2]);

            return values[0];
        }),
            
        new("arr_map", 2, (context, values) =>
        {
            if(values[0] is not Value.Array(var first))
                throw new InvalidValueTypeException(typeof(Value.Array), values[0].GetType(), context.Expression.PositionInText);
            if(values[1] is not Value.FunctionReference(_, var function))
                throw new InvalidValueTypeException(typeof(Value.FunctionReference), values[1].GetType(), context.Expression.PositionInText);

            List<Value> newArray = new List<Value>(first.Count);

            for (int i = 0; i < first.Count; i++)
            {
                Value value = function.Invoke(context, new Value[] {first[i]});
                newArray.Add(value);
            }

            return new Value.Array(newArray);   
        }),
            
        new("arr_concat", 2, (context, values) => {
            if(values[0] is not Value.Array(var first))
                throw new InvalidValueTypeException(typeof(Value.Array), values[0].GetType(), context.Expression.PositionInText);
            if(values[1] is not Value.Array(var second))
                throw new InvalidValueTypeException(typeof(Value.Array), values[1].GetType(), context.Expression.PositionInText);

            return new Value.Array(first.Concat(second).ToList());
        }),
    };
}