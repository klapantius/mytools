using System;
using System.Collections.Generic;
using System.Linq;

namespace juba.consoleapp.CmdLine
{
    public class Parameter : Item, ICmdLineParameter
    {
        public const string InvalidValue = "invalid value 9a9f9v0adfg";
        public string ShortValueDescription { get; private set; }
        public bool IsMandatory { get; private set; }
        public string DefaultValue { get; private set; }
        public string Value { get; set; }
        public List<string> AffectedCommands { get; private set; }

        public Parameter(IEnumerable<string> names, string shortValueDescription, string description, bool isMandatory, string defaultValue = InvalidValue)
            : base(names, description)
        {
            ShortValueDescription = shortValueDescription;
            IsMandatory = isMandatory;
            DefaultValue = IsMandatory ? InvalidValue : defaultValue;
            Value = DefaultValue;
            AffectedCommands=new List<string>();
        }

        public void BelongsTo(params string[] commands)
        {
            AffectedCommands.AddRange(commands.Where(newOne => !AffectedCommands.Any(oldOne => oldOne.Equals(newOne, StringComparison.InvariantCultureIgnoreCase))));
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