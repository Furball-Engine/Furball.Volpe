using System;

namespace Furball.Volpe.Evaluation; 

public class Variable
{
    public EventHandler<Value>? OnChange;

    private Value _value;

    public string Name { get; }

    public virtual Value RawValue
    {
        get => _value;
            
        set
        {
            if (_value.Equals(value))
                return;

            _value = value;
                
            OnChange?.Invoke(this, _value);
        }
    }

    public Variable(string name, Value value)
    {
        Name   = name;
        _value = value;
    }
}
    
public class TypedVariable<pT> : Variable where pT: Value
{
    public new EventHandler<pT>? OnChange;

    public pT Value {
        get => (pT)base.RawValue;
        set {
            base.RawValue = value;
            this.OnChange?.Invoke(this, value);
        }
    }

    public override Value RawValue
    {
        get => base.RawValue;
            
        set
        {
            if (value is not pT castedValue)
                throw new InvalidOperationException($"This variable only supports ${value.GetType()} values.");

            base.RawValue = castedValue;
            this.OnChange?.Invoke(this, castedValue);
        }
    }

    public TypedVariable(string name, pT value) : base(name, value) {}
}