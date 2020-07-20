using System;

namespace DotLisp.Exceptions
{
    public class ParserException : Exception
    {
        public ParserException(string message, int line, int column)
            : base($"Parser Exception:\n{line}:{column}: {message}")
        {
        }
    }
}