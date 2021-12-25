using System.Collections;
using System.Collections.Generic;
using Volpe.Evaluation;
using Volpe.Memory;

namespace Volpe.Exceptions
{
    public class IndexOutOfBoundsException : VolpeException
    {
        public int Index { get; }
        public IList Array { get; }
        
        public IndexOutOfBoundsException(IList array, int index, PositionInText positionInText) : base(positionInText)
        {
            Index = index;
            Array = array;
        }

        public override string Description => $"{Index} is not a valid index for an array of length {Array.Count}";
    }
}