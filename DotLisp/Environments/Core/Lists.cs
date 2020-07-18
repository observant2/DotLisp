using System.Collections.Generic;
using System.Linq;
using DotLisp.Exceptions;
using DotLisp.Types;

namespace DotLisp.Environments.Core
{
    public static class Lists
    {
        public static DotExpression First()
        {
            return DotFunc.From(
                (list) =>
                {
                    var args = (list as DotList).Expressions.First();
                    if (!(args is DotList l))
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

        // TODO: Implement a real cons :)
        public static DotExpression Cons()
        {
            return DotFunc.From(
                (list) =>
                {
                    if (!(list is DotList argList) || argList.Expressions.Count != 2)
                    {
                        throw new EvaluatorException(
                            "'cons' expects 2 arguments!");
                    }

                    if (argList.Expressions.ElementAt(1) is DotList destinationList)
                    {
                        destinationList.Expressions.AddFirst(
                            argList.Expressions.First());

                        return destinationList;
                    }


                    return argList;
                });
        }

        public static DotExpression Concat()
        {
            return DotFunc.From(
                (list) =>
                {
                    var args = list as DotList;

                    return args.Expressions.SelectMany(arg =>
                    {
                        if (arg is DotList lst)
                        {
                            return lst.Expressions;
                        }

                        var ret = new LinkedList<DotExpression>();
                        ret.AddLast(arg);
                        return ret;
                    }).ToDotList();
                });
        }

        public static DotExpression Rest()
        {
            return DotFunc.From(
                (list) =>
                {
                    var args = (list as DotList).Expressions.First();
                    if (!(args is DotList l))
                    {
                        throw new EvaluatorException("'rest' expects a list!");
                    }

                    if (l.Expressions.Count == 0)
                    {
                        throw new EvaluatorException("'rest' called on empty list!");
                    }

                    return new DotList
                    { Expressions = l.Expressions.Skip(1).ToLinkedList() };
                });
        }
    }
}