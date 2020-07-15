using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DotLisp.Exceptions;
using DotLisp.Types;
using String = DotLisp.Types.String;

namespace DotLisp.Parsing
{
    public static class Parser
    {
        public static string ToLisp(Expression exp)
        {
            if (exp is List l)
            {
                return "(" + string.Join(" ",
                               l.Expressions.Select(ToLisp))
                           + ")";
            }

            return exp.ToString();
        }

        public static List<string> Tokenize(string input)
        {
            return input
                .Replace("(", " ( ")
                .Replace(")", " ) ")
                .Split(" ")
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();
        }

        public static Expression Parse(string program)
        {
            return ReadFromTokens(Tokenize(program));
        }

        public static Expression ReadFromTokens(List<string> tokens)
        {
            if (tokens.Count == 0)
            {
                throw new ParserException("Unexpected EOF!");
            }

            var token = tokens[0];
            tokens.RemoveAt(0);

            switch (token)
            {
                case "(":
                    {
                        if (tokens.Count == 0)
                        {
                            throw new ParserException("Missing ')'!");
                        }

                        var l = new LinkedList<Expression>();
                        while (tokens[0] != ")")
                        {
                            l.AddLast(ReadFromTokens(tokens));
                        }

                        tokens.RemoveAt(0);
                        return new List()
                        {
                            Expressions = l
                        };
                    }
                case ")":
                    throw new ParserException("Unexpected ')'!");
                default:
                    return ParseAtom(token);
            }
        }

        public static Atom ParseAtom(string token)
        {
            if (token[0] == '"')
            {
                return new String
                {
                    Value =
                        token.Substring(1, token.Length - 2)
                };
            }

            if (token == "true" || token == "false")
            {
                return new Bool()
                {
                    Value = bool.Parse(token)
                };
            }

            if (int.TryParse(token, out var integer))
            {
                return new Number()
                {
                    Int = integer
                };
            }

            if (float.TryParse(token,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out var floating))
            {
                return new Number()
                {
                    Float = floating
                };
            }

            return new Symbol(token);
        }
    }

    public class InPort
    {
        private Regex _tokenizer = new Regex(
            @"\s*(,@|[('`,)]|""(?:[\\].|[^\\""])*""|;.*|[^\s('""`,;)]*)(.*)"
        );

        private Dictionary<string, string> _quotes = new Dictionary<string, string>()
        {
            ["'"] = "quote",
            ["`"] = "quasiquote",
            [","] = "unquote",
            [",@"] = "unquotesplicing"
        };

        private StreamReader _inputStream;

        private string _line = "";

        public InPort(StreamReader inputStream)
        {
            _inputStream = inputStream;
        }

        public InPort(string input) : this(
            new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(input))))
        {
        }

        public string NextToken()
        {
            while (true)
            {
                if (string.IsNullOrEmpty(_line))
                {
                    _line = _inputStream.ReadLine();
                }

                if (string.IsNullOrEmpty(_line) && _inputStream.EndOfStream)
                {
                    return null;
                }

                var match = _tokenizer.Match(_line);
                var token = match.Groups[1].Value.Trim();
                _line = _line.ReplaceFirst(token, "").Trim();
                return token;
            }
        }

        public static string ReadChar(InPort inPort)
        {
            if (inPort._line != "")
            {
                var ch = "" + inPort._line[0];
                inPort._line = inPort._line.Substring(1);
                return ch;
            }
            else
            {
                return "" + Convert.ToChar(inPort._inputStream.Read());
            }
        }

        private Expression ReadAhead(string token)
        {
            switch (token)
            {
                case "(":
                    var l = new List()
                    {
                        Expressions = new LinkedList<Expression>()
                    };
                    while (true)
                    {
                        token = NextToken();
                        if (token == ")")
                        {
                            return l;
                        }

                        l.Expressions.AddLast(ReadAhead(token));
                    }

                case ")":
                    throw new ParserException("Unexpected ')'!");
            }

            if (_quotes.ContainsKey(token))
            {
                // convert to real expression
                var keyword = _quotes[token];
                var exps = new LinkedList<Expression>();

                exps.AddLast(new Symbol(keyword));
                exps.AddLast(Read());

                return new List
                {
                    Expressions = exps
                };
            }

            return Parser.ParseAtom(token);
        }

        public Expression Read()
        {
            var token1 = NextToken();
            return token1 == null ? new Symbol("eof") : ReadAhead(token1);
        }

        public Expression Read(string input)
        {
            _inputStream =
                new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(input)));
            return Read();
        }
    }
}