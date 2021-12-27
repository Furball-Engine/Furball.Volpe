using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Furball.Volpe.Exceptions;
using Furball.Volpe.Memory;

namespace Furball.Volpe.Evaluation.CoreLib {
    public class Files : CoreLibExtension {

        public override BuiltinFunction[] FunctionExports() => new BuiltinFunction[] {
            new BuiltinFunction("file_read_all_lines", 1, (context, values) => {
                if (values[0] is not Value.String(var filename))
                    throw new InvalidValueTypeException(typeof(Value.String), values[0].GetType(), context.Expression.PositionInText);

                string[] lines = File.ReadAllLines(filename);

                List<CellSwap<Value>> valueLines = new();

                foreach (string s in lines) {
                    valueLines.Add(new CellSwap<Value>(new Value.String(s)));
                }

                return new Value.Array(valueLines);
            }),
            new BuiltinFunction("file_write_all_lines", 1, (context, values) => {
                if (values[0] is not Value.String(var filename))
                    throw new InvalidValueTypeException(typeof(Value.String), values[0].GetType(), context.Expression.PositionInText);
                if (values[1] is not Value.Array(var data))
                    throw new InvalidValueTypeException(typeof(Value.Array), values[1].GetType(), context.Expression.PositionInText);

                List<string> lines = new();

                foreach (CellSwap<Value> value in data) {
                    switch (value.Value) {
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
}
