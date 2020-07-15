using System.Collections.Generic;
using System.Linq;
using DotLisp.Environments.Core;
using DotLisp.Exceptions;
using DotLisp.Parsing;
using DotLisp.Types;

namespace DotLisp.Environments
{
    public class GlobalEnvironment : Environment
    {
        public GlobalEnvironment() :
            base(InitData.Keys, InitData.Values, null)
        {
        }

        private static readonly Dictionary<string, Expression> InitData =
            new Dictionary<string, Expression>()
            {
                ["do"] = Func.From(
                    (expr) =>
                    {
                        if (expr is List l)
                        {
                            return new List()
                            {
                                Expressions = l.Expressions.Skip(1)
                                    .ToLinkedList()
                            };
                        }

                        throw new EvaluatorException(
                            "'do' needs at least one parameter");
                    }),

                ["+"] = Math.Apply((acc, x) => acc + x),
                ["-"] = Math.Apply((acc, x) => acc - x),
                ["*"] = Math.Apply((acc, x) => acc * x),
                ["/"] = Math.Apply((acc, x) => acc / x),

                [">"] = Math.Compare((a, b) => a > b),
                [">="] = Math.Compare((a, b) => a >= b),
                ["<"] = Math.Compare((a, b) => a < b),
                ["<="] = Math.Compare((a, b) => a <= b),
                ["=="] = Math.Equals(),

                ["PI"] = new Number() { Float = (float)System.Math.PI },
                ["E"] = new Number() { Float = (float)System.Math.E },

                ["first"] = Lists.First(),
                ["rest"] = Lists.Rest(),
                ["cons"] = Lists.Cons(),
            };

    }
}