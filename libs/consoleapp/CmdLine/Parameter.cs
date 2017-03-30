using System;
using System.Collections.Generic;
using System.Linq;

namespace juba.consoleapp.CmdLine
{
    public class Parameter : ICmdLineParameter
    {
        public const string InvalidValue = "invalid value 9a9f9v0adfg";
        public string[] Names { get; private set; }
        public string Description { get; private set; }
        public bool IsMandatory { get; private set; }
        public string DefaultValue { get; private set; }
        public string Value { get; internal set; }
        private readonly List<string> myCoParams = new List<string>();
        public List<string> CoParams { get { return myCoParams; } }

        public Parameter(IEnumerable<string> names, string description, bool isMandatory, string defaultValue = InvalidValue)
        {
            Names = names.Select(n => n.ToLower()).ToArray();
            Description = description;
            IsMandatory = isMandatory;
            DefaultValue = IsMandatory ? InvalidValue : defaultValue;
            Value = DefaultValue;
        }

        public void Requires(params string[] coparams)
        {
            myCoParams.AddRange(coparams.Where(newOne => !myCoParams.Any(myOne => myOne.Equals(newOne, StringComparison.InvariantCultureIgnoreCase))));
        }

        public bool Matches(Parameter other)
        {
            return Names.Any(n => other.Names.Any(o => o == n));
        }
        public bool Matches(string name)
        {
            return Names.Any(n => n == name.ToLower());
        }

        public override string ToString()
        {
            var result = IsMandatory ? "[" : "" + "<" + string.Join("|", Names) + ">";
            result += ":<string>";
            result += IsMandatory ? "]" : "";
            return result;
        }
    }
}