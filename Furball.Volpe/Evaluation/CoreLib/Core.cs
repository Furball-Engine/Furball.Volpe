using System;
using System.Globalization;
using System.Linq;
using Furball.Volpe.Exceptions;

namespace Furball.Volpe.Evaluation.CoreLib; 

public class Core : CoreLibExtension {
    public override BuiltinFunction[] FunctionExports() => new BuiltinFunction[] {
        new("int", 1, (context, values) => {
            Value value = values[0];

            if (value is Value.Number)
                return value;

            if (value is Value.String str && double.TryParse(str.Value, out double converted))
                return new Value.Number(converted);

            throw new TypeConversionException(value, typeof(Value.Number),
                                              context.Expression.PositionInText);
        }),

        new("string", 1, (_, values) => {
            Value value = values[0];

            if (value is Value.String)
                return value;

            if (value is Value.Number n)
                return new Value.String(n.Value.ToString(CultureInfo.InvariantCulture));

            return new Value.String(value.Representation);
        }),

        new("repr", 1, (_, values) => new Value.String(values[0].Representation)),

        new("hook", 3, (context, values) => {
            if (values[0] is not Value.String(var name))
                throw new InvalidValueTypeException(
                typeof(Value.FunctionReference), values[0].GetType(), context.Expression.PositionInText);

            if (values[1] is not Value.FunctionReference f1)
                throw new InvalidValueTypeException(
                typeof(Value.FunctionReference), values[1].GetType(), context.Expression.PositionInText);

            if (values[2] is not Value.FunctionReference f2)
                throw new InvalidValueTypeException(
                typeof(Value.FunctionReference), values[2].GetType(), context.Expression.PositionInText);

            context.Environment.SetVariable(new HookedVariable(name, f1.Function, f2.Function));
            return Value.DefaultVoid;
        }),

        new("type", 1, (_, values) => {
            return new Value.String(values[0] switch
            {
                Value.Number            => "number",
                Value.String            => "string",
                Value.Void              => "void",
                Value.FunctionReference => "function_reference",
                Value.Array             => "array",
                Value.Object            => "object",
                Value.Boolean           => "boolean",

                _ => throw new InvalidOperationException(values[0].GetType().ToString())
            });
        }),

        new("invoke", 1, (context, values) => {
            if (values[0] is not Value.FunctionReference functionReference)
                throw new InvalidValueTypeException(
                typeof(Value.FunctionReference), values[0].GetType(),
                context.Expression.PositionInText);

            if (values.Length - 1 < functionReference.Function.ParameterCount)
                throw new ParamaterCountMismatchException(functionReference.Name,
                                                          functionReference.Function.ParameterCount,
                                                          values.Length - 1, context.Expression.PositionInText);

            return functionReference.Function.Invoke(context, values.Skip(1).ToArray());
        }),

        new("error", 1, (context, values) => {
            if (values[0] is not Value.String(var first))
                throw new InvalidValueTypeException(typeof(Value.String), values[0].GetType(), context.Expression.PositionInText);

            throw new UserThrownException(context.Expression.PositionInText, first);
        }),
            
        new("clone", 1, (_, values) =>
        {
            switch (values[0])
            {
                case Value.Array arr:
                    return arr.Copy();
                    
                case Value.Object obj:
                    return obj.Copy();

                default:
                    return values[0] with { };
            }
        })
    };
}