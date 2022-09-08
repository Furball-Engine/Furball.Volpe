using System;
using System.Collections.Generic;
using System.Text;
using Furball.Volpe.Exceptions;

namespace Furball.Volpe.Evaluation; 

public class Operators
{
    public static Value Positive(Value leftValue, EvaluatorContext context)
    {
        Value.Array PositiveArray(Value.Array source)
        {
            List<Value> newArray = new List<Value>(source.Value.Count);

            for (int i = 0; i < source.Value.Count; i++)
                newArray.Add(Positive(source.Value[i], context));

            return new Value.Array(newArray);
        }
            
        return leftValue switch
        {
            Value.Number(var n1) => new Value.Number(+n1),
            Value.Array n1       => PositiveArray(n1),
                
            _ => throw new UndefinedPrefixOperationException(
                 nameof(Positive), leftValue.GetType(), context.Expression.PositionInText)
        };
    }
        
    public static Value ArrayAccess(Value leftValue, Value rightValue, EvaluatorContext context)
    {
        IValueRef ElementAt(List<Value> array, int index)
        {
            if (index >= array.Count || index < 0)
                throw new IndexOutOfBoundsException(array, index, context.Expression.PositionInText);

            return new ArrayValueRef(array, index);
        }

        IValueRef ObjectElementAt(Dictionary<string, Value> dict, string key) => new ObjectValueRef(dict, key, context.Expression.PositionInText);
            
        return (leftValue, rightValue) switch
        {
            (Value.Array(var arr), Value.Number(var n1)) => new Value.ValueReference(ElementAt(arr, (int)n1)),
            (Value.Object(var dict), Value.String(var n1)) => new Value.ValueReference(ObjectElementAt(dict, n1)),
                
            _ => throw new UndefinedPrefixOperationException(
                 nameof(ArrayAccess), leftValue.GetType(), context.Expression.PositionInText)
        };
    }

    public static Value LogicalAnd(Value leftValue, Value rightValue, EvaluatorContext context)
    {
        return (rightValue, leftValue) switch
        {
            (Value.Boolean(var n1), Value.Boolean(var n2)) => n1 && n2 ? Value.DefaultTrue : Value.DefaultFalse,
                
            _ => throw new UndefinedInfixOperationException(
                 nameof(LogicalAnd), rightValue.GetType(), leftValue.GetType(), context.Expression.PositionInText)
        };
    }
        
    public static Value LogicalOr(Value leftValue, Value rightValue, EvaluatorContext context)
    {
        return (rightValue, leftValue) switch
        {
            (Value.Boolean(var n1), Value.Boolean(var n2)) => n1 || n2 ? Value.DefaultTrue : Value.DefaultFalse,
                
            _ => throw new UndefinedInfixOperationException(
                 nameof(LogicalOr), rightValue.GetType(), leftValue.GetType(), context.Expression.PositionInText)
        };
    }
        
    public static Value Eq(Value leftValue, Value rightValue, EvaluatorContext context)
    {
        return leftValue == rightValue ? Value.DefaultTrue : Value.DefaultFalse;
    }
        
    public static Value NotEq(Value leftValue, Value rightValue, EvaluatorContext context)
    {
        return leftValue != rightValue ? Value.DefaultTrue : Value.DefaultFalse;
    }
        
    public static Value GreaterThan(Value leftValue, Value rightValue, EvaluatorContext context)
    {
        return (leftValue, rightValue) switch
        {
            (Value.Number(var n1), Value.Number(var n2)) => n1 > n2 ? Value.DefaultTrue : Value.DefaultFalse,
            (Value.Byte(var n1), Value.Byte(var n2))     => n1 > n2 ? Value.DefaultTrue : Value.DefaultFalse,
            (Value.String(var n1), Value.String(var n2)) =>
                string.Compare(n1, n2, StringComparison.Ordinal) > 0 ? Value.DefaultTrue : Value.DefaultFalse,

            _ => throw new UndefinedInfixOperationException(
                 nameof(GreaterThan), rightValue.GetType(), leftValue.GetType(), context.Expression.PositionInText)
        };
    }

    public static Value LessThan(Value leftValue, Value rightValue, EvaluatorContext context)
    {
        return (leftValue, rightValue) switch
        {
            (Value.Number(var n1), Value.Number(var n2)) => n1 < n2 ? Value.DefaultTrue : Value.DefaultFalse,
            (Value.Byte(var n1), Value.Byte(var n2)) => n1 < n2 ? Value.DefaultTrue : Value.DefaultFalse,
            (Value.String(var n1), Value.String(var n2)) => 
                string.Compare(n1, n2, StringComparison.Ordinal) < 0 ? Value.DefaultTrue : Value.DefaultFalse,

            _ => throw new UndefinedInfixOperationException(
                 nameof(LessThan), rightValue.GetType(), leftValue.GetType(), context.Expression.PositionInText)
        };
    }
        
    public static Value GreaterThanOrEqual(Value leftValue, Value rightValue, EvaluatorContext context)
    {
        return (leftValue, rightValue) switch
        {
            (Value.Number(var n1), Value.Number(var n2)) => n1 >= n2 ? Value.DefaultTrue : Value.DefaultFalse,
            (Value.Byte(var n1), Value.Byte(var n2))     => n1 >= n2 ? Value.DefaultTrue : Value.DefaultFalse,
            (Value.String(var n1), Value.String(var n2)) => 
                string.Compare(n1, n2, StringComparison.Ordinal) >= 0 ? Value.DefaultTrue : Value.DefaultFalse,

            _ => throw new UndefinedInfixOperationException(
                 nameof(GreaterThanOrEqual), rightValue.GetType(), leftValue.GetType(), context.Expression.PositionInText)
        };
    }
        
    public static Value LessThanOrEqual(Value leftValue, Value rightValue, EvaluatorContext context)
    {
        return (leftValue, rightValue) switch
        {
            (Value.Number(var n1), Value.Number(var n2)) => n1 <= n2 ? Value.DefaultTrue : Value.DefaultFalse,
            (Value.Byte(var n1), Value.Byte(var n2))     => n1 <= n2 ? Value.DefaultTrue : Value.DefaultFalse,
            (Value.String(var n1), Value.String(var n2)) => 
                string.Compare(n1, n2, StringComparison.Ordinal) <= 0 ? Value.DefaultTrue : Value.DefaultFalse,

            _ => throw new UndefinedInfixOperationException(
                 nameof(LessThanOrEqual), rightValue.GetType(), leftValue.GetType(), context.Expression.PositionInText)
        };
    }
        
    public static Value Not(Value leftValue, EvaluatorContext context)
    {
        return leftValue switch
        {
            Value.Boolean(var n1) => new Value.Boolean(!n1),
                
            _ => throw new UndefinedPrefixOperationException(
                 nameof(Not), leftValue.GetType(), context.Expression.PositionInText)
        };
    }
        
    public static Value Negative(Value leftValue, EvaluatorContext context)
    {
        Value NegateArray(Value.Array arr1)
        {
            List<Value> newArray = new List<Value>(arr1.Value.Count);

            for (int i = 0; i < arr1.Value.Count; i++)
                newArray.Add(Negative(arr1.Value[i], context));
                
            return new Value.Array(newArray);
        }
            
        return leftValue switch
        {
            Value.Number(var n1) => new Value.Number(-n1),
            Value.Array n1       => NegateArray(n1),
                
            _ => throw new UndefinedPrefixOperationException(
                 nameof(Negative), leftValue.GetType(), context.Expression.PositionInText)
        };
    }
        
    public static Value Append(Value rightValue, Value leftValue, EvaluatorContext context)
    {
        Value AppendArrayToArray(Value.Array arr1, Value.Array arr2)
        {
            arr1 = arr1.Copy();

            for (int i = 0; i < arr2.Value.Count; i++)
                arr1.Value.Add(arr2.Value[i]);

            return arr1;
        }
            
        Value AppendObjectToObject(Value.Object obj1, Value.Object obj2)
        {
            obj1 = obj1.Copy();

            foreach (var key in obj2.Value.Keys)
            {
                if (obj1.Value.ContainsKey(key))
                    throw new KeyAlreadyDefinedException(key, context.Expression.PositionInText);
                    
                obj1.Value.Add(key, obj2.Value[key]);
            }

            return obj1;
        }
            
        return (rightValue, leftValue) switch
        {
            (Value.String(var n1), Value.String(var n2)) => new Value.String(n1 + n2),
            (Value.Array n1, Value.Array n2)             => AppendArrayToArray(n1, n2),
            (Value.Object n1, Value.Object n2)           => AppendObjectToObject(n1, n2),
                
            _ => throw new UndefinedInfixOperationException(
                 nameof(Append), rightValue.GetType(), leftValue.GetType(), context.Expression.PositionInText)
        };
    }
        
    public static Value Sum(Value rightValue, Value leftValue, EvaluatorContext context)
    {
        Value.Array AddTwoArrays(Value.Array n1, Value.Array n2)
        {
            if (n1.Value.Count != n2.Value.Count)
                throw new OperatorDomainException(context.Expression.PositionInText,
                                                  "The arrays do not have the same length");
                    
            int                   length   = n1.Value.Count;
            List<Value> newArray = new List<Value>(length);

            for (int i = 0; i < length; i++)
                newArray.Add(Sum(n1.Value[i], n2.Value[i], context));

            return new Value.Array(newArray);
        }

        return (rightValue, leftValue) switch
        {
            (Value.Number(var n1), Value.Number(var n2)) => new Value.Number(n1 + n2),
            (Value.Byte(var n1), Value.Byte(var n2))     => new Value.Byte((byte)(n1 + n2)),
            (Value.Array n1, Value.Array n2)             => AddTwoArrays(n1, n2),
                
            _ => throw new UndefinedInfixOperationException(
                 nameof(Sum), rightValue.GetType(), leftValue.GetType(), context.Expression.PositionInText)
        };
    }


    public static Value Subtract(Value rightValue, Value leftValue, EvaluatorContext context)
    {
        return (rightValue, leftValue) switch
        {
            (Value.Number(var n1), Value.Number(var n2)) => new Value.Number(n1 - n2),
            (Value.Byte(var n1), Value.Byte(var n2))     => new Value.Byte((byte)(n1 - n2)),
            (Value.String(var n1), Value.String(var n2)) => new Value.String(n1.Replace(n2, string.Empty)),
                
            _ => throw new UndefinedInfixOperationException(
                 nameof(Subtract), rightValue.GetType(), leftValue.GetType(), context.Expression.PositionInText)
        };
    }
        
    public static Value Multiply(Value rightValue, Value leftValue, EvaluatorContext context)
    {
        string MultiplyString(string source, int count)
        {
            if (count < 0)
                throw new OperatorDomainException(context.Expression.PositionInText,
                                                  "A string cannot be multiplied by a negative integer");

            StringBuilder stringBuilder = new StringBuilder(count * source.Length);

            for (int i = 0; i < count; i++)
                stringBuilder.Append(source);

            return stringBuilder.ToString();
        }
            
        Value.Array MultiplyArray(Value.Array source, Value left)
        {
            List<Value> newArray = new List<Value>(source.Value.Count);

            for (int i = 0; i < source.Value.Count; i++)
                newArray.Add(Multiply(source.Value[i], left, context));

            return new Value.Array(newArray);
        }
            
        return (rightValue, leftValue) switch
        {
            (Value.Number(var n1), Value.Number(var n2)) => new Value.Number(n1 * n2),
            (Value.Byte(var n1), Value.Byte(var n2))     => new Value.Byte((byte)(n1 * n2)),
            (Value.String(var n1), Value.Number(var n2)) => new Value.String(MultiplyString(n1, (int)n2)),
            (Value.Array n1, var n2)                     => MultiplyArray(n1, n2),
            (var n2, Value.Array n1)                     => MultiplyArray(n1, n2),
                
            _ => throw new UndefinedInfixOperationException(
                 nameof(Multiply), rightValue.GetType(), leftValue.GetType(), context.Expression.PositionInText)
        };
    }
        
    public static Value Divide(Value rightValue, Value leftValue, EvaluatorContext context)
    {
        return (rightValue, leftValue) switch
        {
            (Value.Number, Value.Number(0)) => 
                throw new OperatorDomainException(context.Expression.PositionInText, "cannot divide by zero"),
            (Value.Byte, Value.Byte(0)) => 
                throw new OperatorDomainException(context.Expression.PositionInText, "cannot divide by zero"),
                
            (Value.Number(var n1), Value.Number(var n2)) => new Value.Number(n1 / n2),
            (Value.Byte(var n1), Value.Byte(var n2))     => new Value.Byte((byte)(n1 / n2)),
                
            _ => throw new UndefinedInfixOperationException(
                 nameof(Divide), rightValue.GetType(), leftValue.GetType(), context.Expression.PositionInText)
        };
    }
}