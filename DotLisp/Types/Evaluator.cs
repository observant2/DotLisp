using System;
using System.Collections.Generic;
using System.Linq;
using DotLisp.Exceptions;
using DotLisp.Parsing;
using static DotLisp.Parsing.Parser;

namespace DotLisp.Types
{
    public class Evaluator
    {
        public readonly Dictionary<string, Expression> Environment = new Dictionary<string, Expression>()
        {
            ["begin"] = new Func()
            {
                Action = (expr) =>
                {
                    if (expr is List l)
                    {
                        return new List()
                        {
                            Expressions = l.Expressions.Skip(1).ToList()
                        };
                    }

                    throw new EvaluatorException("'begin' needs at least one parameter");
                }
            },

            ["+"] = Apply((acc, x) => acc + x),
            ["-"] = Apply((acc, x) => acc - x),
            ["*"] = Apply((acc, x) => acc * x),
            ["/"] = Apply((acc, x) => acc / x),

            [">"] = BoolApply((a, b) => a > b),
            [">="] = BoolApply((a, b) => a >= b),
            ["<"] = BoolApply((a, b) => a < b),
            ["<="] = BoolApply((a, b) => a <= b),
            ["=="] = Equals(),

            ["PI"] = new Number() {Float = (float) Math.PI},
            ["E"] = new Number() {Float = (float) Math.E}
        };

        public static Expression Apply(Func<float, float, float> reducer)
        {
            return new Func()
            {
                Action = args =>
                {
                    switch (args)
                    {
                        case Number n:
                            return n;
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
                }
            };
        }

        public static Expression BoolApply(Func<float, float, bool> predicate)
        {
            return new Func()
            {
                Action = args =>
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
                }
            };
        }

        public static Func Equals()
        {
            return new Func()
            {
                Action = args =>
                {
                    switch (args)
                    {
                        case Symbol _:
                            return Bool.True();
                        case List l:
                        {
                            // TODO: extend equality to objects?

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

                                    var predicateStillTrue = a.GetValue() == b.GetValue();

                                    return (b, predicateStillTrue);
                                });
                            return new Bool(result);
                        }
                    }

                    return Bool.False();
                }
            };
        }

        public Evaluator()
        {
            // TODO: Implement dotnet types and add InterOp.
            // var math = Type.GetType("System.Math");
            // if (math == null)
            // {
            //     Console.WriteLine("Not found.");
            //     return;
            // }
            //
            // foreach (var type in math!.GetMembers())
            // {
            //     Console.Write(
            //         $"{type.Name}, :{type.MemberType} ");
            //
            //     if (type is MethodInfo methodInfo)
            //     {
            //         Console.WriteLine($"params: {methodInfo.GetParameters().Length}");
            //     }
            //
            //     if (type is FieldInfo fieldInfo)
            //     {
            //         if (fieldInfo.IsStatic)
            //         {
            //             Console.WriteLine(fieldInfo.GetRawConstantValue());
            //         }
            //     }
            // }
        }

        public Expression Eval(Expression x)
        {
            // Here the special forms (if, define, ...)
            // are implemented.
            switch (x)
            {
                case Symbol s:
                    if (Environment.TryGetValue(s.Name, out var val))
                    {
                        return val;
                    }

                    throw new EvaluatorException($"Symbol {s.Name} not found.");
                case Number n:
                    return n;
                case List l when l.Expressions.Count == 0:
                    throw new EvaluatorException("List cannot be empty");
                case List l when l.Expressions[0] is Symbol s
                                 && s.Name == "if":
                    
                    if (l.Expressions.Count != 4)
                    {
                        throw new EvaluatorException("'if' requires 2 parameters!");
                    }
                    
                    var test = l.Expressions[1];
                    var consequence = l.Expressions[2];
                    var alternative = l.Expressions[3];

                    if (!(Eval(test) is Bool b))
                    {
                        throw new EvaluatorException("'if' requires a bool!");
                    }

                    var toEval = b.Value ? consequence : alternative;

                    return Eval(toEval);
                case List l when l.Expressions[0] is Symbol s
                                 && s.Name == "define":
                    var name = (l.Expressions[1] as Symbol).Name;
                    // TODO: Allow overwriting of existing definitions
                    Environment.Add(name, Eval(l.Expressions[2]));
                    break;
                case List l: // function call
                {
                    var exps = l.Expressions;

                    Func functionToCall = null;

                    if (!(exps[0] is Symbol sym))
                    {
                        throw new EvaluatorException("Function call expected!");
                    }


                    if (Environment.TryGetValue(sym.Name, out var func))
                    {
                        functionToCall = func as Func;
                    }

                    var args = new List() {Expressions = new List<Expression>()};
                    foreach (var exp in exps.Skip(1).ToList())
                    {
                        args.Expressions.Add(Eval(exp));
                    }

                    return functionToCall?.Action.Invoke(args);
                }
            }

            return x;
        }
    }
}