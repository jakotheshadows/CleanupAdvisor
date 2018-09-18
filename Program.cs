using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace CleanupAdvisor
{
    /// <summary>
    /// CleanupAdvisor
    /// Author: Anthony Wolf
    /// Date: 9-17-2018
    /// </summary>
    class Program
    {
        /// <summary>
        /// Key: Full path of the directory
        /// Value: Size of the directory in bytes
        /// </summary>
        private static Dictionary<string, long> directorySizes;

        private static readonly string[] SizeSuffixes = 
                   { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

        private static string root = @"C:\";

        private static string windows = @"C:\Windows";

        static void Main(string[] args)
        {
            const int MAX_LINES_WRITTEN_BEFORE_PAUSE = 25;
            directorySizes = new Dictionary<string, long>();
            long totalSize = DirSize(new DirectoryInfo(@"C:\"));
            var sorted = directorySizes.AsEnumerable().OrderByDescending(x => x.Value);
            int linesWritten = 0;
            foreach (KeyValuePair<string, long> kvp in sorted)
            {
                string key = kvp.Key;
                Console.WriteLine(SizeSuffix(directorySizes[key], 2) + " : " + key);
                linesWritten += 1;
                if (linesWritten == MAX_LINES_WRITTEN_BEFORE_PAUSE)
                {
                    linesWritten = 0;
                    Console.Write("Press Enter to continue...");
                    Console.ReadLine();
                }
            }
            Console.Write("Program Complete - Press Enter to exit...");
            Console.ReadLine();
        }
        
        private static string SizeSuffix(long value, int decimalPlaces = 1)
        {
            if (decimalPlaces < 0) { throw new ArgumentOutOfRangeException("decimalPlaces"); }
            if (value < 0) { return "-" + SizeSuffix(-value); } 
            if (value == 0) { return string.Format("{0:n" + decimalPlaces + "} bytes", 0); }

            // mag is 0 for bytes, 1 for KB, 2, for MB, etc.
            int mag = (int)Math.Log(value, 1024);

            // 1L << (mag * 10) == 2 ^ (10 * mag) 
            // [i.e. the number of bytes in the unit corresponding to mag]
            decimal adjustedSize = (decimal)value / (1L << (mag * 10));

            // make adjustment when the value is large enough that
            // it would round up to 1000 or more
            if (Math.Round(adjustedSize, decimalPlaces) >= 1000)
            {
                mag += 1;
                adjustedSize /= 1024;
            }

            return string.Format("{0:n" + decimalPlaces + "} {1}", 
                adjustedSize, 
                SizeSuffixes[mag]);
        }

        private static long DirSize(DirectoryInfo d) 
        {
            long size = 0;
            FileInfo[] fis;
            bool isSystem = (d.Attributes | FileAttributes.System) == d.Attributes;
            bool isRoot = d.FullName == root;
            try
            {
                fis = d.GetFiles();
            }
            catch
            {
                return -1;
            }
            bool containsNonSystem = fis.Any(x => (x.Attributes | FileAttributes.System) != x.Attributes);
            bool isWindows = d.FullName == windows;
            // Add file sizes.
            foreach (FileInfo fi in fis) 
            {      
                size += fi.Length;
            }
            // Add subdirectory sizes.
            DirectoryInfo[] dis = d.GetDirectories();
            
            foreach(DirectoryInfo di in dis)
            {
                if ((isSystem && !isRoot && !containsNonSystem) || isWindows)
                {
                    continue;
                }
                long currentDirectorySize = DirSize(di);
                size += currentDirectorySize;
                if (size >= 0)
                {
                    directorySizes.Add(di.FullName, currentDirectorySize);
                }
            }
            
            return size;  
        }
    }
}
