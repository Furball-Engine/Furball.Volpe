using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Volpe.Exceptions;
using Volpe.Memory;

namespace Volpe.Evaluation
{
    public class Operators
    {
        public static Value Positive(Value leftValue, EvaluatorContext context)
        {
            return leftValue switch
            {
                Value.Number(var n1) => new Value.Number(+n1),
                
                _ => throw new UndefinedPrefixOperationException(
                    nameof(Positive), leftValue.GetType(), context.Expression.PositionInText)
            };
        }
        
        public static Value ArrayAccess(Value leftValue, Value rightValue, EvaluatorContext context)
        {
            CellSwap<Value> ElementAt(List<CellSwap<Value>> array, int index)
            {
                if (index >= array.Count || index < 0)
                    throw new IndexOutOfBoundsException(array, index, context.Expression.PositionInText);

                return array[index];
            }
            
            return (leftValue, rightValue) switch
            {
                (Value.Array(var arr), Value.Number(var n1)) => new Value.ValueReference(ElementAt(arr, (int)n1)),
                
                _ => throw new UndefinedPrefixOperationException(
                    nameof(Positive), leftValue.GetType(), context.Expression.PositionInText)
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
        
        public static Value GreaterThan(Value leftValue, Value rightValue, EvaluatorContext context)
        {
            return (leftValue, rightValue) switch
            {
                (Value.Number(var n1), Value.Number(var n2)) => n1 > n2 ? Value.DefaultTrue : Value.DefaultFalse,
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
                (Value.String(var n1), Value.String(var n2)) => 
                    string.Compare(n1, n2, StringComparison.Ordinal) <= 0 ? Value.DefaultTrue : Value.DefaultFalse,

                _ => throw new UndefinedInfixOperationException(
                    nameof(LessThanOrEqual), rightValue.GetType(), leftValue.GetType(), context.Expression.PositionInText)
            };
        }
        
        public static Value Negative(Value leftValue, EvaluatorContext context)
        {
            return leftValue switch
            {
                Value.Number(var n1) => new Value.Number(-n1),
                
                _ => throw new UndefinedPrefixOperationException(
                    "negative", leftValue.GetType(), context.Expression.PositionInText)
            };
        }
        
        public static Value Sum(Value rightValue, Value leftValue, EvaluatorContext context)
        {
            Value AppendArrayToArray(Value.Array arr1, Value.Array arr2)
            {
                arr1.Value.AddRange(arr2.Value);
                return arr1;
            }
            
            return (rightValue, leftValue) switch
            {
                (Value.Number(var n1), Value.Number(var n2)) => new Value.Number(n1 + n2),
                (Value.String(var n1), Value.String(var n2)) => new Value.String(n1 + n2),
                (Value.Array n1, Value.Array n2) => AppendArrayToArray(n1, n2),
                
                _ => throw new UndefinedInfixOperationException(
                    nameof(Sum), rightValue.GetType(), leftValue.GetType(), context.Expression.PositionInText)
            };
        }
        
        public static Value Subtract(Value rightValue, Value leftValue, EvaluatorContext context)
        {
            return (rightValue, leftValue) switch
            {
                (Value.Number(var n1), Value.Number(var n2)) => new Value.Number(n1 - n2),
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
            
            return (rightValue, leftValue) switch
            {
                (Value.Number(var n1), Value.Number(var n2)) => new Value.Number(n1 * n2),
                (Value.String(var n1), Value.Number(var n2)) => new Value.String(MultiplyString(n1, (int)n2)),
                
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
                
                (Value.Number(var n1), Value.Number(var n2)) => new Value.Number(n1 / n2),
                
                _ => throw new UndefinedInfixOperationException(
                    nameof(Divide), rightValue.GetType(), leftValue.GetType(), context.Expression.PositionInText)
            };
        }
    }
}