using System;
using System.Collections.Generic;
using System.Linq;
using DotLisp.Environments;
using DotLisp.Exceptions;
using DotLisp.Types;

namespace DotLisp.Parsing
{
    /// The main purpose of the expander is to
    /// expand macros and quotes
    public static class Expander
    {
        public static DotExpression Expand(DotExpression expression,
            bool topLevel = false)
        {
            if (!(expression is DotList l))
            {
                // constants can't be expanded
                return expression;
            }

            if (l.Expressions.Count == 0)
            {
                throw new ParserException(
                    "An empty list needs to be quoted!", l.Line, l.Column);
            }

            var args = l.Expressions.Skip(1).ToList();

            // if (!(l.Expressions.First() is DotSymbol op))
            // {
            //     throw new EvaluatorException(
            //         "Function or special form expected!");
            // }

            if (l.Expressions.First() is DotSymbol op)
            {
                if (op.Name == "quote")
                {
                    return expression;
                }

                // TODO Find a generic way to check function and procedure argument length and type

                if (op.Name == "def" || op.Name == "defmacro")
                {
                    if (args.Count != 2)
                    {
                        throw new ParserException("name and body expected!", op.Line,
                            op.Column);
                    }

                    var defBody = Expand(args.ElementAt(1));

                    if (op.Name == "defmacro")
                    {
                        if (!topLevel)
                        {
                            throw new ParserException(
                                "'defmacro' only allowed at top level!", op.Line,
                                op.Column);
                        }

                        var procedure = Evaluator.Eval(defBody);

                        if (!(procedure is DotProcedure dp))
                        {
                            throw new ParserException(
                                "A macro must be a procedure", op.Line, op.Column);
                        }

                        // Add macro
                        GlobalEnvironment.MacroTable.Add(
                            (args.ElementAt(0) as DotSymbol).Name,
                            dp
                        );

                        return DotBool.True();
                    }

                    var expandedDefinition = new LinkedList<DotExpression>();

                    expandedDefinition.AddLast(op);
                    expandedDefinition.AddLast(args.First());
                    expandedDefinition.AddLast(defBody);

                    return new DotList()
                    {
                        Expressions = expandedDefinition
                    };
                }

                if (op.Name == "quasiquote")
                {
                    return ExpandQuasiquote(args.First());
                }

                if (GlobalEnvironment.MacroTable.ContainsKey(op.Name))
                {
                    // call macro
                    var macro = GlobalEnvironment.MacroTable[op.Name];
                    var macroArgs = new DotList()
                    {
                        Expressions = args.ToLinkedList()
                    };

                    return Expand(macro.Call(macroArgs), topLevel);
                }
            }

            l.Expressions = l.Expressions
                .Select(exp => Expand(exp, false))
                .ToLinkedList();
            return l;
        }

        private static DotExpression ExpandQuasiquote(DotExpression expression)
        {
            Func<DotExpression, bool> isPair =
                exp => exp is DotList l && l.Expressions.Count != 0;

            if (!isPair(expression)) // `x => 'x
            {
                var quotedExpr = new LinkedList<DotExpression>();
                quotedExpr.AddLast(new DotSymbol("quote", expression.Line,
                    expression.Column));
                quotedExpr.AddLast(expression);

                return quotedExpr.ToDotList();
            }

            var list = (expression as DotList).Expressions;

            if (list.First() is DotSymbol ds && ds.Name == "unquote")
            {
                return list.ElementAt(1);
            }

            if (isPair(expression) &&
                (list.First() is DotList uqList)
                && (uqList.Expressions.First() as DotSymbol).Name ==
                "unquotesplicing")
            {
                var first = uqList.Expressions.Skip(1).First();
                var others = list.Skip(1).ToDotList();

                var expandedSplice = new LinkedList<DotExpression>();

                expandedSplice.AddLast(new DotSymbol("concat", expression.Line,
                    expression.Column));
                expandedSplice.AddLast(first);
                expandedSplice.AddLast(ExpandQuasiquote(others));

                return expandedSplice.ToDotList();
            }

            var ret = new LinkedList<DotExpression>();
            ret.AddLast(new DotSymbol("cons", expression.Line, expression.Column));
            ret.AddLast(ExpandQuasiquote(list.First()));
            var rest = list.Skip(1).ToDotList();
            ret.AddLast(ExpandQuasiquote(rest));

            return ret.ToDotList();
        }
    }
}