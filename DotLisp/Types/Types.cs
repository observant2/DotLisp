using System;
using System.Collections.Generic;
using System.Linq;
using DotLisp.Environments;
using DotLisp.Exceptions;
using DotLisp.Parsing;
using Environment = DotLisp.Environments.Environment;

namespace DotLisp.Types
{
    public abstract class DotExpression
    {
        public new abstract string ToString();
    }

    public abstract class DotAtom : DotExpression
    {
    }

    public class DotSymbol : DotAtom
    {
        public string Name { get; set; }

        public DotSymbol(string name)
        {
            Name = name;
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public class DotFunc : DotExpression
    {
        public delegate DotExpression DoSomething(DotExpression args);

        public DoSomething Action { get; set; }

        public override string ToString()
        {
            return $"builtin function '{Action.Method.Name}'";
        }

        public static DotFunc From(Func<DotExpression, DotExpression> f)
        {
            return new DotFunc()
            {
                Action = new DoSomething(f)
            };
        }
    }

    public class DotList : DotExpression
    {
        public LinkedList<DotExpression> Expressions { get; set; }

        public override string ToString()
        {
            return "(" +
                   string.Join(" ", Expressions.Select(e => e.ToString()).ToList()) +
                   ")";
        }
    }

    public class DotNumber : DotAtom
    {
        public int? Int { get; set; }
        public float? Float { get; set; }

        public bool IsFloat() => this.Float != null;

        public float GetValue() =>
            IsFloat() ? this.Float!.Value : this.Int!.Value;

        public override string ToString()
        {
            return IsFloat() ? Float!.ToString() : Int!.ToString();
        }
    }

    public class DotString : DotAtom
    {
        public string Value { get; set; }

        public override string ToString()
        {
            return $"\"{Value}\"";
        }
    }

    public class DotBool : DotAtom
    {
        public bool Value { get; set; }

        public DotBool(bool value)
        {
            Value = value;
        }

        public DotBool()
        {
        }

        public static DotBool True()
        {
            return new DotBool()
            {
                Value = true
            };
        }

        public static DotBool False()
        {
            return new DotBool()
            {
                Value = false
            };
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }

    /// A user defined procedure. This is different
    /// from Func, which represents builtin functions
    /// that directly map to C# code and act independently
    /// of their current environment.
    public class DotProcedure : DotExpression
    {
        private readonly List<string> _parameters;

        private readonly DotExpression _body;

        private readonly Environment _env;

        public DotProcedure(DotExpression parameters, DotExpression body,
            Environment env)
        {
            if (!(parameters is DotList l))
            {
                throw new EvaluatorException("Parameter list expected!");
            }

            Console.WriteLine(l.PrettyPrint());

            var names = l.Expressions.Cast<DotSymbol>().Select(s => s.Name)
                .ToList();

            _parameters = names;
            _body = body;
            _env = env;
        }

        public DotExpression Call(DotExpression args)
        {
            LinkedList<DotExpression> exps;
            if (!(args is DotList l))
            {
                exps = new LinkedList<DotExpression>();
                exps.AddFirst(args);
            }
            else
            {
                exps = l.Expressions;
            }

            return Evaluator.Eval(_body,
                new Environment(_parameters, exps, _env));
        }

        public override string ToString()
        {
            return $"procedure with {_parameters.Count} parameters";
        }
    }
}