namespace Volpe.Memory
{
    public class CellSwap<T>
    {
        public T Value { get; private set; }

        public void Swap(T value)
        {
            Value = value;
        } 
        
        public static implicit operator T(CellSwap<T> cell) => cell.Value;
        
        public CellSwap(T value)
        {
            Value = value;
        }
    }
}