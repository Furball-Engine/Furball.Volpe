using System;

namespace Furball.Volpe.Evaluation; 

public interface IVariable
{
    string Name { get; }
}

public static class VariableExtensions 
{
    public static Variable ToVariable(this IVariable value)
    {
        if (value is not Variable concreteVariable)
            throw new InvalidOperationException("The variable is not a concrete variable.");

        return concreteVariable;
    }
    
    public static TypedVariable<T> ToTypedVariable<T>(this IVariable value) where T : Value
    {
        if (value is not TypedVariable<T> typedVariable)
            throw new InvalidOperationException($"The variable is not a typed variable with {typeof(T)}.");

        return typedVariable;
    }
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
    public new EventHandler<pT>? OnChange;

    public T Value {
        get => (T)this.RawValue;
        set {
            this.RawValue = value;
        }
    }

    public override Value RawValue
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

    public TypedVariable(string name, pT value) : base(name, value) {}
}