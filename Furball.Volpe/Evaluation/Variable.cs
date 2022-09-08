using System;

namespace Furball.Volpe.Evaluation; 

public interface IVariable
{
    string Name { get; }
}

public class Variable : IVariable
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

public class HookedVariable : IVariable
{
    public string Name { get; }

    public Function Getter { get; }
    public Function Setter { get; }

    public HookedVariable(string name, Function getter, Function setter)
    {
        Getter = getter;
        Setter = setter;
        Name = name;
    }
}

public class TypedVariable<T> : Variable where T: Value
{
    public new EventHandler<T>? OnChange;

    public T Value {
        get => (T)this.RawValue;
        set {
            this.RawValue = value;
        }
    }

    public new Value RawValue
    {
        get => base.RawValue;
            
        set
        {
            if (value is not T castedValue)
                throw new InvalidOperationException($"This variable only supports {value.GetType()} values.");

            base.RawValue = castedValue;
            this.OnChange?.Invoke(this, castedValue);
        }
    }

    public TypedVariable(string name, T value) : base(name, value) {}
}