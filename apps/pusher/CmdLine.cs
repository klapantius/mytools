using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CmdLine
{
    public class Interpreter
    {
        private List<Parameter> parameters = new List<Parameter>();
        public string ValueOf(string paramName)
        {
            var asked = parameters.FirstOrDefault(p => p.Matches(paramName));
            return asked != null ? asked.Value : string.Empty;
        }
        public void Add(Parameter parameter)
        {
            if (parameters.Any(p => p.Matches(parameter)))
            {
                throw new Exception(string.Format("Parameter definition \"{0}\" is ambiguous.", parameter.Names.First()));
            }
            parameters.Add(parameter);
        }

        public bool Parse(string commandLine)
        {
            const string ParamSeparators = "/-";
            const string ValueSeparators = ":=";
            Errors.Clear();
            var x = commandLine.Split(ParamSeparators.ToArray());
            var args = commandLine
                .Split(ParamSeparators.ToArray())
                .Where(a => !string.IsNullOrEmpty(a.Trim()))
                .Select(a => a.Trim().Split(ValueSeparators.ToArray()))
                .ToDictionary(a => a[0].ToLower().Trim('/', '-'), a => a.Count() > 1 ? a[1] : "true");
            foreach (var a in args)
            {
                var parameter = parameters.FirstOrDefault(p => p.Matches(a.Key));
                if (parameter == null)
                {
                    Errors.Add(string.Format("Not supported parameter: \"{0}\"", a.Key));
                    continue;
                }
                parameter.Value = a.Value;
            }
            parameters
                .Where(p => p.IsMandatory && p.Value == Parameter.InvalidValue)
                .ToList()
                .ForEach(p => Errors.Add(string.Format("Missing mandatory parameter \"{0}\"", p.Names.First())));
            return !Errors.Any();
        }

        public List<string> Errors = new List<string>();


        public void PrintErrors(string prgName)
        {
            Errors.ForEach(e => Console.WriteLine(e));
            Console.WriteLine("\nusage:");
            Console.WriteLine("{0} {1}", prgName, string.Join(" ", parameters.Select(p => p.ToString())));
        }
    }

    public class Parameter
    {
        public const string InvalidValue = "invalid value 9a9f9v0adfg";
        public string[] Names { get; private set; }
        public string Description { get; private set; }
        public bool IsMandatory { get; private set; }
        public string DefaultValue { get; private set; }
        public string Value { get; set; }

        public Parameter(string[] names, string description, bool isMandatory, string defaultValue = "")
        {
            Names = names.Select(n => n.ToLower()).ToArray();
            Description = description;
            IsMandatory = isMandatory;
            DefaultValue = IsMandatory ? InvalidValue : defaultValue;
            Value = DefaultValue;
        }

        public bool Matches(Parameter other)
        {
            return this.Names.Any(n => other.Names.Any(o => o == n));
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