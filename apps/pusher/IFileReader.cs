namespace FileReader
{
    internal interface IFileReader
    {
        string FileName { get; }
        byte[] ReadLastNBytes(long n);
        byte[] ReadNewBytes(long max = -1);
        void ResetPosition();
        bool IsAtTheEof { get; }
    }
}
