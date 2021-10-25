

using Volpe.Exceptions;
using Volpe.SyntaxAnalysis;

namespace Volpe.Evaluation
{
    public class Evaluator
    {
        public readonly struct Context
        {
            public Expression RootExpression { get; init; }
            public Scope Scope { get; init; }
        } 
        
        private Value EvaluateInfixExpression(ExpressionValue.InfixExpression expr, Context context)
        {
            Value leftValue = Evaluate(expr.Left, context.Scope);
            Value rightValue = Evaluate(expr.Right, context.Scope);

            return expr.Operator switch
            {
                ExpressionOperator.Add => Builtins.Sum(leftValue, rightValue, context),
                ExpressionOperator.Sub => Builtins.Subtract(leftValue, rightValue, context),
                ExpressionOperator.Mul => Builtins.Multiply(leftValue, rightValue, context),
                ExpressionOperator.Div => Builtins.Divide(leftValue, rightValue, context)
            };
        }

        private Value EvaluateAssignment(ExpressionValue.Assignment expr, Context context)
        {
            Value value = Evaluate(expr.Expression, context.Scope);
            context.Scope.SetVariableValue(expr.VariableName, value);

            return value;
        }
        
        private Value EvaluateVariable(ExpressionValue.Variable expr, Context context)
        {
            Value? value;
            if (!context.Scope.TryGetVariableValue(expr.Name, out value))
                throw new VariableNotFoundException(context.RootExpression.PositionInText);
                
            return value;
        }
        
        public Value Evaluate(Context context)
        {
            return context.RootExpression.Value switch
            {
                ExpressionValue.InfixExpression expr => EvaluateInfixExpression(expr, context),
                ExpressionValue.Number(var v) => new Value.Number(v),
                ExpressionValue.Assignment expr => EvaluateAssignment(expr, context)
            };
        }

        public Value Evaluate(Expression expression, Scope scope) =>
            Evaluate(new Context {Scope = scope, RootExpression = expression});
    }
}