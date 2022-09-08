using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Furball.Volpe.Exceptions;

namespace Furball.Volpe.Evaluation.CoreLib; 

public class Files : CoreLibExtension {

    public override BuiltinFunction[] FunctionExports() => new BuiltinFunction[] {
        new("file_read_all_lines", 1, (context, values) => {
            if (values[0] is not Value.String(var filename))
                throw new InvalidValueTypeException(typeof(Value.String), values[0].GetType(), context.Expression.PositionInText);

            string[] lines = File.ReadAllLines(filename);

            List<Value> valueLines = new();

            foreach (string s in lines) {
                valueLines.Add(new Value.String(s));
            }

            return new Value.Array(valueLines);
        }),
        new("file_write_all_lines", 1, (context, values) => {
            if (values[0] is not Value.String(var filename))
                throw new InvalidValueTypeException(typeof(Value.String), values[0].GetType(), context.Expression.PositionInText);
            if (values[1] is not Value.Array(var data))
                throw new InvalidValueTypeException(typeof(Value.Array), values[1].GetType(), context.Expression.PositionInText);

            List<string> lines = new();

            foreach (Value value in data) {
                switch (value) {
                    case Value.Boolean boolean: {
                        lines.Add(boolean.Value.ToString());
                        break;
                    }
                    case Value.Number number: {
                        lines.Add(number.Value.ToString(CultureInfo.InvariantCulture));
                        break;
                    }
                    case Value.String str: {
                        lines.Add(str.Value);
                        break;
                    }
                }
            }

            File.WriteAllLines(filename, lines);

            return Value.DefaultVoid;
        }),
    };
}