namespace Furball.Volpe; 

public struct PositionInText
{
    public static PositionInText Zero = new PositionInText {Column = 0, Row = 0};
        
    public int Row { get; set; }
    public int Column { get; set; }

    public override string ToString()
    {
        return $"PositionInText {{ Row = {Row}, Column = {Column} }}>";
    }
}