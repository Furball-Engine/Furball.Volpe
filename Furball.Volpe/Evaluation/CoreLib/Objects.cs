using Furball.Volpe.Exceptions;
using Furball.Volpe.Memory;

namespace Furball.Volpe.Evaluation.CoreLib; 

public class Objects : CoreLibExtension
{
    public override BuiltinFunction[] FunctionExports()
    {
        return new BuiltinFunction[]
        {
            new BuiltinFunction("obj_add", 3, (context, values) =>
            {
                if (values[0] is not Value.Object(var dict))
                    throw new InvalidValueTypeException(typeof(Value.Object), values[0].GetType(),
                                                        context.Expression.PositionInText);
                if (values[1] is not Value.String(var key))
                    throw new InvalidValueTypeException(typeof(Value.String), values[0].GetType(),
                                                        context.Expression.PositionInText);

                if (dict.ContainsKey(key))
                    throw new KeyAlreadyDefinedException(key, context.Expression.PositionInText);
                    
                dict.Add(key, new CellSwap<Value>(values[2]));

                return values[0];
            }),
                
            new BuiltinFunction("obj_has_key", 2, (context, values) =>
            {
                if (values[0] is not Value.Object(var dict))
                    throw new InvalidValueTypeException(typeof(Value.Object), values[0].GetType(),
                                                        context.Expression.PositionInText);
                if (values[1] is not Value.String(var key))
                    throw new InvalidValueTypeException(typeof(Value.String), values[0].GetType(),
                                                        context.Expression.PositionInText);

                return dict.ContainsKey(key) ? Value.DefaultTrue : Value.DefaultFalse;
            }),
                
            new BuiltinFunction("obj_get_or_add", 3, (context, values) =>
            {
                if (values[0] is not Value.Object(var dict))
                    throw new InvalidValueTypeException(typeof(Value.Object), values[0].GetType(),
                                                        context.Expression.PositionInText);
                if (values[1] is not Value.String(var key))
                    throw new InvalidValueTypeException(typeof(Value.String), values[0].GetType(),
                                                        context.Expression.PositionInText);

                if (dict.TryGetValue(key, out var value))
                    return value.Value;
                    
                dict.Add(key, new CellSwap<Value>(values[2]));

                return values[2];                
            }),
                
            new BuiltinFunction("obj_remove", 2, (context, values) =>
            {
                if (values[0] is not Value.Object(var dict))
                    throw new InvalidValueTypeException(typeof(Value.Object), values[0].GetType(),
                                                        context.Expression.PositionInText);
                if (values[1] is not Value.String(var key))
                    throw new InvalidValueTypeException(typeof(Value.String), values[0].GetType(),
                                                        context.Expression.PositionInText);

                return dict.Remove(key) ? Value.DefaultTrue : Value.DefaultFalse;
            })
        };
    }
}