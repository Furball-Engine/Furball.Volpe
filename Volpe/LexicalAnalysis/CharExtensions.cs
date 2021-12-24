namespace Volpe.LexicalAnalysis
{
    public static class CharExtensions
    {
        public static bool IsVolpeOperator(this char character)
        {
            return character is ('+' or '-' or '/' or '*' or '!' or '&' or '|' or '~' or '^' or '.' or '='
                or '>' or '<');
        }

        public static bool IsVolpeDigit(this char character)
        {
            return character is >= '0' and <= '9';
        }

        public static bool CanBeInVolpeLiteral(this char character)
        {
            return character is
                ((>= '¡' or (>= 'a' and <= 'z') or (>= '@' and <= 'Z') or (>= '0' and <= '9'))
                or '_' and not '$');
        }
    }
}