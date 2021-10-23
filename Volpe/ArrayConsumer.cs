using System;
using System.Collections.Immutable;

namespace Volpe
{
    public class ArrayConsumer<T> 
    {
        private ImmutableArray<T> _elements;
        private int _position;

        public ArrayConsumer(ImmutableArray<T> elements)
        {
            _elements = elements;
            _position = -1;
        }

        public virtual bool TryPeekNext(out T? value)
        {
            int nextPosition = _position + 1;
            return TryGetAtPosition(nextPosition, out value);
        }

        public TResult TryConsumeNextAndThen<TResult>(Func<bool, T?, TResult> function)
        {
            bool consumed = TryConsumeNext(out T? value);
            return function(consumed, value);
        }
        
        public virtual bool TryGetAtPosition(int position, out T? value)
        {
            value = default;
            
            if (position < 0 || position >= _elements.Length)
                return false;

            value = _elements[position];
            return true;
        }
        
        public virtual bool TryConsumeNext(out T value)
        {
            _position++;
            return TryGetAtPosition(_position, out value);
        }
    }
}