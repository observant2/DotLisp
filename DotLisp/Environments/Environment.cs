using System;
using System.Collections.Generic;
using System.Linq;
using DotLisp.Exceptions;
using DotLisp.Types;

namespace DotLisp.Environments
{
    public class Environment
    {
        /// All the symbols defined in this environment
        public Dictionary<string, Expression> Data { get; set; }

        /// The environment containing this environment
        public Environment Outer { get; set; }

        public Environment(
            IEnumerable<string> parameters,
            IEnumerable<Expression> expressions,
            Environment outer)
        {
            var data = parameters.Zip(expressions)
                .ToDictionary(tuple => tuple.First, tuple => tuple.Second);

            Data = data;
            Outer = outer;
        }

        /// Find the innermost environment where the symbol is defined
        public Environment Find(string symbol)
        {
            if (Data.ContainsKey(symbol))
            {
                return this;
            }

            if (Outer == null)
            {
                throw new SymbolNotFoundException(symbol);
            }

            return Outer.Find(symbol);
        }
    }
}