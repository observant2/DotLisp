using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
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
    public class Parser
    {
        public class Result
        {
            public DotExpression AST { get; set; }
            public List<ParserError> ParserErrors { get; set; }
        }

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

        public List<ParserError> ParserErrors { get; set; } =
            new List<ParserError>();

        private StreamReader _inputStream;

        private string _line = "";
        public int CurColumn = 0;
        public int CurLine = 0;

        public Parser()
        {
        }

        public Parser(StreamReader inputStream)
        {
            _inputStream = inputStream;
            CurLine = 0;
            CurColumn = 0;
        }

        public Parser(Stream stream) : this(new StreamReader(stream))
        {
        }

        public Parser(string input) : this(
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
                    CurLine += 1;
                    CurColumn = 0;
                }

                if (string.IsNullOrEmpty(_line) && _inputStream.EndOfStream)
                {
                    return null;
                }

                // Incomplete string cannot be recognized...
                if (_line.Count(c => c == '\"') == 1)
                {
                    // Skip over it. May lead to weird error messages?
                    _line = _line.ReplaceFirst("\"", "");
                }

                var match = _tokenizer.Match(_line).Groups[1].Value;

                var whitespaceInFrontOfToken =
                    _line.IndexOf(match, StringComparison.Ordinal);

                var token = match;

                // The order is important here! First trim, then replace the found token.
                // Otherwise the whitespace in the line adds up.
                _line = _line.TrimStart();
                _line = _line.ReplaceFirst(token, "");
                CurColumn += whitespaceInFrontOfToken;

                // Console.WriteLine($"_line at the end: {_line}");
                // Console.WriteLine($"Token: '{token}' ({CurLine}:{CurColumn})");
                if (token != "" && !token.StartsWith(";"))
                {
                    return token;
                }
            }
        }

        public static string ReadChar(Parser parser)
        {
            if (parser._line != "")
            {
                var ch = "" + parser._line[0];
                parser._line = parser._line.Substring(1);
                return ch;
            }
            else
            {
                return "" + Convert.ToChar(parser._inputStream.Read());
            }
        }

        private DotExpression ReadAhead(string token)
        {
            switch (token)
            {
                case "(":
                    CurColumn += 1;
                    var l = new DotList()
                    {
                        Line = CurLine,
                        Column = CurColumn,
                        Expressions = new LinkedList<DotExpression>()
                    };
                    while (true)
                    {
                        token = NextToken();
                        if (token == ")")
                        {
                            CurColumn += 1;
                            return l;
                        }

                        l.Expressions.AddLast(ReadAhead(token));
                    }

                case ")":
                    CurColumn += 1;
                    ParserErrors.Add(new ParserError()
                    {
                        Line = CurLine,
                        Column = CurColumn,
                        Message = "Unexpected ')'!"
                    });
                    token = NextToken();
                    return ReadAhead(token);
            }

            if (_quotes.ContainsKey(token))
            {
                // convert to real expression
                var keyword = _quotes[token];
                var exps = new LinkedList<DotExpression>();

                exps.AddLast(new DotSymbol(keyword, CurLine, CurColumn));
                exps.AddLast(Read().AST);

                return new DotList
                {
                    Line = CurLine,
                    Column = CurColumn,
                    Expressions = exps
                };
            }

            var ret = ParseAtom(token, CurLine, CurColumn);
            CurColumn += token.Length;
            return ret;
        }

        public Result Read()
        {
            var token1 = NextToken();
            var ast = token1 == null ? null : ReadAhead(token1);
            return new Result()
            {
                AST = ast,
                ParserErrors = ParserErrors
            };
        }

        /// Reads one s-expression at a time
        public Result Read(string input)
        {
            ParserErrors.Clear();
            _inputStream =
                new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(input)));
            return Read();
        }

        public Result Read(FileStream fileStream)
        {
            ParserErrors.Clear();
            _inputStream = new StreamReader(fileStream);
            return Read();
        }

        public static DotAtom ParseAtom(string token, int line, int column)
        {
            if (token[0] == '"')
            {
                return new DotString
                {
                    Line = line,
                    Column = column,
                    Value =
                        token.Substring(1, token.Length - 2)
                };
            }

            if (token == "true" || token == "false")
            {
                return new DotBool()
                {
                    Line = line,
                    Column = column,
                    Value = bool.Parse(token)
                };
            }

            if (int.TryParse(token, out var integer))
            {
                return new DotNumber()
                {
                    Line = line,
                    Column = column,
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
                    Line = line,
                    Column = column,
                    Float = floating
                };
            }

            return new DotSymbol(token, line, column);
        }
    }

    public class ParserError
    {
        public string Message { get; set; }
        public int Line { get; set; }
        public int Column { get; set; }
    }
}