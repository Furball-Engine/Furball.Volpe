using Volpe.Exceptions;

namespace Volpe.Evaluation
{
    public class Builtins
    {
        public static Value Sum(Value rightValue, Value leftValue, Evaluator.Context context)
        {
            return (rightValue, leftValue) switch
            {
                (Value.Number(var n1), Value.Number(var n2)) => new Value.Number(n1 + n2)
            };
        }
        
        public static Value Subtract(Value rightValue, Value leftValue, Evaluator.Context context)
        {
            return (rightValue, leftValue) switch
            {
                (Value.Number(var n1), Value.Number(var n2)) => new Value.Number(n1 - n2)
            };
        }
        
        public static Value Multiply(Value rightValue, Value leftValue, Evaluator.Context context)
        {
            return (rightValue, leftValue) switch
            {
                (Value.Number(var n1), Value.Number(var n2)) => new Value.Number(n1 * n2)
            };
        }
        
        public static Value Divide(Value rightValue, Value leftValue, Evaluator.Context context)
        {
            return (rightValue, leftValue) switch
            {
                (Value.Number(var n1), Value.Number(var n2)) => new Value.Number(n1 / n2)
            };
        }
    }
}