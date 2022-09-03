using System;
using System.Collections.Generic;

namespace Furball.Volpe; 

public class Consumer<pT> 
{
    public Consumer(IEnumerable<pT> enumerable)
    {
        Enumerator = enumerable.GetEnumerator();
    }
        
    protected IEnumerator<pT> Enumerator { get; }
        
    public int ConsumedCount { get; private set; }

    public pT? LastConsumed { get; private set; }

    private object? _onHold;

    public virtual bool TryPeekNext(out pT? value)
    {
        value = default;

        if (_onHold != null)
        {
            value = (pT) _onHold;
            return true;
        }

        if (!Enumerator.MoveNext())
            return false;

        _onHold = value = Enumerator.Current;
        return true;
    }

    public pTResult TryConsumeNextAndThen<pTResult>(Func<bool, pT?, pTResult> function)
    {
        bool consumed = TryConsumeNext(out pT? value);
        return function(consumed, value);
    }
        
    public bool SkipOne() => TryConsumeNext(out _);

    public void SkipTill(Func<pT, bool> predicate)
    {
        for (;;)
        {
            pT? t;
            if (!TryPeekNext(out t))
                break;
                
            if (!predicate(t!))
                break;

            SkipOne();
        }
    } 
        
    public void SkipTill<pTState>(Func<pTState, pT, (bool, pTState)> predicate, pTState initialState)
    {
        for (;;)
        {
            pT? t;
            if (!TryPeekNext(out t))
                break;

            (bool result, pTState newState) = predicate(initialState, t!);
                
            if (!result)
                break;

            initialState = newState;

            SkipOne();
        }
    } 
        
    public virtual bool TryConsumeNext(out pT? value)
    {
        value = default;
        ConsumedCount++;
            
        if (_onHold != null)
        {
            value   = (pT) _onHold;
            _onHold = null;
            return true;
        }

        if (!Enumerator.MoveNext())
            return false;
            
        value = Enumerator.Current;
            
        LastConsumed = value;
        return true;
    }
}