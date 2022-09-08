using Furball.Volpe.Exceptions;

namespace Furball.Volpe.Evaluation.CoreLib; 

public class Objects : CoreLibExtension
{
    public override BuiltinFunction[] FunctionExports()
    {
        return new BuiltinFunction[]
        {
            new("obj_add", 3, (context, values) =>
            {
                if (values[0] is not Value.Object(var dict))
                    throw new InvalidValueTypeException(typeof(Value.Object), values[0].GetType(),
                                                        context.Expression.PositionInText);
                if (values[1] is not Value.String(var key))
                    throw new InvalidValueTypeException(typeof(Value.String), values[0].GetType(),
                                                        context.Expression.PositionInText);

                if (dict.ContainsKey(key))
                    throw new KeyAlreadyDefinedException(key, context.Expression.PositionInText);
                    
                dict.Add(key, values[2]);

                return values[0];
            }),
                
            new("obj_has_key", 2, (context, values) =>
            {
                if (values[0] is not Value.Object(var dict))
                    throw new InvalidValueTypeException(typeof(Value.Object), values[0].GetType(),
                                                        context.Expression.PositionInText);
                if (values[1] is not Value.String(var key))
                    throw new InvalidValueTypeException(typeof(Value.String), values[0].GetType(),
                                                        context.Expression.PositionInText);

                return dict.ContainsKey(key) ? Value.DefaultTrue : Value.DefaultFalse;
            }),
                
            new("obj_get_or_add", 3, (context, values) =>
            {
                if (values[0] is not Value.Object(var dict))
                    throw new InvalidValueTypeException(typeof(Value.Object), values[0].GetType(),
                                                        context.Expression.PositionInText);
                if (values[1] is not Value.String(var key))
                    throw new InvalidValueTypeException(typeof(Value.String), values[0].GetType(),
                                                        context.Expression.PositionInText);

                if (dict.TryGetValue(key, out var value))
                    return value;
                    
                dict.Add(key, values[2]);

                return values[2];                
            }),
                
            new("obj_remove", 2, (context, values) =>
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