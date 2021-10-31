using System;
using System.Collections.Immutable;
using Volpe.LexicalAnalysis;

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
            LastConsumed = default;
        }

        public T? LastConsumed { get; private set; }

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

        public bool SkipOne() => TryConsumeNext(out _);

        public void SkipTill(Func<T, bool> predicate)
        {
            for (;;)
            {
                T? t;
                if (!TryPeekNext(out t))
                    break;
                
                if (!predicate(t!))
                    break;

                SkipOne();
            }
        } 
        
        public virtual bool TryConsumeNext(out T? value)
        {
            _position++;
            
            if (!TryGetAtPosition(_position, out value)) 
                return false;
            
            LastConsumed = value;
            return true;
        }
    }
}