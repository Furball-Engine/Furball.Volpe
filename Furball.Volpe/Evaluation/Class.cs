using System;
using System.Collections.Generic;
using System.Linq;
using Furball.Volpe.LexicalAnalysis;

namespace Furball.Volpe.Evaluation
{
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
            _methods = methods.ToDictionary(p => p.id, p => p.fn);
            Name = name;
            ExtendedClass = extendedClass;
        }

        public bool ExtendsClassWithName(string className)
        {
            if (ExtendedClass!.Name == className) 
                return true;

            return ExtendedClass.ExtendsClassWithName(className);
        }

        public void CallMethod(Value instance, string name)
        {

        }

        public Class(string name) : this(name, Array.Empty<(string, Function)>()){}
    }
}