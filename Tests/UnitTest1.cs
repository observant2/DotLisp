using DotLisp.Environments;
using DotLisp.Exceptions;
using DotLisp.Parsing;
using DotLisp.Types;
using Xunit;

namespace Tests
{
    public class UnitTest1
    {
        private readonly Parser _parser = new Parser("");

        // runs before every test
        public UnitTest1()
        {
            Evaluator.ClearEnvironment();
        }

        [Fact]
        public void Math()
        {
            var result = Eval("(+ 2 2)");

            Assert.IsType<DotNumber>(result);
            if (result is DotNumber n1)
            {
                Assert.Equal(2 + 2, n1.GetValue());
            }

            result = Eval("(- 3 2)");

            Assert.IsType<DotNumber>(result);
            if (result is DotNumber n2)
            {
                Assert.Equal(3 - 2, n2.GetValue());
            }

            result = Eval("(* 3 2)");

            Assert.IsType<DotNumber>(result);
            if (result is DotNumber n3)
            {
                Assert.Equal(3 * 2, n3.GetValue());
            }

            result = Eval("(/ 3 2)");

            Assert.IsType<DotNumber>(result);
            if (result is DotNumber n4)
            {
                Assert.Equal(3.0 / 2, n4.GetValue());
            }
        }

        [Fact]
        public void SpecialForms()
        {
            If();
            Cons();
            Do();
        }

        private void If()
        {
            Assert.Throws<EvaluatorException>(() => { Eval("(if)"); });

            var result = Eval("(if true 2 1)");
            Assert.IsType<DotNumber>(result);
            Assert.Equal(2, (result as DotNumber).GetValue());

            result = Eval("(if false 2 1)");
            Assert.IsType<DotNumber>(result);
            Assert.Equal(1, (result as DotNumber).GetValue());
        }

        private void Cons()
        {
            Assert.Equal("(8 12)",
                Eval("(cons 8 12)").ToString());
            Assert.Equal("(1 2 3)",
                Eval("(cons 1 '(2 3))").ToString());
        }

        private void Do()
        {
            Assert.Equal("10",
                Eval("(do (def y 3) (+ 5 5))").ToString());

            Assert.Equal("3", Eval("y").ToString());
        }

        [Fact]
        public void Macros()
        {
            Eval("(def lst '(1 2 3))");

            Assert.Equal("(1 2 3 4 5)",
                Eval("`(,@lst ,@'(4 5))").ToString());
        }

        private DotExpression Eval(string program)
        {
            return Evaluator.Eval(Expander.Expand(_parser.Read(program).AST));
        }
    }
}