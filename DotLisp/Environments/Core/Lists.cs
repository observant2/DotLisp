using System.Linq;
using DotLisp.Exceptions;
using DotLisp.Parsing;
using DotLisp.Types;

namespace DotLisp.Environments.Core
{
    public static class Lists
    {
        public static Expression First()
        {
            return Func.From(
                (list) =>
                {
                    var args = (list as List).Expressions.First();
                    if (!(args is List l))
                    {
                        throw new EvaluatorException("'first' expects a list!");
                    }

                    if (l.Expressions.Count == 0)
                    {
                        throw new EvaluatorException(
                            "'first' called on empty list!");
                    }

                    return l.Expressions.First();
                });
        }

        public static Expression Cons()
        {
            return Func.From(
                (list) =>
                {
                    if (!(list is List argList) || argList.Expressions.Count != 2)
                    {
                        throw new EvaluatorException(
                            "'cons' expects at least 2 arguments!");
                    }

                    if (!(argList.Expressions.ElementAt(1) is List destinationList))
                    {
                        throw new EvaluatorException(
                            "'cons' expects second argument to be a list");
                    }

                    destinationList.Expressions.AddFirst(
                        argList.Expressions.First());

                    return destinationList;
                });
        }

        public static Expression Rest()
        {
            return Func.From(
                (list) =>
                {
                    var args = (list as List).Expressions.First();
                    if (!(args is List l))
                    {
                        throw new EvaluatorException("'rest' expects a list!");
                    }

                    if (l.Expressions.Count == 0)
                    {
                        throw new EvaluatorException("'rest' called on empty list!");
                    }

                    return new List
                    { Expressions = l.Expressions.Skip(1).ToLinkedList() };
                });
        }
    }
}