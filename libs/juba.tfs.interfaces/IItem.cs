using System;
using System.IO;

namespace juba.tfs.interfaces
{
    public interface IItem
    {
        string ServerItem { get; }
        bool IsBranch { get; }
        int ChangesetId { get; }
        DateTime CheckinDate { get; }
        Stream DownloadFile();
    }
}
