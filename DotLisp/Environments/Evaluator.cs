using System.Collections.Generic;
using System.Linq;
using DotLisp.Exceptions;
using DotLisp.Types;

namespace DotLisp.Environments
{
    public static class Evaluator
    {
        private static readonly Environment GlobalEnvironment =
            new GlobalEnvironment();

        // public Evaluator()
        // {
        //     TODO: Implement dotnet types and add InterOp?
        //     var math = Type.GetType("System.Math");
        //     if (math == null)
        //     {
        //         Console.WriteLine("Not found.");
        //         return;
        //     }
        //     
        //     foreach (var type in math!.GetMembers())
        //     {
        //         Console.Write(
        //             $"{type.Name}, :{type.MemberType} ");
        //     
        //         if (type is MethodInfo methodInfo)
        //         {
        //             Console.WriteLine($"params: {methodInfo.GetParameters().Length}");
        //         }
        //     
        //         if (type is FieldInfo fieldInfo)
        //         {
        //             if (fieldInfo.IsStatic)
        //             {
        //                 Console.WriteLine(fieldInfo.GetRawConstantValue());
        //             }
        //         }
        //     }
        // }

        public static Expression Eval(Expression x, Environment env = null)
        {
            env ??= GlobalEnvironment;

            // The special forms (if, define, ...)
            // are implemented here

            if (x is Symbol symRef) // symbol reference
            {
                return env.Find(symRef.Name).Data[symRef.Name];
            }

            if (!(x is List)) // constant
            {
                return x;
            }

            // everything else is a list
            var l = x as List;

            if (l.Expressions.Count == 0)
            {
                throw new EvaluatorException("An empty list needs to be quoted!");
            }

            if (!(l.Expressions.First() is Symbol op))
            {
                throw new EvaluatorException("Function or special form expected!");
            }

            var args = l.Expressions.Skip(1).ToList();

            switch (op.Name)
            {
                case "quote":
                    return args[0];
                case "if":
                {
                    if (l.Expressions.Count != 4)
                    {
                        throw new EvaluatorException("'if' requires 2 parameters!");
                    }

                    var test = args[0];
                    var consequence = args[1];
                    var alternative = args[2];

                    if (!(Eval(test, env) is Bool b))
                    {
                        throw new EvaluatorException("'if' requires a bool!");
                    }

                    var toEval = b.Value ? consequence : alternative;

                    return Eval(toEval, env);
                }
                case "def":
                {
                    var name = (args[0] as Symbol).Name;
                    // TODO: Allow overwriting of existing definitions
                    if (env.Data.ContainsKey(name))
                    {
                        throw new EvaluatorException(
                            $"Symbol {name} already defined!");
                    }

                    var data = Eval(args[1], env);
                    env.Data.Add(name, data);
                    return data;
                }
                case "set!":
                {
                    var (symbol, exp) = (args[0], args[1]);
                    if (!(symbol is Symbol s))
                    {
                        throw new EvaluatorException(
                            "'set!' expects a symbol name as first parameter!");
                    }

                    // TODO: Evaluation happens first
                    // then assignment. If assignment fails,
                    // evaluation happened anyway. Is this
                    // wanted behavior?
                    var data = Eval(exp, env);
                    env.Find(s.Name).Data[s.Name] = data;
                    return data;
                }
                case "fn":
                {
                    var (parameters, body) = (args[0], args[1]);
                    return new Procedure(parameters, body, env);
                }
            }

            // all other special forms failed to match
            // it has to be a simple function call

            var exps = l.Expressions;

            var functionToCall = env.Find(op.Name).Data[op.Name];

            var arguments = new List() {Expressions = new LinkedList<Expression>()};
            foreach (var exp in exps.Skip(1).ToList())
            {
                arguments.Expressions.AddLast(Eval(exp, env));
            }

            if (functionToCall is Func f)
            {
                return f.Action.Invoke(arguments);
            }

            return (functionToCall as Procedure).Call(arguments);
        }
    }
}