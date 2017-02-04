using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

[assembly: InternalsVisibleTo("Tests")]
namespace Push
{
    public class VersionFileManager
    {
        public static readonly string EndOfPath = @"_Globals\VersionInformation";
        public static readonly string VersionFileFormatString = "Module_{0}_VerisonReference.xml";

        private static FileSystem.IDirectory directory;
        public static FileSystem.IDirectory Directory
        {
            get
            {
                if (directory == null)
                {
                    directory = new FileSystem.DirectoryWrapper();
                }
                return directory;
            }
            set { directory = value; }
        }

        private static FileSystem.IFile file;
        public static FileSystem.IFile File
        {
            get
            {
                if (file == null)
                {
                    file = new FileSystem.FileWrapper();
                }
                return file;
            }
            set { file = value; }
        }

        public string ErrorMessage { get; private set; }
        public bool IsOkay { get { return string.IsNullOrEmpty(ErrorMessage); } }

        public VersionFileManager(string path)
        {
            var dir = path.EndsWith(EndOfPath) ? path : Path.Combine(path, EndOfPath);
            if (!Directory.Exists(dir))
            {
                ErrorMessage = "Could not find directory " + dir;
                return;
            }
        }

        internal static string GetSCPFolder(string path, string scp)
        {
            var r = new Regex("\\" + scp + "[$|\\]", RegexOptions.CultureInvariant + RegexOptions.IgnoreCase);
            if (!r.IsMatch(path)) return string.Empty;
            var lastmatch = r.Matches(path).Count-1;
            
            var x = path.LastIndexOf(string.Format("{0}{1}{0}", Path.DirectorySeparatorChar, scp), StringComparison.InvariantCultureIgnoreCase);
            return x >= 0 ? path.Substring(x) : string.Empty;
        }

    }
}
