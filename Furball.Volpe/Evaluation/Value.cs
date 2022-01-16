using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Furball.Volpe.Memory;

namespace Furball.Volpe.Evaluation
{
    public abstract record Value
    {
        public virtual Value InnerOrSelf => this;

        public record ValueReference(CellSwap<Value> Value) : Value
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
            public override string Representation => Value.ToString(CultureInfo.InvariantCulture);
        }

        public record Boolean(bool Value) : Value
        {
            public override string Representation => Value ? "true" : "false";
        }
        
        public record String(string Value) : Value
        {
            public override string Representation => '"' + Value + '"';
        }
        
        public record Void : Value
        {
            public override string Representation => "void";
        }

        public record Array(List<CellSwap<Value>> Value) : Value
        {
            public override string Representation
            {
                get
                {
                    StringBuilder stringBuilder = new StringBuilder();

                    stringBuilder.Append('[');

                    for (int i = 0; i < Value.Count; i++)
                    {
                        stringBuilder.Append(Value[i].Value.Representation);
                        
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
            public override string Representation
            {
                get
                {
                    StringBuilder stringBuilder = new StringBuilder();

                    stringBuilder.Append('{');

                    int i = 0;
                    foreach (var key in Value.Keys)
                    {
                        stringBuilder.Append($"\"{key}\" = {Value[key].Value.Representation}");
                        
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

        public static readonly Void DefaultVoid = new Void();
        public static readonly Boolean DefaultTrue = new Boolean(true);
        public static readonly Boolean DefaultFalse = new Boolean(false);
        
        public abstract string Representation { get; }
    }
}