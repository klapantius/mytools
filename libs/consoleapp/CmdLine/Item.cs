using System;
using System.Collections.Generic;
using System.Linq;


namespace juba.consoleapp.CmdLine
{
    public class Item : ICmdLineItem
    {
        public string[] Names { get; private set; }
        public string Description { get; private set; }

        public Item(IEnumerable<string> names, string description)
        {
            Names = names.Select(n => n.ToLower()).ToArray();
            Description = description;
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
}