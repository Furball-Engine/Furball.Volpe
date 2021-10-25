using System.Collections.Generic;

namespace Volpe.Evaluation
{
    public class Scope
    {
        private Scope? Parent { get; }
        
        public Scope()
        {
            _variables = new Dictionary<string, Value>();
        }

        public Scope(Scope parent) : this()
        {
            Parent = parent;
        }

        public bool TryGetVariableValue(string variableName, out Value? value)
        {
            if (_variables.TryGetValue(variableName, out value))
                return true;
            
            if (Parent is not null)
                return Parent.TryGetVariableValue(variableName, out value);

            return false;
        }

        public bool HasVariable(string variableName) => TryGetVariableValue(variableName, out _);
        
        public void SetVariableValue(string variableName, Value value)
        {
            if (Parent is not null && Parent.HasVariable(variableName))
            {
                Parent.SetVariableValue(variableName, value);
                return;
            }

            if (_variables.ContainsKey(variableName))
                _variables[variableName] = value;
            else
                _variables.Add(variableName, value);
        }
        
        private readonly Dictionary<string, Value> _variables;
    }
}