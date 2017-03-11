using System.Collections.Generic;

namespace sybi
{
    public interface ISourceControlProject
    {
        string Name { get; }
        string ScpType { get; }
        IEnumerable<IBranch> Branches { get; }
    }
    public class SourceControlProject: ISourceControlProject
    {
        public string Name { get; private set; }
        public string ScpType { get; private set; }

        public SourceControlProject(string scpType, string name)
        {
            Name = name;
        }
        public IEnumerable<IBranch> Branches
        {
            get { throw new System.NotImplementedException(); }
        }
    }
}
