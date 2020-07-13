using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DotLisp.Exceptions;
using DotLisp.Types;
using Newtonsoft.Json;

namespace DotLisp.Parsing
{
    public static class Parser
    {
        public static string PrettyPrint(this object o)
        {
            return JsonConvert.SerializeObject(o, Formatting.None,
                new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });
        }

        public static IEnumerable<TResult> Pairwise<T, TResult>(this IEnumerable<T> enumerable,
            Func<T, T, TResult> selector)
        {
            var list = enumerable.ToList();
            var previous = list.First();
            foreach (var item in list.Skip(1))
            {
                yield return selector(previous, item);
                previous = item;
            }
        }

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
                    var l = new List<Expression>();
                    while (tokens[0] != ")")
                    {
                        l = l.Append(ReadFromTokens(tokens)).ToList();
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

            return new Symbol()
            {
                Name = token
            };
        }
    }
}