using System.Collections.Generic;


namespace juba.consoleapp.CmdLine
{
    public interface ICmdLineParameter : ICmdLineItem
    {
        string ShortValueDescription { get; }
        bool IsMandatory { get; }
        string DefaultValue { get; }
        string Value { get; set; }
        void BelongsTo(params string[] items);
        List<string> AffectedCommands { get; }
    }
}