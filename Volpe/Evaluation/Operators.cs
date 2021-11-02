using System;
using System.Linq;
using System.Text;
using Volpe.Exceptions;

namespace Volpe.Evaluation
{
    public class Operators
    {
        public static Value Sum(Value rightValue, Value leftValue, EvaluatorContext context)
        {
            return (rightValue, leftValue) switch
            {
                (Value.Number(var n1), Value.Number(var n2)) => new Value.Number(n1 + n2),
                (Value.String(var n1), Value.String(var n2)) => new Value.String(n1 + n2),
                
                _ => throw new UndefinedOperationException(
                    "sum", rightValue.GetType(), leftValue.GetType(), context.Expression.PositionInText)
            };
        }
        
        public static Value Subtract(Value rightValue, Value leftValue, EvaluatorContext context)
        {
            return (rightValue, leftValue) switch
            {
                (Value.Number(var n1), Value.Number(var n2)) => new Value.Number(n1 - n2),
                (Value.String(var n1), Value.String(var n2)) => new Value.String(n1.Replace(n2, string.Empty)),
                
                _ => throw new UndefinedOperationException(
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
                
                _ => throw new UndefinedOperationException(
                    "multiplication", rightValue.GetType(), leftValue.GetType(), context.Expression.PositionInText)
            };
        }
        
        public static Value Divide(Value rightValue, Value leftValue, EvaluatorContext context)
        {
            return (rightValue, leftValue) switch
            {
                (Value.Number(var n1), Value.Number(var n2)) => new Value.Number(n1 / n2),
                
                _ => throw new UndefinedOperationException(
                    "division", rightValue.GetType(), leftValue.GetType(), context.Expression.PositionInText)
            };
        }
    }
}