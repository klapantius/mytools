using Microsoft.TeamFoundation.VersionControl.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tfsaccess
{
    public interface IChange
    {
        IItem Item { get; }
    }
    public class ChangeWrapper : IChange
    {
        private Change ch;
        private Change CH
        {
            get
            {
                if (ch == null) throw new TFSAccessException("Not initialized object of type Change.");
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
