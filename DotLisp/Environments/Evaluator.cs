using System.Collections.Generic;
using System.Linq;
using DotLisp.Exceptions;
using DotLisp.Types;

namespace DotLisp.Environments
{
    public static class Evaluator
    {
        public static readonly Environment GlobalEnvironment =
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

        public static DotExpression Eval(DotExpression x, Environment env = null)
        {
            env ??= GlobalEnvironment;

            while (true)
            {
                if (x is DotSymbol symRef) // symbol reference
                {
                    return env.Find(symRef.Name).Data[symRef.Name];
                }

                if (!(x is DotList)) // constant
                {
                    return x;
                }

                // everything else is a list
                var l = x as DotList;

                if (!(l.Expressions.First() is DotSymbol op))
                {
                    throw new EvaluatorException(
                        "Function or special form expected!");
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
                            throw new EvaluatorException(
                                "'if' requires 2 parameters!");
                        }

                        var test = args[0];
                        var consequence = args[1];
                        var alternative = args[2];

                        if (!(Eval(test, env) is DotBool b))
                        {
                            throw new EvaluatorException("'if' requires a bool!");
                        }

                        var toEval = b.Value ? consequence : alternative;

                        x = Eval(toEval, env);
                        continue;
                    }
                    case "set!":
                    {
                        var (symbol, exp) = (args[0], args[1]);
                        if (!(symbol is DotSymbol s))
                        {
                            throw new EvaluatorException(
                                "'set!' expects a symbol name as first parameter!");
                        }

                        var foundEnv = env.Find(s.Name);
                        if (foundEnv?.Data[s.Name] == null)
                        {
                            throw new EvaluatorException(
                                $"'set!': symbol '{s.Name}' not found.");
                        }

                        var data = Eval(exp, env);
                        foundEnv.Data[s.Name] = data;
                        return data;
                    }
                    case "def":
                    {
                        var name = (args[0] as DotSymbol).Name;
                        if (env.Data.ContainsKey(name))
                        {
                            throw new EvaluatorException(
                                $"Symbol '{name}' already defined!");
                        }

                        var data = Eval(args[1], env);
                        env.Data.Add(name, data);
                        return data;
                    }
                    case "fn":
                    {
                        var (parameters, body) = (args[0], args[1]);
                        return new DotProcedure(parameters, body, env);
                    }
                    case "do":
                    {
                        foreach (var expression in args.SkipLast(1))
                        {
                            Eval(expression, env);
                        }

                        // return only value of last expression
                        x = args.Last();
                        continue;
                    }
                }

                // all other special forms failed to match
                // it has to be a simple function call

                var exps = l.Expressions;


                var arguments = new DotList()
                    {Expressions = new LinkedList<DotExpression>()};
                foreach (var exp in exps.Skip(1).ToList())
                {
                    arguments.Expressions.AddLast(Eval(exp, env));
                }

                var functionToCall = env.Find(op.Name).Data[op.Name];

                if (functionToCall is DotFunc f)
                {
                    return f.Action.Invoke(arguments);
                }

                if (functionToCall is DotProcedure proc)
                {
                    // Enable tail call optimization.
                    // Instead of recursion, set the current environment and expressions
                    // to the function's environment and body.
                    env = new Environment(
                        proc.Parameters,
                        arguments.Expressions,
                        proc.Env);
                    x = proc.Body;
                    continue;
                }
            }
        }
    }
}