using System;
using System.Collections.Generic;
using System.Linq;

namespace juba.consoleapp.CmdLine
{
    public class Item : ICmdLineItem
    {
        public string[] Names { get; private set; }
        public string Description { get; private set; }
        public List<string> RequiredParams { get; private set; }

        public Item(IEnumerable<string> names, string description)
        {
            Names = names.Select(n => n.ToLower()).ToArray();
            Description = description;
            RequiredParams = new List<string>();
        }

        public void Requires(params string[] coparams)
        {
            RequiredParams.AddRange(coparams.Where(newOne => !RequiredParams.Any(oldOne => oldOne.Equals(newOne, StringComparison.InvariantCultureIgnoreCase))));
        }

        public bool Matches(ICmdLineItem other)
        {
            return Names.Any(n => other.Names.Any(o => o == n));
        }
        public bool Matches(string name)
        {
            return Names.Any(n => n == name.ToLower());
        }

        public virtual string Help(bool multiLine)
        {
            throw new NotImplementedException();
        }
    }

    public class Parameter : Item, ICmdLineParameter
    {
        public const string InvalidValue = "invalid value 9a9f9v0adfg";
        public string ShortValueDescription { get; private set; }
        public bool IsMandatory { get; private set; }
        public string DefaultValue { get; private set; }
        public string Value { get; set; }

        public Parameter(IEnumerable<string> names, string shortValueDescription, string description, bool isMandatory, string defaultValue = InvalidValue)
            : base(names, description)
        {
            ShortValueDescription = shortValueDescription;
            IsMandatory = isMandatory;
            DefaultValue = IsMandatory ? InvalidValue : defaultValue;
            Value = DefaultValue;
        }

        public override string ToString()
        {
            return Help(false);
        }

        public override string Help(bool verbose)
        {
            if (!verbose)
            {
                return string.Format("/{0}:<{1}>", Names.First(), ShortValueDescription);
            }
            return string.Format("/{0}:<{1}>{3} - {2}",
                Names.First(),
                ShortValueDescription,
                Description,
                DefaultValue.Equals(InvalidValue)
                    ? " (mandatory)"
                    : !string.IsNullOrEmpty(DefaultValue)
                        ? string.Format(" (default:{0})", DefaultValue)
                        : "");
        }

    }
}