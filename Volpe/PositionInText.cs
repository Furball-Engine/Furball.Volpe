namespace Volpe
{
    public struct PositionInText
    {
        public int Row { get; set; }
        public int Column { get; set; }

        public override string ToString()
        {
            return $"<PositionInText {{ Row = {Row}, Column = {Column} }}>";
        }
    }
}