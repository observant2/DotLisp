using System;
using System.Linq;
using DotLisp.Exceptions;
using DotLisp.Types;

namespace DotLisp.Environments.Core
{
    public static class Math
    {
        public static Expression Apply(Func<float, float, float> reducer)
        {
            return Func.From(
                args =>
                {
                    switch (args)
                    {
                        case List l:
                        {
                            var sum = l.Expressions.Cast<Number>()
                                .Select(s => s.GetValue())
                                .Aggregate(reducer);

                            return new Number()
                            {
                                Float = sum
                            };
                        }
                        default:
                            throw new EvaluatorException("Illegal arguments for +");
                    }
                });
        }

        public static Expression Compare(Func<float, float, bool> predicate)
        {
            return Func.From(
                args =>
                {
                    switch (args)
                    {
                        case Bool b:
                            return b;
                        case List l:
                        {
                            var numbers = l.Expressions.Cast<Number>()
                                .Select(s => s.GetValue()).ToList();

                            var (_, result) = numbers.Skip(1).Aggregate(
                                (numbers[0], true), (tuple, b) =>
                                {
                                    var (a, resultSoFar) = tuple;

                                    if (!resultSoFar)
                                    {
                                        return (b, false);
                                    }

                                    var predicateStillTrue = predicate(a, b);

                                    return (b, predicateStillTrue);
                                });


                            return new Bool(result);
                        }
                        default:
                            throw new Exception("Illegal arguments for BoolApply.");
                    }
                });
        }

        public static Func Equals()
        {
            return Func.From(
                args =>
                {
                    switch (args)
                    {
                        case Symbol _:
                            return Bool.True();
                        case List l:
                        {
                            // TODO: extend equality to objects!

                            var numbers = l.Expressions.Cast<Number>().ToList();

                            var first = numbers[0];

                            var (_, result) = numbers.Skip(1).Aggregate(
                                seed: (first, true), func: (tuple, b) =>
                                {
                                    var (a, resultSoFar) = tuple;

                                    if (!resultSoFar)
                                    {
                                        return (b, false);
                                    }

                                    var predicateStillTrue =
                                        a.GetValue() == b.GetValue();

                                    return (b, predicateStillTrue);
                                });
                            return new Bool(result);
                        }
                    }

                    return Bool.False();
                });
        }
    }
}