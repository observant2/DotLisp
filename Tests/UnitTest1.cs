using DotLisp.Environments;
using DotLisp.Exceptions;
using DotLisp.Parsing;
using DotLisp.Types;
using Xunit;

namespace Tests
{
    public class UnitTest1
    {
        private readonly Evaluator _evaluator = new Evaluator();

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
            Assert.Throws<EvaluatorException>(() => { Eval("(if)"); });
        }


        private Expression Eval(string program)
        {
            return _evaluator.Eval(Parser.Parse(program));
        }
    }
}