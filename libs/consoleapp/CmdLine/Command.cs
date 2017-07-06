using System;
using System.Collections.Generic;
using System.Linq;

using juba.consoleapp.Out;


namespace juba.consoleapp.CmdLine
{
    public class Command: Item, ICmdLineCommand
    {
        private readonly Action myAction;
        public List<string> RequiredParams { get; private set; }

        public Command(IEnumerable<string> names, string description, Action todo)
            :base(names, description)
        {
            myAction = todo;
            RequiredParams = new List<string>();
        }

        public void Requires(params string[] parameters)
        {
            RequiredParams.AddRange(parameters.Where(newOne => !RequiredParams.Any(oldOne => oldOne.Equals(newOne, StringComparison.InvariantCultureIgnoreCase))));
        }

        public void Execute()
        {
            try
            {
                myAction();
            }
            catch (ExceptionBase exception) { Console.WriteLine(exception.Message); }
            catch (Exception exception) { Console.WriteLine(exception.ToString()); }
        }

        public override string Help(bool verbose)
        {
            return !verbose
                ? string.Format("[/{0}]", Names.First())
                : string.Format("/{0}: {1}", string.Join("|", Names), Description);
        }

    }
}
