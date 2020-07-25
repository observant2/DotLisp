using System;
using System.Linq;
using DotLisp.Exceptions;
using DotLisp.Types;

namespace DotLisp.Environments.Core
{
    public static class Math
    {
        public static DotExpression Apply(Func<float, float, float> reducer)
        {
            return DotFunc.From(
                args =>
                {
                    switch (args)
                    {
                        case DotList l:
                        {
                            var nums = l.Expressions.Cast<DotNumber>()
                                .Select(s => s.GetValue());

                            if (nums.Count() <= 1) // edge case for (- x) => -x and (+) => 0
                            {
                                nums = nums.Prepend(0);
                            }

                            var sum = nums.Aggregate(reducer);

                            return new DotNumber()
                            {
                                Float = sum
                            };
                        }
                        default:
                            throw new EvaluatorException(
                                "Illegal arguments for Math.Apply(...)");
                    }
                });
        }

        public static DotExpression Compare(Func<float, float, bool> predicate)
        {
            return DotFunc.From(
                args =>
                {
                    switch (args)
                    {
                        case DotBool b:
                            return b;
                        case DotList l:
                        {
                            var numbers = l.Expressions.Cast<DotNumber>()
                                .Select(s => s.GetValue()).ToList();

                            if (numbers.Count < 2)
                            {
                                throw new EvaluatorException("Cannot compare less than two numbers.");
                            }

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


                            return new DotBool(result);
                        }
                        default:
                            throw new Exception("Illegal arguments for BoolApply.");
                    }
                });
        }

        public static DotFunc Equals()
        {
            return DotFunc.From(
                args =>
                {
                    switch (args)
                    {
                        case DotSymbol _:
                            return DotBool.True();
                        case DotList l:
                        {
                            // TODO: extend equality to objects!

                            var numbers = l.Expressions.Cast<DotNumber>().ToList();

                            if (numbers.Count == 0)
                            {
                                throw new EvaluatorException("Nothing to compare.");
                            }

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
                            return new DotBool(result);
                        }
                    }

                    return DotBool.False();
                });
        }
    }
}