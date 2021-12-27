using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Furball.Volpe.Exceptions;
using Furball.Volpe.Memory;

namespace Furball.Volpe.Evaluation
{
    public delegate Value FunctionInvokeCallback(EvaluatorContext context, Value[] parameters);
        
    public class BuiltinFunction
    {
        public string Identifier { get; }
        public int ParamCount { get; }
        public FunctionInvokeCallback Callback { get; }

        public BuiltinFunction(string identifier, int paramCount, FunctionInvokeCallback cb)
        {
            Identifier = identifier;
            ParamCount = paramCount;
            Callback = cb;
        }

        public override string ToString() => $"{Identifier}";
    }
    
    public static class DefaultBuiltins
    {
        /// <summary>
        /// Gets the Entire CoreLib while allowing to exclude certain CoreLib Extensions
        /// </summary>
        /// <param name="except"></param>
        /// <returns></returns>
        public static BuiltinFunction[] GetAll(string[] except = null!) {
            List<BuiltinFunction> functions = new();

            List<Type> types = Assembly.GetAssembly(typeof(BuiltinFunction))!
                .GetTypes()
                .Where(
                    type => type.IsSubclassOf(typeof(CoreLibExtension))
                ).ToList();

            foreach (Type type in types) {
                if (except != null) {
                    if (except.Contains(type.Name))
                        continue;
                }

                CoreLibExtension extension = (CoreLibExtension) Activator.CreateInstance(type)!;

                functions.AddRange(extension.FunctionExports());
            }

            return functions.ToArray();
        }
    }
}