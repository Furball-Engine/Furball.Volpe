using System;
using System.Linq;
using System.Text;
using Volpe.Exceptions;

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
                    "positive", leftValue.GetType(), context.Expression.PositionInText)
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
            return (rightValue, leftValue) switch
            {
                (Value.Number(var n1), Value.Number(var n2)) => new Value.Number(n1 + n2),
                (Value.String(var n1), Value.String(var n2)) => new Value.String(n1 + n2),
                
                _ => throw new UndefinedInfixOperationException(
                    "sum", rightValue.GetType(), leftValue.GetType(), context.Expression.PositionInText)
            };
        }
        
        public static Value Subtract(Value rightValue, Value leftValue, EvaluatorContext context)
        {
            return (rightValue, leftValue) switch
            {
                (Value.Number(var n1), Value.Number(var n2)) => new Value.Number(n1 - n2),
                (Value.String(var n1), Value.String(var n2)) => new Value.String(n1.Replace(n2, string.Empty)),
                
                _ => throw new UndefinedInfixOperationException(
                    "subtraction", rightValue.GetType(), leftValue.GetType(), context.Expression.PositionInText)
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
                    "multiplication", rightValue.GetType(), leftValue.GetType(), context.Expression.PositionInText)
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
                    "division", rightValue.GetType(), leftValue.GetType(), context.Expression.PositionInText)
            };
        }
    }
}