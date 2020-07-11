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
}