namespace Volpe.Evaluation
{
    public abstract record Value
    {
        public record Number(double Value) : Value;
    }
}