using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DotLisp.Environments.Core;
using DotLisp.Exceptions;
using DotLisp.Parsing;
using DotLisp.Types;
using Math = DotLisp.Environments.Core.Math;

namespace DotLisp.Environments
{
    public class GlobalEnvironment : Environment
    {
        public GlobalEnvironment() :
            base(InitData.Keys, InitData.Values, null)
        {
        }

        public static readonly Dictionary<string, DotProcedure> MacroTable =
            new Dictionary<string, DotProcedure>();

        private static readonly Dictionary<string, DotExpression> InitData =
            new Dictionary<string, DotExpression>()
            {
                ["+"] = Math.Apply((acc, x) => acc + x),
                ["-"] = Math.Apply((acc, x) => acc - x),
                ["*"] = Math.Apply((acc, x) => acc * x),
                ["/"] = Math.Apply((acc, x) => acc / x),

                [">"] = Math.Compare((a, b) => a > b),
                [">="] = Math.Compare((a, b) => a >= b),
                ["<"] = Math.Compare((a, b) => a < b),
                ["<="] = Math.Compare((a, b) => a <= b),
                // TODO: implement general equals
                ["=="] = Math.Equals(),

                ["PI"] = new DotNumber() {Float = (float) System.Math.PI},
                ["E"] = new DotNumber() {Float = (float) System.Math.E},
                ["nil"] = new DotSymbol("nil"),

                ["first"] = Lists.First(),
                ["rest"] = Lists.Rest(),
                ["cons"] = Lists.Cons(),
                ["load"] = DotFunc.From(expr =>
                {
                    var argumentException = new EvaluatorException(
                        $"'load' expects a path as string.");

                    if (!(expr is DotList args) || args.Expressions.Count == 0)
                    {
                        throw argumentException;
                    }


                    if (!(args.Expressions.First() is DotString path))
                    {
                        throw argumentException;
                    }

                    try
                    {
                        using var file = File.Open(path.Value, FileMode.Open);
                        Repl(null, new InPort(new StreamReader(file)),
                            false);
                        return DotBool.True();
                    }
                    catch (Exception e)
                    {
                        throw new EvaluatorException("Could not open file:\n" +
                                                     e.Message);
                    }
                })
            };

        public static void Repl(string prompt = "> ",
            InPort inPort = null, bool printToConsole = true)

        {
            inPort ??=
                new InPort(new StreamReader(Console.OpenStandardInput(),
                    Encoding.UTF8));

            while (true)
            {
                try
                {
                    if (printToConsole && !string.IsNullOrEmpty(prompt))
                    {
                        Console.Write(prompt);
                    }

                    var x = Expander.Expand(inPort.Read(), true);

                    if (x == null)
                    {
                        return;
                    }

                    var ret = Evaluator.Eval(x);

                    if (printToConsole)
                    {
                        Console.WriteLine(ret.ToString());
                    }
                }
                catch (ParserException pe)
                {
                    Console.WriteLine(pe.Message);
                }
                catch (EvaluatorException ee)
                {
                    Console.WriteLine(ee.Message);
                }
            }
        }
    }
}