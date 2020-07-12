using System;
using DotLisp.Environments;
using DotLisp.Exceptions;
using static DotLisp.Parsing.Parser;

namespace DotLisp
{
    class Program
    {
        static void Main()
        {
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
                    Console.WriteLine(ToLisp(Evaluator.Eval(parsedProgram)));
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