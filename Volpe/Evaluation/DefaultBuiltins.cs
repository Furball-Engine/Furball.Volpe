using System;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using Volpe.Exceptions;

namespace Volpe.Evaluation
{
    public delegate Value FunctionInvokeCallback(EvaluatorContext context, Value[] parameters);
        
    public class BuiltinFunction
    {
        public string Identifier { get; }
        public int ParamCount { get; }
        public FunctionInvokeCallback Callback { get; }

        public BuiltinFunction(string identifier, int paramCount, FunctionInvokeCallback cb)
        {
            Identifier = identifier;
            ParamCount = paramCount;
            Callback = cb;
        }
    }
    
    public static class DefaultBuiltins
    {
        public static BuiltinFunction[] Math = new BuiltinFunction[]
        {
            new BuiltinFunction ("abs", 1, (context, values) =>
            {
                if (values[0] is not Value.Number n)
                    throw new InvalidValueTypeException(
                        typeof(Value.Number), values[0].GetType(), context.Expression.PositionInText);

                return new Value.Number(System.Math.Abs(n.Value));
            }),
            
            new BuiltinFunction ("cos", 1, (context, values) =>
            {
                if (values[0] is not Value.Number n)
                    throw new InvalidValueTypeException(
                        typeof(Value.Number), values[0].GetType(), context.Expression.PositionInText);

                return new Value.Number(System.Math.Cos(n.Value));
            }),
            
            new BuiltinFunction ("sin", 1, (context, values) =>
            {
                if (values[0] is not Value.Number n)
                    throw new InvalidValueTypeException(
                        typeof(Value.Number), values[0].GetType(), context.Expression.PositionInText);

                return new Value.Number(System.Math.Sin(n.Value));
            }),
            
            new BuiltinFunction ("tan", 1, (context, values) =>
            {
                if (values[0] is not Value.Number n)
                    throw new InvalidValueTypeException(
                        typeof(Value.Number), values[0].GetType(), context.Expression.PositionInText);

                return new Value.Number(System.Math.Tan(n.Value));
            }),
            
            new BuiltinFunction ("log", 1, (context, values) =>
            {
                if (values[0] is not Value.Number n)
                    throw new InvalidValueTypeException(
                        typeof(Value.Number), values[0].GetType(), context.Expression.PositionInText);

                return new Value.Number(System.Math.Log(n.Value));
            }),
            
            new BuiltinFunction ("log2", 1, (context, values) =>
            {
                if (values[0] is not Value.Number n)
                    throw new InvalidValueTypeException(
                        typeof(Value.Number), values[0].GetType(), context.Expression.PositionInText);

                return new Value.Number(System.Math.Log2(n.Value));
            }),
            
            new BuiltinFunction ("sqrt", 1, (context, values) =>
            {
                if (values[0] is not Value.Number(var n))
                    throw new InvalidValueTypeException(
                        typeof(Value.Number), values[0].GetType(), context.Expression.PositionInText);

                return new Value.Number(System.Math.Sqrt(n));
            }),
            
            new BuiltinFunction ("pow", 2, (context, values) =>
            {
                if (values[0] is not Value.Number(var x))
                    throw new InvalidValueTypeException(
                        typeof(Value.Number), values[0].GetType(), context.Expression.PositionInText);
                
                if (values[1] is not Value.Number(var n))
                    throw new InvalidValueTypeException(
                        typeof(Value.Number), values[1].GetType(), context.Expression.PositionInText);
                
                return new Value.Number(System.Math.Pow(x, n));
            }),
            
            new BuiltinFunction ("ceil", 1, (context, values) =>
            {
                if (values[0] is not Value.Number(var x))
                    throw new InvalidValueTypeException(
                        typeof(Value.Number), values[0].GetType(), context.Expression.PositionInText);

                return new Value.Number(System.Math.Ceiling(x));
            }),
            
            new BuiltinFunction ("floor", 1, (context, values) =>
            {
                if (values[0] is not Value.Number(var x))
                    throw new InvalidValueTypeException(
                        typeof(Value.Number), values[0].GetType(), context.Expression.PositionInText);

                return new Value.Number(System.Math.Floor(x));
            }),
            
            new BuiltinFunction ("round", 1, (context, values) =>
            {
                if (values[0] is not Value.Number(var x))
                    throw new InvalidValueTypeException(
                        typeof(Value.Number), values[0].GetType(), context.Expression.PositionInText);

                return new Value.Number(System.Math.Round(x));
            }),
        };

        public static BuiltinFunction[] Core = new BuiltinFunction[]
        {
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

                throw new TypeConversionException(value, typeof(Value.Number),
                    context.Expression.PositionInText);
            }),
            
            new BuiltinFunction ("repr", 1, (context, values) => new Value.String(values[0].Representation)),
            
            new BuiltinFunction ("hook", 3, (context, values) =>
            {
                if (values[0] is not Value.String(var name))
                    throw new InvalidValueTypeException(
                        typeof(Value.FunctionReference), values[0].GetType(), context.Expression.PositionInText);
                
                if (values[1] is not Value.FunctionReference f1)
                    throw new InvalidValueTypeException(
                        typeof(Value.FunctionReference), values[0].GetType(), context.Expression.PositionInText);
                
                if (values[2] is not Value.FunctionReference f2)
                    throw new InvalidValueTypeException(
                        typeof(Value.FunctionReference), values[1].GetType(), context.Expression.PositionInText);

                context.Scope.HookVariableToGetterAndSetter(name, (f1, f2));
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