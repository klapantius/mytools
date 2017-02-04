using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Push
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var cmdInterpreter = new CmdLine.Interpreter();
            cmdInterpreter.Add(new CmdLine.Parameter(new[] { "source", "src" }, "source path", true));
            cmdInterpreter.Add(new CmdLine.Parameter(new[] { "target", "tgt" }, "target path", true));
            if (!cmdInterpreter.Parse(string.Join(" ", args)))
            {
                cmdInterpreter.PrintErrors("push.exe");
                return;
            }

        }
    }
}