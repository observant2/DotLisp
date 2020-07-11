using System;
using DotLisp.Exceptions;
using DotLisp.Types;
using static DotLisp.Parsing.Parser;

namespace DotLisp
{
    class Program
    {
        static void Main()
        {
            var env = new Evaluator();

            while (true)
            {
                Console.Write("> ");
                var input = Console.ReadLine();

                if (input == "(quit)" || input == "(exit)")
                {
                    return;
                }

                try
                {
                    var parsedProgram = Parse(input);
                    Console.WriteLine(ToLisp(env.Eval(parsedProgram)));
                }
                catch (ParserException pe)
                {
                    Console.WriteLine(pe.Message);
                }
                catch (EvaluatorException ee)
                {
                    Console.WriteLine(ee.Message);
                }
            }
        }
    }
}