using DotLisp.Environments;
using DotLisp.Exceptions;
using DotLisp.Parsing;
using DotLisp.Types;
using Xunit;

namespace Tests
{
    public class UnitTest1
    {
        private readonly InPort _inPort = new InPort("");

        [Fact]
        public void Math()
        {
            var result = Eval("(+ 2 2)");

            Assert.IsType<Number>(result);
            if (result is Number n1)
            {
                Assert.Equal(2 + 2, n1.GetValue());
            }

            result = Eval("(- 3 2)");

            Assert.IsType<Number>(result);
            if (result is Number n2)
            {
                Assert.Equal(3 - 2, n2.GetValue());
            }

            result = Eval("(* 3 2)");

            Assert.IsType<Number>(result);
            if (result is Number n3)
            {
                Assert.Equal(3 * 2, n3.GetValue());
            }

            result = Eval("(/ 3 2)");

            Assert.IsType<Number>(result);
            if (result is Number n4)
            {
                Assert.Equal(3.0 / 2, n4.GetValue());
            }
        }

        [Fact]
        public void SpecialForms()
        {
            If();
            Cons();
        }

        private void If()
        {
            Assert.Throws<EvaluatorException>(() => { Eval("(if)"); });

            var result = Eval("(if true 2 1)");
            Assert.IsType<Number>(result);
            Assert.Equal(2, (result as Number).GetValue());

            result = Eval("(if false 2 1)");
            Assert.IsType<Number>(result);
            Assert.Equal(1, (result as Number).GetValue());
        }

        private void Cons()
        {
            Assert.Throws<EvaluatorException>(() => { Eval("(cons 1 2)"); });

            Assert.Equal("(1 2 3)", Parser.ToLisp(Eval("(cons 1 '(2 3))")));
        }


        private Expression Eval(string program)
        {
            return Evaluator.Eval(_inPort.Read(program));
        }
    }
}