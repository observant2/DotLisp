using System;

namespace DotLisp.Exceptions
{
    public class EvaluatorException : Exception
    {
        public EvaluatorException(string message)
            : base($"Evaluator Exception: \n{message}")
        {
        }
    }

    public class SymbolNotFoundException : EvaluatorException
    {
        public SymbolNotFoundException(string symbol)
            : base($"Symbol {symbol} not found!")
        {
        }
    }
}