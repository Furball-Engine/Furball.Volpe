using System.Collections.Generic;

namespace Volpe.Evaluation
{
    public class Scope
    {
        public Scope? Parent { get; }
        
        private readonly Dictionary<string, Value> _variables;
        private readonly Dictionary<string, Value.Function> _functions;
        
        public Scope()
        {
            _variables = new Dictionary<string, Value>();
            _functions = new Dictionary<string, Value.Function>();
        }

        public Scope(Scope? parent) : this()
        {
            Parent = parent;
        }

        public bool TryGetFunctionReference(string functionName, out Value.Function? function)
        {
            if (_functions.TryGetValue(functionName, out function))
                return true;
            
            if (Parent is not null)
                return Parent.TryGetFunctionReference(functionName, out function);

            return false;
        }
        
        public bool TrySetFunction(string functionName, Value.Function function)
        {
            if (Parent?.HasFunction(functionName) ?? false)
                return false;

            if (this.HasFunction(functionName))
                return false;
            
            _functions.Add(functionName, function);
            
            return true;
        }
        
        public bool TryGetVariableValue(string variableName, out Value? value)
        {
            if (_variables.TryGetValue(variableName, out value))
                return true;
            
            if (Parent is not null)
                return Parent.TryGetVariableValue(variableName, out value);

            return false;
        }

        public bool HasVariable(string variableName) => _variables.ContainsKey(variableName);
        public bool HasFunction(string functionName) => _functions.ContainsKey(functionName);
        
        public void SetVariableValue(string variableName, Value value, bool shadowParentVariables = true)
        {
            if (!shadowParentVariables && Parent is not null && Parent.HasVariable(variableName))
            {
                Parent.SetVariableValue(variableName, value);
                return;
            }

            if (_variables.ContainsKey(variableName))
                _variables[variableName] = value;
            else
                _variables.Add(variableName, value);
        }
        
    }
}