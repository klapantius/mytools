﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace consoleapp.CmdLine
{
  public class Interpreter
  {
    private readonly List<Parameter> parameters = new List<Parameter>();
    public string ValueOf(string paramName)
    {
      var asked = parameters.FirstOrDefault(p => p.Matches(paramName));
      return asked != null ? asked.Value : string.Empty;
    }

    public string ValueOf(string paramName, Func<bool, string> validation)
    {
      var asked = parameters.FirstOrDefault(p => p.Matches(paramName));
      return asked != null ? asked.Value : string.Empty;
    }

    public static int DefaultIntConverter(string str)
    {
      var result = int.MinValue;
      int.TryParse(str, out result);
      return result;
    }

    public T Evaluate<T>(string paramName, Func<string, T> converter, params Action<T>[] validator)
    {
      var asked = parameters.FirstOrDefault(p => p.Matches(paramName));
      var result = converter(asked != null ? asked.Value : string.Empty);
      validator.ToList().ForEach(v => v(result));
      return result;
    }

    public bool IsSpecified(string paramName)
    {
      return !string.IsNullOrEmpty(ValueOf(paramName));
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
      const string sub = @"( [\/\-].*)";
      var pattern = @"[\/\-](.*)";
      while (new Regex(pattern + sub).IsMatch(commandLine)) pattern += sub;
      var groups = new Regex(pattern).Match(commandLine).Groups;
      var splits = new List<string>();
      for (var g = 1; g < groups.Count; ++g) splits.Add(groups[g].Value.Trim().Trim(ParamSeparators.ToCharArray()));
      var args = splits
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
      Errors.ForEach(Console.WriteLine);
      Console.WriteLine("\nusage:");
      Console.WriteLine("{0} {1}", prgName, string.Join(" ", parameters.Select(p => p.ToString())));
    }
  }

  public interface IParameter
  {
    string[] Names { get; }
    string Description { get; }
    bool IsMandatory { get; }
    string DefaultValue { get; }
    string Value { get; }
    bool Matches(Parameter other);
    bool Matches(string name);
    string ToString();
  }

  public class Parameter : IParameter
  {
    public const string InvalidValue = "invalid value 9a9f9v0adfg";
    public string[] Names { get; private set; }
    public string Description { get; private set; }
    public bool IsMandatory { get; private set; }
    public string DefaultValue { get; private set; }
    public string Value { get; internal set; }

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