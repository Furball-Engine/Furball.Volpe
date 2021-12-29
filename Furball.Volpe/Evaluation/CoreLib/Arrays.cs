using System.Collections.Generic;
using System.Linq;
using Furball.Volpe.Exceptions;
using Furball.Volpe.Memory;

namespace Furball.Volpe.Evaluation.CoreLib {
    public class Arrays : CoreLibExtension {
        public override BuiltinFunction[] FunctionExports() => new BuiltinFunction[] {
            new BuiltinFunction("arr_len", 1, (context, values) => {
                if (values[0] is not Value.Array(var arr))
                    throw new InvalidValueTypeException(typeof(Value.Array), values[0].GetType(), context.Expression.PositionInText);

                return new Value.Number(arr.Count);
            }),
            new BuiltinFunction("arr_append", 2, (context, values) => {
                if (values[0] is not Value.Array(var arr))
                    throw new InvalidValueTypeException(typeof(Value.Array), values[0].GetType(), context.Expression.PositionInText);

                arr.Add(new CellSwap<Value>(values[1]));

                return values[0];
            }),
            
            new BuiltinFunction("arr_append_range", 2, (context, values) => {
                if (values[0] is not Value.Array(var first))
                    throw new InvalidValueTypeException(typeof(Value.Array), values[0].GetType(), context.Expression.PositionInText);
                if (values[1] is not Value.Array(var second))
                    throw new InvalidValueTypeException(typeof(Value.Array), values[1].GetType(), context.Expression.PositionInText);

                first.AddRange(second);

                return values[0];
            }),
            
            new BuiltinFunction("arr_remove_range", 2, (context, values) => {
                if (values[0] is not Value.Array(var first))
                    throw new InvalidValueTypeException(typeof(Value.Array), values[0].GetType(), context.Expression.PositionInText);
                if (values[1] is not Value.Array(var second))
                    throw new InvalidValueTypeException(typeof(Value.Array), values[1].GetType(), context.Expression.PositionInText);

                foreach (CellSwap<Value> value in second) {
                    int index = first.FindIndex(x=> x.Value == value.Value);
                    first.RemoveAt(index);
                }

                return values[0];
            }),
            
            new BuiltinFunction("arr_remove_at", 2, (context, values) => {
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
            
            new BuiltinFunction("arr_remove", 2, (context, values) => {
                if (values[0] is not Value.Array(var first))
                    throw new InvalidValueTypeException(typeof(Value.Array), values[0].GetType(), context.Expression.PositionInText);

                int index = first.FindIndex(x=> x.Value == values[1]);
                first.RemoveAt(index);
                
                return values[0];
            }),
            
            new BuiltinFunction("arr_insert_at", 3, (context, values) => {
                if (values[0] is not Value.Array(var arr))
                    throw new InvalidValueTypeException(typeof(Value.Array), values[0].GetType(), context.Expression.PositionInText);

                if (values[1] is not Value.Number(var fIndex))
                    throw new InvalidValueTypeException(typeof(Value.Number), values[1].GetType(), context.Expression.PositionInText);

                int index = (int) fIndex;

                if (index >= arr.Count || index < 0)
                    throw new IndexOutOfBoundsException(arr, index, context.Expression.PositionInText);

                arr.Insert(index, new CellSwap<Value>(values[2]));

                return values[0];
            }),
            
            new BuiltinFunction("arr_map", 2, (context, values) =>
            {
                if(values[0] is not Value.Array(var first))
                    throw new InvalidValueTypeException(typeof(Value.Array), values[0].GetType(), context.Expression.PositionInText);
                if(values[1] is not Value.FunctionReference(_, var function))
                    throw new InvalidValueTypeException(typeof(Value.FunctionReference), values[1].GetType(), context.Expression.PositionInText);

                List<CellSwap<Value>> newArray = new List<CellSwap<Value>>(first.Count);

                for (int i = 0; i < first.Count; i++)
                {
                    Value value = function.Invoke(context, new Value[] {first[i].Value});
                    newArray.Add(new CellSwap<Value>(value));
                }

                return new Value.Array(newArray);   
            }),
            
            new BuiltinFunction("arr_concat", 2, (context, values) => {
                if(values[0] is not Value.Array(var first))
                    throw new InvalidValueTypeException(typeof(Value.Array), values[0].GetType(), context.Expression.PositionInText);
                if(values[1] is not Value.Array(var second))
                    throw new InvalidValueTypeException(typeof(Value.Array), values[1].GetType(), context.Expression.PositionInText);

                return new Value.Array(first.Concat(second).ToList());
            }),
        };
    }
}
