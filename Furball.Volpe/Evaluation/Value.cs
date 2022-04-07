using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Furball.Volpe.Evaluation.CoreLib;
using Furball.Volpe.Memory;

namespace Furball.Volpe.Evaluation
{
    public abstract record Value
    {
        public abstract Value Copy();
        public Class? Class { 
            get 
            {
                return _class ?? BaseClass;
            } 

            set 
            { 
                _class = value;
            } 
        }

        public Class? _class;

        public virtual Class? BaseClass => null;
        
        public virtual Value InnerOrSelf => this;

        public record ValueReference(CellSwap<Value> Value) : Value
        {
            public override string Representation => Value.Value.Representation;

            public override Value InnerOrSelf => Value.Value;

            public override Value Copy()
            {
                throw new NotImplementedException();
            }
        }
        
        public record ToReturn(Value Value) : Value
        {
            public override string Representation => throw new InvalidOperationException();

            public override Value Copy() =>
                throw new NotImplementedException();
        }

        public record Number(double Value) : Value
        {
            public override Class BaseClass => BaseClasses.NumberClass.Default;

            public override string Representation => Value.ToString(CultureInfo.InvariantCulture);

            public override Value Copy() => this;
        }

        public record Boolean(bool Value) : Value
        {
            public override Class BaseClass => BaseClasses.BooleanClass.Default;
            public override string Representation => Value ? "true" : "false";
            public override Value Copy() => this;
        }
        
        public record String(string Value) : Value
        {
            public override Class BaseClass => BaseClasses.StringClass.Default;
            
            public override string Representation => '"' + Value + '"';
            public override Value Copy() => this;
        }
        
        public record Void : Value
        {
            public override string Representation => "void";
            public override Value Copy() => this;
        }

        public record Zero : Value
        {
            public override string Representation => "zero";
            public override Value Copy() => this;
        }

        public record Array(List<CellSwap<Value>> Value) : Value
        {
            public override Class BaseClass => BaseClasses.ArrayClass.Default;

            public override Value Copy()
            {
                List<CellSwap<Value>> newArray = new List<CellSwap<Value>>(Value.Count);

                for (int i = 0; i < Value.Count; i++)
                    newArray.Add(new CellSwap<Value>(Value[i].Value));

                return this with { Value = newArray };
            }
            
            public override string Representation
            {
                get
                {
                    StringBuilder stringBuilder = new StringBuilder();

                    stringBuilder.Append('[');

                    for (int i = 0; i < Value.Count; i++)
                    {
                        stringBuilder.Append(Value[i].Value.RepresentationWithClass);
                        
                        if (i != Value.Count - 1)
                            stringBuilder.Append(',');
                    }
                    
                    stringBuilder.Append(']');

                    return stringBuilder.ToString();
                }
            }
        }
        
        public record Object(Dictionary<string, CellSwap<Value>> Value) : Value
        {
            public override Class BaseClass => BaseClasses.ObjectClass.Default;
            
            public override Value Copy()
            {
                Dictionary<string, CellSwap<Value>> newDict = new Dictionary<string, CellSwap<Value>>(Value);
                newDict.EnsureCapacity(Value.Count);

                foreach (var pair in Value)
                    newDict.Add(pair.Key, new CellSwap<Value>(pair.Value.Value));
                
                return this with { Value = newDict };
            }

            public override string Representation
            {
                get
                {
                    StringBuilder stringBuilder = new StringBuilder();

                    stringBuilder.Append('{');

                    int i = 0;
                    foreach (var key in Value.Keys)
                    {
                        stringBuilder.Append($"\"{key}\" = {Value[key].Value.RepresentationWithClass}");
                        
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

            public override Value Copy() => this;
        }

        public static readonly Void DefaultVoid = new Void();
        public static readonly Zero DefaultZero = new Zero();
        public static readonly Boolean DefaultTrue = new Boolean(true);
        public static readonly Boolean DefaultFalse = new Boolean(false);
        
        public abstract string Representation { get; }

        public string RepresentationWithClass => Class == null ? Representation : Representation + $" <class \"{Class.Name}\">";
    }
}