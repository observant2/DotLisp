using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using DotLisp.Exceptions;
using DotLisp.Types;

namespace DotLisp.Parsing
{
    /// It's basically a reader.
    /// It also converts special characters to lispy function calls,
    /// for example: '(1 2 3) -> (quote (1 2 3)), so that the
    /// expander and the evaluator don't have to deal with that.
    public class InPort
    {
        private readonly Regex _tokenizer = new Regex(
            @"\s*(,@|[('`,)]|""(?:[\\].|[^\\""])*""|;.*|[^\s('""`,;)]*)(.*)"
        );

        private Dictionary<string, string> _quotes = new Dictionary<string, string>()
        {
            // TODO: these words have to be illegal symbol names...?!
            ["'"] = "quote",
            ["`"] = "quasiquote",
            [","] = "unquote",
            [",@"] = "unquotesplicing"
        };

        private StreamReader _inputStream;

        private string _line = "";

        public InPort()
        {
        }

        public InPort(StreamReader inputStream)
        {
            _inputStream = inputStream;
        }

        public InPort(Stream stream) : this(new StreamReader(stream))
        {
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

                if (token != "" && !token.StartsWith(";"))
                {
                    return token;
                }
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

        private DotExpression ReadAhead(string token)
        {
            switch (token)
            {
                case "(":
                    var l = new DotList()
                    {
                        Expressions = new LinkedList<DotExpression>()
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
                var exps = new LinkedList<DotExpression>();

                exps.AddLast(new DotSymbol(keyword));
                exps.AddLast(Read());

                return new DotList
                {
                    Expressions = exps
                };
            }

            return ParseAtom(token);
        }

        public DotExpression Read()
        {
            var token1 = NextToken();
            return token1 == null ? null : ReadAhead(token1);
        }

        public DotExpression Read(string input)
        {
            _inputStream =
                new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(input)));
            return Read();
        }

        public DotExpression Read(FileStream fileStream)
        {
            _inputStream = new StreamReader(fileStream);
            return Read();
        }

        public static DotAtom ParseAtom(string token)
        {
            if (token[0] == '"')
            {
                return new DotString
                {
                    Value =
                        token.Substring(1, token.Length - 2)
                };
            }

            if (token == "true" || token == "false")
            {
                return new DotBool()
                {
                    Value = bool.Parse(token)
                };
            }

            if (int.TryParse(token, out var integer))
            {
                return new DotNumber()
                {
                    Int = integer
                };
            }

            if (float.TryParse(token,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out var floating))
            {
                return new DotNumber()
                {
                    Float = floating
                };
            }

            return new DotSymbol(token);
        }
    }
}