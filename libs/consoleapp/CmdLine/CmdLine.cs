using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace juba.consoleapp.CmdLine
{
    // todo: make "anykey" to an automatic parameter
    // todo: automatic paramters should not be displayed in the help(?)
    // todo: display optional parameters with square brackets
    public class Interpreter : ICmdLineInterpreter
    {
        private readonly List<ICmdLineCommand> myCommands = new List<ICmdLineCommand>();
        private readonly List<ICmdLineParameter> myParameters = new List<ICmdLineParameter>();
        private List<ICmdLineCommand> SpecifiedCommands { get; set; }

        public string ValueOf(string paramName)
        {
            var asked = myParameters.FirstOrDefault(p => p.Matches(paramName));
            return asked != null && asked.Value != Parameter.InvalidValue ? asked.Value : string.Empty;
        }

        public string ValueOf(string paramName, Func<bool, string> validation)
        {
            var asked = myParameters.FirstOrDefault(p => p.Matches(paramName));
            return asked != null ? asked.Value : string.Empty;
        }

        public static int DefaultIntConverter(string str)
        {
            var result = int.MinValue;
            int.TryParse(str, out result);
            return result;
        }

        public static bool DefaultBoolConverter(string str)
        {
            var result = false;
            bool.TryParse(str, out result);
            return result;
        }

        public T Evaluate<T>(string paramName, Func<string, T> converter, params Action<T>[] validator)
        {
            var asked = myParameters.FirstOrDefault(p => p.Matches(paramName));
            var result = converter(asked != null ? asked.Value : string.Empty);
            validator.ToList().ForEach(v => v(result));
            return result;
        }

        public bool IsSpecified(string itemName)
        {
            return !string.IsNullOrEmpty(ValueOf(itemName));
        }

        public ICmdLineParameter Add(ICmdLineParameter parameter)
        {
            if (myParameters.Any(p => p.Matches(parameter)))
            {
                throw new Exception(string.Format("Parameter definition \"{0}\" is ambiguous.", parameter.Names.First()));
            }
            myParameters.Add(parameter);
            return parameter;
        }

        public ICmdLineCommand Add(ICmdLineCommand command, params string[] requiredParameters)
        {
            if (myCommands.Any(p => p.Matches(command)))
            {
                throw new Exception(string.Format("Parameter definition \"{0}\" is ambiguous.", command.Names.First()));
            }
            if (requiredParameters != null && requiredParameters.Length > 0) command.Requires(requiredParameters);
            myCommands.Add(command);
            return command;
        }

        public bool Parse(params string[] inArgs)
        {
            const string ItemSeparators = "/-";
            const string ValueSeparators = ":=";
            Errors.Clear();
            const string sub = @"( [\/\-].*)";
            var pattern = @"[\/\-](.*)";
            var commandLine = inArgs != null ? string.Join(" ", inArgs) : string.Empty;
            while (new Regex(pattern + sub).IsMatch(commandLine)) pattern += sub;
            var groups = new Regex(pattern).Match(commandLine).Groups;
            var splits = new List<string>();
            for (var g = 1; g < groups.Count; ++g) splits.Add(groups[g].Value.Trim().Trim(ItemSeparators.ToCharArray()));
            var args = splits
                .Where(a => !string.IsNullOrEmpty(a.Trim()))
                .Select(a => a.Trim().Split(ValueSeparators.ToArray()))
                .ToDictionary(a => a[0].ToLower().Trim('/', '-'), a => a.Count() > 1 ? string.Join(":", a.Skip(1)) : "true");
            SpecifiedCommands = new List<ICmdLineCommand>();
            foreach (var a in args)
            {
                var item = myParameters.FirstOrDefault(p => p.Matches(a.Key));
                if (item != null)
                {
                    item.Value = a.Value;
                    continue;
                }
                if (myCommands.Any(c => c.Matches(a.Key))) SpecifiedCommands.Add(myCommands.First(c => c.Matches(a.Key)));
                else Errors.Add(string.Format("Not supported command line argument: \"{0}\"", a.Key));
            }
            if (!SpecifiedCommands.Any() && myCommands.Count > 1) Errors.Add("Please specify a command to be executed.");
            if (!SpecifiedCommands.Any() && myCommands.Count == 1) SpecifiedCommands.Add(myCommands.First());
            SpecifiedCommands
                .Where(c => c.RequiredParams.Any(r => !IsSpecified(r)))
                .ToList()
                .ForEach(c => c.RequiredParams
                    .Where(r => !IsSpecified(r))
                    .ToList()
                    .ForEach(r => Errors.Add(string.Format("Command \"{0}\" requires parameter \"{1}\".", c.Names.First(), r))));
            myParameters
                .Where(p => p.IsMandatory && p.Value == Parameter.InvalidValue)
                .ToList()
                .ForEach(p => Errors.Add(string.Format("Missing mandatory parameter \"{0}\"", p.Names.First())));
            return !Errors.Any();
        }

        public void ExecuteCommands()
        {
            if (myCommands.Count == 1)
            {
                myCommands.First().Execute();
                return;
            }
            SpecifiedCommands.ForEach(c => c.Execute());
        }

        public List<string> Errors = new List<string>();

        public void PrintErrors(string prgName)
        {
            Errors.ForEach(Console.WriteLine);
            PrintUsage(prgName);
        }

        //todo: make PrintUsage to an automatically added command
        public void PrintUsage(string prgName)
        {
            Console.WriteLine("\nusage:");
            if (myCommands != null && myCommands.Count > 1) 
                Console.WriteLine("{0} {1}", prgName, string.Join(" ", myCommands.Union(myParameters.Cast<ICmdLineItem>()).Select(p => p.Help(false))));
            else
                Console.WriteLine("{0} {1}", prgName, string.Join(" ", myParameters.Select(p => p.Help(false))));

            if (myCommands.Count > 1)
            {
                foreach (var c in myCommands)
                {
                    Console.WriteLine("\t{0}", c.Help(true));
                    if (!c.RequiredParams.Any()) continue;
                    PrintParamsOf(c);
                }
                Console.WriteLine("Global parameters:");
                foreach (var p in myParameters.Where(myp => myp.AffectedCommands.Count==0 && !myCommands.Any(c => c.RequiredParams.Any(myp.Matches))))
                {
                    Console.WriteLine("\t{0}", p.Help(true));
                }
            }
            else
            {
                PrintParamsOf(null);
            }
        }

        private void PrintParamsOf(ICmdLineCommand cmd)
        {
            foreach (var pname in cmd != null ? cmd.RequiredParams : myParameters.Select(p => p.Names.First()))
            {
                var par = myParameters.FirstOrDefault(p => p.Matches(pname));
                if (par == null) continue;
                Console.WriteLine("\t\t{0}", par.Help(true));
            }
            foreach (var par in myParameters.Where(p => p.AffectedCommands.Any(cmd.Matches) && cmd.RequiredParams.All(rp => !ParameterByName(rp).Matches(p))))
            {
                Console.WriteLine("\t\t[{0}]", par.Help(true));
            }
        }

        private ICmdLineParameter ParameterByName(string name)
        {
            return myParameters.FirstOrDefault(p => p.Matches(name));
        }

    }
}