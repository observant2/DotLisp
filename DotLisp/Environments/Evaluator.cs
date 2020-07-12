using System.Collections.Generic;
using System.Linq;
using DotLisp.Exceptions;
using DotLisp.Types;

namespace DotLisp.Environments
{
    public class Evaluator
    {
        private readonly Environment _globalEnvironment = new GlobalEnvironment();

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

        public Expression Eval(Expression x, Environment env = null)
        {
            env ??= _globalEnvironment;

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
                throw new EvaluatorException("List cannot be empty");
            }

            if (!(l.Expressions[0] is Symbol op))
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
                case "define":
                {
                    var name = (args[0] as Symbol).Name;
                    // TODO: Allow overwriting of existing definitions
                    if (env.Data.ContainsKey(name))
                    {
                        throw new EvaluatorException($"Symbol {name} already defined!");
                    }

                    var data = Eval(args[1], env);
                    env.Data.Add(name, data);
                    return data;
                }
            }

            // all other special forms failed to match
            // it has to be a simple function call

            var exps = l.Expressions;

            var functionToCall = env.Find(op.Name).Data[op.Name] as Func;

            var vals = new List() {Expressions = new List<Expression>()};
            foreach (var exp in exps.Skip(1).ToList())
            {
                vals.Expressions.Add(Eval(exp, env));
            }

            return functionToCall?.Action.Invoke(vals);
        }
    }
}