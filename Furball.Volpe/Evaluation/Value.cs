using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Furball.Volpe.Evaluation.CoreLib;
using Furball.Volpe.Exceptions;

namespace Furball.Volpe.Evaluation;

public interface IValueRef
{
    Value Value { get; set; }
}

public class ArrayValueRef : IValueRef
{
    private List<Value> _array;
    private int _index;

    public ArrayValueRef(List<Value> array, int index)
    {
        _array = array;
        _index = index;
    }

    public Value Value
    {
        get => _array[_index];
        set => _array[_index] = value;
    }
}

public class ObjectValueRef : IValueRef
{
    private Dictionary<string, Value> _dictionary;
    private string _key;

    private PositionInText _positionInText;
    
    public ObjectValueRef(Dictionary<string, Value> dictionary, string key, PositionInText positionInText)
    {
        _dictionary = dictionary;
        _key = key;
        _positionInText = positionInText;
    }

    public Value Value
    {
        get
        {
            Value? value;
            if (!_dictionary.TryGetValue(_key, out value))
                throw new Exceptions.KeyNotFoundException(_key, _positionInText);
            
            return value;
        }
        set => _dictionary[_key] = value;
    }
}

public abstract record Value
{
    public Class? Class { get; set; }
    public virtual Class? BaseClass => null;
        
    public virtual Value InnerOrSelf => this;

    public record ValueReference(IValueRef Value) : Value
    {
        public override string Representation => Value.Value.Representation;

        public override Value InnerOrSelf => Value.Value;
    }
        
    public record ToReturn(Value Value) : Value
    {
        public override string Representation => throw new InvalidOperationException();
    }

    public record Number(double Value) : Value
    {
        public override Class BaseClass => BaseClasses.NumberClass.Default;
        
        public override string Representation => Value.ToString(CultureInfo.InvariantCulture);
    }

    public record Byte(byte Value) : Value 
    {
        public override Class BaseClass => BaseClasses.ByteClass.Default;

        public override string Representation => Value.ToString(CultureInfo.InvariantCulture);
    }

    public record Boolean(bool Value) : Value
    {
        public override Class BaseClass => BaseClasses.BooleanClass.Default;

        public override string Representation => Value ? "true" : "false";
    }
        
    public record String(string Value) : Value
    {
        public override Class BaseClass => BaseClasses.StringClass.Default;
            
        public override string Representation => '"' + Value + '"';
    }
        
    public record Void : Value
    {
        public override string Representation => "void";
    }
    
    

    public record Array(List<Value> Value) : Value
    {
        public override Class BaseClass => BaseClasses.ArrayClass.Default;

        public Array Copy() =>
            this with { Value = new List<Value>(Value) };
            
        public override string Representation
        {
            get
            {
                StringBuilder stringBuilder = new();

                stringBuilder.Append('[');

                for (int i = 0; i < Value.Count; i++)
                {
                    stringBuilder.Append(Value[i].RepresentationWithClass);

                    if (i != Value.Count - 1)
                        stringBuilder.Append(',');
                }
                    
                stringBuilder.Append(']');

                return stringBuilder.ToString();
            }
        }
    }
        
    public record Object(Dictionary<string, Value> Value) : Value
    {
        public override Class BaseClass => BaseClasses.ObjectClass.Default;
            
        public Object Copy() =>
            this with { Value = new Dictionary<string, Value>(Value) };

        public override string Representation
        {
            get
            {
                StringBuilder stringBuilder = new();

                stringBuilder.Append('{');

                int i = 0;
                foreach (var key in Value.Keys)
                {
                    stringBuilder.Append($"\"{key}\" = {Value[key].RepresentationWithClass}");
                        
                    if (i != Value.Count - 1)
                        stringBuilder.Append(',');
                        
                    ++i;
                }
                    
                stringBuilder.Append('}');

                return stringBuilder.ToString();
            }
        }
    }
        
    public record FunctionReference(string Name, Function Function) : Value
    {
        public override string Representation => $"<Function \"{Name}\", {Function.GetHashCode()}>";
    }

    public static readonly Void    DefaultVoid  = new Void();
    
    public static readonly Boolean DefaultTrue  = new Boolean(true);
    public static readonly Boolean DefaultFalse = new Boolean(false);
        
    public abstract string Representation { get; }

    public string RepresentationWithClass => Class == null ? Representation : Representation + $" <class \"{Class.Name}\">";
    public Number  ToNumber()      => (Number) this;
    public Boolean ToBoolean()     => (Boolean) this;
    public String  ToStringValue() => (String) this;
}