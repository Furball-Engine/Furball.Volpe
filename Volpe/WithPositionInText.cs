namespace Volpe
{
    public struct WithPositionInText<T>
    {
        public PositionInText Position { get; init; }
        public T Value { get; init; }
    }
}