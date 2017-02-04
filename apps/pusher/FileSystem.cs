using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace FileSystem
{
    public interface IFile
    {
        bool Exists(string path);
    }

    public class FileWrapper : IFile
    {
        public bool Exists(string path) { return File.Exists(path); }
    }

    public interface IDirectory
    {
        bool Exists(string path);
    }

    public class DirectoryWrapper : IDirectory
    {
        public bool Exists(string path) { return Directory.Exists(path); }
    }

}
