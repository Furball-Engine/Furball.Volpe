using System;
using System.Collections.Generic;
using System.Linq;

namespace Furball.Volpe.Evaluation; 

public class Environment
{
    public Environment? Parent { get; }
        
    private readonly Dictionary<string, IVariable>                           _variables;
    private readonly Dictionary<string, Function>                           _functions;
    private readonly Dictionary<string, Class>                              _classes;

    public IReadOnlyDictionary<string, IVariable> Variables => this._variables;

    public Environment(BuiltinFunction[] builtins)
    {
        _variables       = new Dictionary<string, IVariable>();
        _functions       = builtins.ToDictionary(b => b.Identifier, b => (Function)new Function.Builtin(b.Callback, b.ParamCount));
        _classes         = new Dictionary<string, Class>();
    }

    public void AddBuiltin(BuiltinFunction function) =>
        _functions.Add(function.Identifier, new Function.Builtin(function.Callback, function.ParamCount));

    public void RemoveBuiltin(BuiltinFunction function) =>
        _functions.Remove(function.Identifier);
        
    public Environment(Environment? parent = null) : this(Array.Empty<BuiltinFunction>())
    {
        Parent = parent;
    }

    public bool TrySetClass(string className, Class classRef)
    {
        if (_classes.ContainsKey(className))
            return false;
            
        if (Parent is not null)
            return Parent._classes.ContainsKey(className);
            
        _classes.Add(className, classRef);

        return true;
    }
        
    public bool TryGetClass(string className, out Class? classRef)
    {
        if (_classes.TryGetValue(className, out classRef))
            return true;
            
        if (Parent is not null)
            return Parent.TryGetClass(className, out classRef);

        return false;
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

    public bool TryGetVariable(string variableName, out IVariable? value)
    {
        value = null;

        if (_variables.TryGetValue(variableName, out value))
            return true;

        if (Parent is not null)
            return Parent.TryGetVariable(variableName, out value);

        return false;
    }

    public bool HasVariable(string variableName) => _variables.ContainsKey(variableName);
    public bool HasFunction(string functionName) => _functions.ContainsKey(functionName);

    public void SetVariable(IVariable variable, bool shadowParentVariables = true)
    {
        if (!shadowParentVariables && Parent is not null && Parent.HasVariable(variable.Name))
        {
            Parent.SetVariable(variable, false);
            return;
        }

        if (_variables.ContainsKey(variable.Name))
            _variables[variable.Name] = variable;
        else
            _variables.Add(variable.Name, variable);
    }
}