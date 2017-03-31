using System;
using System.Collections.Generic;
using System.Linq;

namespace juba.consoleapp.CmdLine
{
    public class Item : ICmdLineItem
    {
        public string[] Names { get; private set; }
        public string Description { get; private set; }
        private readonly List<string> myCoParams = new List<string>();
        public List<string> RequiredParams { get { return myCoParams; } }

        public Item(IEnumerable<string> names, string description)
        {
            Names = names.Select(n => n.ToLower()).ToArray();
            Description = description;
        }

        public void Requires(params string[] coparams)
        {
            myCoParams.AddRange(coparams.Where(newOne => !myCoParams.Any(myOne => myOne.Equals(newOne, StringComparison.InvariantCultureIgnoreCase))));
        }

        public bool Matches(ICmdLineItem other)
        {
            return Names.Any(n => other.Names.Any(o => o == n));
        }
        public bool Matches(string name)
        {
            return Names.Any(n => n == name.ToLower());
        }
   }

    public class Parameter : Item, ICmdLineParameter
    {
        public const string InvalidValue = "invalid value 9a9f9v0adfg";
        public bool IsMandatory { get; private set; }
        public string DefaultValue { get; private set; }
        public string Value { get; set; }

        public Parameter(IEnumerable<string> names, string description, bool isMandatory, string defaultValue = InvalidValue)
            :base(names, description)
        {
            IsMandatory = isMandatory;
            DefaultValue = IsMandatory ? InvalidValue : defaultValue;
            Value = DefaultValue;
        }

        public override string ToString()
        {
            var result = (IsMandatory ? "[" : "") + (Names.Count() > 1 ? "<" : "") + string.Join("|", Names) + (Names.Count() > 1 ? ">" : "");
            result += ":<string>";
            result += IsMandatory ? "]" : "";
            return result;
        }
    }
}