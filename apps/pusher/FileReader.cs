using System;
using System.IO;
using System.Threading;

namespace FileReader
{
    internal class FileReader : IFileReader
    {
        public string FileName { get; private set; }
        public long Position { get; private set; }
        public bool IsAtTheEof { get; private set; }
        internal IFileStream myFileStream;
        internal IFile File { get; set; }

        public FileReader(string fileName)
        {
            FileName = fileName;
            Position = 0;
            IsAtTheEof = false;
            File = new TailFile();
        }

        public byte[] ReadLastNBytes(long n)
        {
            using (var fs = myFileStream ?? new TailStream(FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var numberOfBytesToDisplay = n < fs.Length ? n : fs.Length;

                byte[] buf = new byte[numberOfBytesToDisplay];

                fs.Seek(-numberOfBytesToDisplay, SeekOrigin.End);
                fs.Read(buf, 0, (int)numberOfBytesToDisplay);

                fs.Close();

                return buf;
            }
        }

        public byte[] ReadNewBytes(long max = -1)
        {
            // wait until file file is reset but max 5 seconds
            var secsLeft = 5;
            while (!File.Exists(FileName))
            {
                if (secsLeft-- < 0) return new byte[] { };
                Thread.Sleep(1000);
            }
            try
            {
                using (var fs = myFileStream ?? new TailStream(FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    // compare the size of the file to the last position
                    // and return an empty array if nothing has changed since the last read
                    if (fs.Length == Position) return new byte[] { };

                    long bytesToRead;
                    // decide the start position and the number of bytes to read:
                    // if no bytes read yet get the minimum of file size vs max enabled (if any)
                    if (Position == 0 || fs.Length < Position) bytesToRead = GetMinOrFirst(fs.Length, max);
                    // else get the minimum of difference between file size and last position vs max enabled (if any)
                    else bytesToRead = GetMinOrFirst(fs.Length - Position, max);

                    // reset file pointer if the file has been reset since the last read
                    // (assumed the file is still smaller than the last read position before reset / assumed it was at the end)
                    if (fs.Length < Position) Position = 0;

                    // read now
                    var buf = new byte[bytesToRead];
                    fs.Seek(Position, SeekOrigin.Begin);
                    fs.Read(buf, 0, (int)bytesToRead);

                    // notice the new position
                    Position = fs.Position;
                    IsAtTheEof = (fs.Position == fs.Length);

                    fs.Close();

                    return buf;
                }

            }
            catch (FileNotFoundException)
            {
                return new byte[] { };
            }
        }

        public void ResetPosition()
        {
            Position = 0;
        }

        /// <summary>
        /// selects the minimum of two values or the first one if the second one is lower than 1
        /// </summary>
        private static long GetMinOrFirst(long a, long b)
        {
            return Math.Min(a, b < 1 ? a : b);
        }
    }

}
