using juba.tfs.interfaces;
using Microsoft.TeamFoundation.VersionControl.Client;

namespace juba.tfs.wrappers
{
    public class ChangeWrapper : IChange
    {
        private Change ch;
        private Change CH
        {
            get
            {
                if (ch == null) throw new TfsAccessException("Not initialized object of type Change.");
                return ch;
            }
            set { ch = value; }
        }
        public ChangeWrapper(Change change)
        {
            CH = change;
        }

        public IItem Item { get { return new ItemWrapper(CH.Item); } }

    }
}
