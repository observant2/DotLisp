using System;
using System.IO;
using System.Text;
using System.Text.Unicode;
using DotLisp.Environments;
using DotLisp.Exceptions;
using DotLisp.Parsing;
using static DotLisp.Parsing.Parser;

namespace DotLisp
{
    class Program
    {
        static void Main()
        {
            var ip = new InPort(new StreamReader(Console.OpenStandardInput(),
                Encoding.UTF8));
            while (true)
            {
                Console.Write("> ");
                var input = ip.Read();

                // if (input == "(quit)" || input == "(exit)")
                // {
                //     return;
                // }

                try
                {
                    // var parsedProgram = Parse(input);
                    // Console.WriteLine(ToLisp(Evaluator.Eval(parsedProgram)));
                    Console.WriteLine(ToLisp(Evaluator.Eval(input)));
                }
                catch (ParserException pe)
                {
                    Console.WriteLine(pe.Message);
                }
                catch (EvaluatorException ee)
                {
                    Console.WriteLine(ee.Message);
                }

                // var ip = new InPort(
                //     new StreamReader(new MemoryStream(
                //         Encoding.UTF8.GetBytes(
                //             "(begin (cons 2 `(,@(1 2 3) \"hallo\" 3)))"))));
                //
                // // ip = new InPort(
                // //     new StreamReader(
                // //         "../../../" + 
                // //         @"./Examples/test.dl")
                // // );
                //
                // var expressions = ip.Read();
                //
                // Console.WriteLine(expressions.PrettyPrint());
            }
        }
    }
}