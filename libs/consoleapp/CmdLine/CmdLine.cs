using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace juba.consoleapp.CmdLine
{
    public class Interpreter : ICmdLineInterpreter
    {
        private readonly List<ICmdLineCommand> myCommands = new List<ICmdLineCommand>();
        private readonly List<ICmdLineParameter> myParameters = new List<ICmdLineParameter>();
        private List<ICmdLineItem> AllKnownItems { get { return myCommands.Cast<ICmdLineItem>().Union(myParameters).ToList(); } }
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

        public ICmdLineCommand Add(ICmdLineCommand comamnd)
        {
            if (myCommands.Any(p => p.Matches(comamnd)))
            {
                throw new Exception(string.Format("Parameter definition \"{0}\" is ambiguous.", comamnd.Names.First()));
            }
            myCommands.Add(comamnd);
            return comamnd;
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
                .ToDictionary(a => a[0].ToLower().Trim('/', '-'), a => a.Count() > 1 ? a[1] : "true");
            foreach (var a in args)
            {
                if (!AllKnownItems.Any(p => p.Matches(a.Key)))
                {
                    Errors.Add(string.Format("Not supported entity: \"{0}\"", a.Key));
                }
            }
            SpecifiedCommands = new List<ICmdLineCommand>();
            foreach (var a in args)
            {
                var item = myParameters.FirstOrDefault(p => p.Matches(a.Key));
                if (item != null)
                {
                    item.Value = a.Value;
                    continue;
                }
                SpecifiedCommands.Add(myCommands.FirstOrDefault(c => c.Matches(a.Key)));
            }
            myCommands
                .Where(c => c.RequiredParams.Any(r => ValueOf(r) == Parameter.InvalidValue))
                .ToList()
                .ForEach(c => c.RequiredParams
                    .Where(r => ValueOf(r) == Parameter.InvalidValue)
                    .ToList()
                    .ForEach(r => Errors.Add(string.Format("Command \"{0}\" requires parameter \"{1}\".", c.Names.First(), c))));
            myParameters
                .Where(p => p.IsMandatory && p.Value == Parameter.InvalidValue)
                .ToList()
                .ForEach(p => Errors.Add(string.Format("Missing mandatory parameter \"{0}\"", p.Names.First())));
            myParameters
              .Where(p => p.RequiredParams.Any(c => ValueOf(c) == Parameter.InvalidValue))
              .ToList()
              .ForEach(p => p.RequiredParams
                .Where(c => ValueOf(c) == Parameter.InvalidValue)
                .ToList()
                .ForEach(c => Errors.Add(string.Format("Parameter \"{0}\" requires parameter \"{1}\".", p.Names.First(), c))));
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
            Console.WriteLine("\nusage:");
            Console.WriteLine("{0} {1}", prgName, string.Join(" ", myParameters.Select(p => p.ToString())));
        }

    }
}