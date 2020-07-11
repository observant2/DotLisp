using System;
using System.Collections.Generic;

namespace DotLisp.Types
{
    public abstract class Expression
    {
    }

    public abstract class Atom : Expression
    {
    }

    public class Symbol : Atom
    {
        public string Name { get; set; }
    }

    public class Func : Expression
    {
        public delegate Expression DoSomething(Expression args);

        public DoSomething Action { get; set; }
    }

    public class List : Expression
    {
        public List<Expression> Expressions { get; set; }
    }

    public class Number : Atom
    {
        public int? Int { get; set; }
        public float? Float { get; set; }

        public bool IsFloat() => this.Float != null;

        public float GetValue() =>
            IsFloat() ? this.Float!.Value : this.Int!.Value;
    }

    public class Bool : Atom
    {
        public bool Value { get; set; }

        public Bool(bool value)
        {
            Value = value;
        }

        public Bool()
        {
        }

        public static Bool True()
        {
            return new Bool()
            {
                Value = true
            };
        }

        public static Bool False()
        {
            return new Bool()
            {
                Value = false
            };
        }
    }
}