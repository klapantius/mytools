﻿using juba.tfs.interfaces;
using System;
using System.Collections.Generic;

namespace sybi
{
    public interface IBranch
    {
        IEnumerable<IVersionInfo> LatestVersionInfo { get; }
    }
    public class Branch : IBranch
    {
        private string myBranchPath;
        private string myScpPath;
        public string Error { get; internal set; }
        public Branch(string scpPath, string branchPath)
        {
            myScpPath = scpPath;
            myBranchPath = branchPath;
        }

        public IEnumerable<IVersionInfo> LatestVersionInfo
        {
            get
            {
                //var versionInfoItems = juba.tfs.wrappers.VCS.GetItems(Path.Combine(scpPath, branchPath, juba.tfs.wrappers.Configuration.VersionInfoPath));
                throw new NotImplementedException();
            }
        }
    }
}
