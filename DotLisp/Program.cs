using System.IO;
using DotLisp.Environments;
using DotLisp.Parsing;

namespace DotLisp
{
    class Program
    {
        static void Main(string[] filesToPreload)
        {
            foreach (var file in filesToPreload)
            {
                using var fileStream = File.Open(file, FileMode.Open);
                GlobalEnvironment.Repl(
                    null,
                    new InPort(fileStream),
                    false);
            }

            GlobalEnvironment.Repl();
        }
    }
}