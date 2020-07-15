using System.Collections.Generic;
using DotLisp.Environments.Core;
using DotLisp.Types;

namespace DotLisp.Environments
{
    public class GlobalEnvironment : Environment
    {
        public GlobalEnvironment() :
            base(InitData.Keys, InitData.Values, null)
        {
        }

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
                ["=="] = Math.Equals(),

                ["PI"] = new DotNumber() { Float = (float)System.Math.PI },
                ["E"] = new DotNumber() { Float = (float)System.Math.E },

                ["first"] = Lists.First(),
                ["rest"] = Lists.Rest(),
                ["cons"] = Lists.Cons(),
            };

    }
}