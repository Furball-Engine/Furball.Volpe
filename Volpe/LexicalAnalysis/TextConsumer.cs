using System;
using System.Collections.Immutable;

namespace Volpe.LexicalAnalysis
{
    public class TextConsumer : ArrayConsumer<char>
    {
        public PositionInText PositionInText => _positionInText;

        private PositionInText _positionInText;
        
        public TextConsumer(ImmutableArray<char> elements) : base(elements)
        {
        }

        public override bool TryConsumeNext(out char value)
        {
            bool consumed = base.TryConsumeNext(out value);

            if (!consumed)
                return false;
            
            switch (value)
            {
                case '\n':
                    _positionInText.Row = 0;
                    ++_positionInText.Column;
                    break;
                
                case var c when !char.IsControl(c):
                    ++_positionInText.Row;
                    break;
            }

            return true;
        }
    }
}