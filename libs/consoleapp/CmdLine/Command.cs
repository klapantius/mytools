using System;
using System.Collections.Generic;

namespace juba.consoleapp.CmdLine
{
    public class Command: Item, ICmdLineCommand
    {
        private readonly Action myAction;

        public Command(IEnumerable<string> names, string description, Action todo)
            :base(names, description)
        {
            myAction = todo;
        }

        public void Execute()
        {
            try
            {
                myAction();
            }
            catch (ExceptionBase exception) { Out.Error(exception.Message); }
            catch (Exception exception) { Out.Error(exception.ToString()); }
        }
    }
}
