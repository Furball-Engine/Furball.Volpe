using System;
using System.Collections.Generic;
using System.Linq;
using Volpe.Memory;

namespace Volpe.Evaluation
{
    public class Scope
    {
        public Scope? Parent { get; }
        
        private readonly Dictionary<string, Value> _variables;
        
        private readonly Dictionary<string, (Function Getter, Function Setter)> _hookedVariables;
        
        private readonly Dictionary<string, Function> _functions;
        
        public Scope(BuiltinFunction[] builtins)
        {
            _variables = new Dictionary<string, Value>();
            _hookedVariables = new Dictionary<string, (Function Getter, Function Setter)>();
            _functions = builtins.ToDictionary(b => b.Identifier, b => (Function)new Function.Builtin(b.Callback, b.ParamCount));
        }

        public Scope() : this(Array.Empty<BuiltinFunction>())
        {
        }
        
        public Scope(Scope? parent) : this()
        {
            Parent = parent;
        }
        
        public bool TryGetFunctionReference(string functionName, out Function? function)
        {
            if (_functions.TryGetValue(functionName, out function))
                return true;
            
            if (Parent is not null)
                return Parent.TryGetFunctionReference(functionName, out function);

            return false;
        }

        public bool TrySetFunction(string functionName, Function.Standard function, bool overwriteIfAlreadyDefined = false)
        {
            if (Parent?.HasFunction(functionName) ?? false)
                return false;

            if (this.HasFunction(functionName))
            {
                if (!overwriteIfAlreadyDefined) 
                    return false;
                
                this._functions[functionName] = function;
                return true;
            }

            _functions.Add(functionName, function);
            
            return true;
        }

        public bool TryGetGetterFromHookedVariable(string variableName, out Function? getter)
        {
            getter = null;
            (Function Getter, Function Setter) pair;

            if (_hookedVariables.TryGetValue(variableName, out pair))
            {
                getter = pair.Getter;
                return true;
            }

            if (Parent is not null)
                return Parent.TryGetGetterFromHookedVariable(variableName, out getter);

            return false;
        }
        
        public bool TryGetSetterFromHookedVariable(string variableName, out Function? setter)
        {
            setter = null;
            (Function Getter, Function Setter) pair;

            if (_hookedVariables.TryGetValue(variableName, out pair))
            {
                setter = pair.Setter;
                return true;
            }
            
            if (Parent is not null)
                return Parent.TryGetSetterFromHookedVariable(variableName, out setter);
            
            return false;
        }

        public bool TryGetVariableValue(string variableName, out Value? value)
        {
            value = null;
            
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
                _variables.Add(variableName, new CellSwap<Value>(value));
        }
        
        public void HookVariableToGetterAndSetter(string variableName, 
            (Value.FunctionReference Getter, Value.FunctionReference Setter) value, bool shadowParentVariables = true)
        {
            if (!shadowParentVariables && Parent is not null && Parent.HasVariable(variableName))
            {
                Parent.HookVariableToGetterAndSetter(variableName, value);
                return;
            }

            if (_variables.ContainsKey(variableName))
                _hookedVariables[variableName] = (value.Getter.Function, value.Setter.Function);
            else
                _hookedVariables.Add(variableName, (value.Getter.Function, value.Setter.Function));
        }
    }
}