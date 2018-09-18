using System.IO;

namespace CleanupAdvisor
{
    public class DirectoryModel
    {
        public FileInfo[] Files;
        public DirectoryInfo[] Directories;
        public bool IsSystem;
        public bool IsWindows;
        public bool ContainsNonSystem;
        public bool IsRoot;
    }
}
