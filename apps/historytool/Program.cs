using System;
using System.Linq;

using juba.consoleapp.CmdLine;
using juba.consoleapp.Out;
using juba.tfs.interfaces;
using juba.tfs.wrappers;

using SimpleInjector;

namespace HistoryTool
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var ioc = new Container();
            ioc.Register<IConsoleAppOut, ConsoleOut>(Lifestyle.Singleton);
            ioc.Register<ICmdLineInterpreter, Interpreter>(Lifestyle.Singleton);
            ioc.Register<IVersionControlServer, VersionControlServerWrapper>(Lifestyle.Singleton);
            ioc.Register(() => new Uri("https://tfs.healthcare.siemens.com:8090/tfs/ikm.tpc.projects"), Lifestyle.Singleton);

            var myOut = ioc.GetInstance<IConsoleAppOut>();
            var cmd = ioc.GetInstance<ICmdLineInterpreter>();
            var vcs = ioc.GetInstance<IVersionControlServer>();

            cmd.Add(new Parameter(new[] { "serverpath", "path" }, "full path", "folder to check history of", true));
            cmd.Add(new Command(new[] { "toupload" }, "list CSs added since last upload", () =>
            {
                cmd.Parse(args);
                var folder = cmd.ValueOf("serverpath");
                var changes = vcs.QueryHistory(folder, true, false, DateTime.Now - TimeSpan.FromDays(7), true).ToList();
                var upload = changes.FirstOrDefault(c => c.Comment.StartsWith("created by build process"));
                if (upload == null)
                {
                    myOut.Error("Could not find an upload CS on the specified path. Isn't that a main branch?");
                    return;
                }
                foreach (var changeset in changes.Where(c => c.CreationDate > upload.CreationDate))
                {
                    myOut.Info(changeset.ToString());
                }
            })).Requires("serverpath");
            cmd.Add(new Parameter(new[] { "anykey" }, "bool", "doesn't exit at the end", false, "false"));

            cmd.ExecuteCommands();

            if (cmd.Evaluate("anykey", Interpreter.DefaultBoolConverter))
            {
                Console.WriteLine("\n\ndone, please press a key to finish the execution");
                Console.ReadKey();
            }
        }
    }
}
