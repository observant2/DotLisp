using System;

namespace DotLisp.Exceptions
{
    public class ParserException : Exception
    {
        public ParserException(string message)
            : base($"Parser Exception: \n{message}")
        {
        }
    }
}