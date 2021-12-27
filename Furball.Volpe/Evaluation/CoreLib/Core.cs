using System;
using System.Globalization;
using Furball.Volpe.Exceptions;

namespace Furball.Volpe.Evaluation.CoreLib {
    public class Core : CoreLibExtension {

        public override BuiltinFunction[] FunctionExports() => new BuiltinFunction[] {
            new BuiltinFunction ("int", 1, (context, values) =>
                {
                    Value value = values[0];

                    if (value is Value.Number)
                        return value;

                    if (value is Value.String str && double.TryParse(str.Value, out double converted))
                        return new Value.Number(converted);

                    throw new TypeConversionException(value, typeof(Value.Number),
                        context.Expression.PositionInText);
                }),

                new BuiltinFunction ("string", 1, (context, values) =>/**/
                {
                    Value value = values[0];

                    if (value is Value.String)
                        return value;

                    if (value is Value.Number n)
                        return new Value.String(n.Value.ToString(CultureInfo.InvariantCulture));

                    return new Value.String(value.Representation);
                }),

                new BuiltinFunction ("repr", 1, (context, values) => new Value.String(values[0].Representation)),

                new BuiltinFunction ("hook", 3, (context, values) =>
                {
                    if (values[0] is not Value.String(var name))
                        throw new InvalidValueTypeException(
                            typeof(Value.FunctionReference), values[0].GetType(), context.Expression.PositionInText);

                    if (values[1] is not Value.FunctionReference f1)
                        throw new InvalidValueTypeException(
                            typeof(Value.FunctionReference), values[1].GetType(), context.Expression.PositionInText);

                    if (values[2] is not Value.FunctionReference f2)
                        throw new InvalidValueTypeException(
                            typeof(Value.FunctionReference), values[2].GetType(), context.Expression.PositionInText);

                    context.Environment.HookVariableToGetterAndSetter(name, (f1, f2));
                    return Value.DefaultVoid;
                }),

                new BuiltinFunction ("type", 1, (context, values) =>
                {
                    return new Value.String(values[0] switch
                    {
                        Value.Number => "number",
                        Value.String => "string",
                        Value.Void => "void",
                        Value.FunctionReference => "function_reference",
                        Value.Array => "array",
                        Value.Boolean => "boolean",

                        _ => throw new InvalidOperationException(values[0].GetType().ToString())
                    });
                }),

                new BuiltinFunction ("invoke", 1, (context, values) =>
                {
                    if (values[0] is not Value.FunctionReference functionReference)
                        throw new InvalidValueTypeException(
                        typeof(Value.FunctionReference), values[0].GetType(),
                        context.Expression.PositionInText);

                    if (values.Length - 1 < functionReference.Function.ParameterCount)
                        throw new ParamaterCountMismatchException(functionReference.Name,
                                                                  functionReference.Function.ParameterCount,
                                                                  values.Length - 1, context.Expression.PositionInText);

                    return functionReference.Function.Invoke(context, values[1..]);
                }),
        };
    }
}
