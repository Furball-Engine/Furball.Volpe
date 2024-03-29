using System;
using System.Diagnostics;
using System.Linq;
using Furball.Volpe.Evaluation;
using Furball.Volpe.Exceptions;

namespace Furball.Volpe.Repl; 

public static class Builtins {
	public static BuiltinFunction[] Funcs;
		
	static Builtins() {
		Funcs = new BuiltinFunction[]
		{
			new BuiltinFunction("clear", 0, (_, _) =>
			{
				Console.Clear();
				return Value.DefaultVoid;
			}),
                
			new BuiltinFunction("println", 1, (context, values) =>
			{
				if (values[0] is not Value.String(var str))
					throw new InvalidValueTypeException(
						typeof(Value.String), values[0].GetType(), context.Expression.PositionInText);
                    
				Program.OutputStream.WriteLine(str);
                    
				return Value.DefaultVoid;
			}),
                
			new("timeit", 2, (context, values) =>
			{
				if (values[0] is not Value.FunctionReference(_, var function))
					throw new InvalidValueTypeException(
						typeof(Value.FunctionReference), values[0].GetType(), context.Expression.PositionInText);
                    
				if (values[1] is not Value.Number(var loops))
					throw new InvalidValueTypeException(
						typeof(Value.FunctionReference), values[0].GetType(), context.Expression.PositionInText);

				long   lLoops            = (long) loops;
				double totalMilliseconds = 0;

				for (long i = 0; i < lLoops; i++)
				{
					Stopwatch stopwatch = new Stopwatch();

					stopwatch.Start();
					function.Invoke(context, Array.Empty<Value>());
					stopwatch.Stop();

					totalMilliseconds += stopwatch.Elapsed.TotalMilliseconds;
				}
                    
				return new Value.Number(totalMilliseconds / loops);
			})
		}.ToArray();
	}
}