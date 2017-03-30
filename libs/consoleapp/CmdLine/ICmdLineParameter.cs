namespace juba.consoleapp.CmdLine
{
    public interface ICmdLineParameter : ICmdLineItem
    {
        bool IsMandatory { get; }
        string DefaultValue { get; }
        string Value { get; set; }
    }
}