namespace Furball.Volpe.Memory; 

public class CellSwap<pT>
{
    public pT Value { get; private set; }

    public void Swap(pT value)
    {
        Value = value;
    } 
        
    public static implicit operator pT(CellSwap<pT> cell) => cell.Value;
        
    public CellSwap(pT value)
    {
        Value = value;
    }
}