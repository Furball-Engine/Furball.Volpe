using System;
using System.Collections.Generic;
using System.Linq;
using Furball.Volpe.Exceptions;

namespace Furball.Volpe.Evaluation; 

public class Class
{
    private readonly Dictionary<string, Function> _methods;
        
    public string Name { get; }
        
    public Class? ExtendedClass { get; }

    public bool TryGetMethod(string name, out Function? funcRef)
    {
        if (_methods.TryGetValue(name, out funcRef))
            return true;

        return ExtendedClass?.TryGetMethod(name, out funcRef) ?? false;
    }

    public bool TryGetConstructor(out Function? funcRef) => TryGetMethod("init", out funcRef);
        
    public Class(string name, (string id, Function fn)[] methods, Class? extendedClass = null)
    {
        Function CreateConstructor(Function initFunction)
        {
            //if (initFunction.ParameterCount == 0)

            return new Function.Builtin((context, parameters) =>
            {
                Value.Object baseObject = new Value.Object(new Dictionary<string, Value>());

                if (parameters.Length != initFunction.ParameterCount - 1)
                    throw new ParamaterCountMismatchException($"{name}->init",
                        initFunction.ParameterCount - 1, parameters.Length, context.Expression.PositionInText);
                
                Value[] values = new Value[parameters.Length + 1];
                values[0] = baseObject;
                Array.Copy(parameters, 0, values, 1, parameters.Length);
                
                initFunction.Invoke(context, values);

                return baseObject;
            }, 0);
        }
        
        _methods      = methods.ToDictionary(p => p.id, p => p.id == "init" ? CreateConstructor(p.fn) : p.fn);
        Name          = name;
        ExtendedClass = extendedClass;
    }

    public Class(string name) : this(name, Array.Empty<(string, Function)>()){}
}