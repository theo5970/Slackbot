
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace Slackbot
{
    public class FileMonitor
    {
        private static List<string> block_extensions = new List<string> { "png", "jpg", "txt", "pdf", "exe", "com", "bat", "bin", "jpeg" };
        private static FileSystemWatcher watcher = new FileSystemWatcher();
        public static void Start()
        {
            watcher.Path = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + @"\";
            watcher.IncludeSubdirectories = true;
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            watcher.Filter = "*.*";
            watcher.Created += Watcher_Created;
            watcher.EnableRaisingEvents = true;
        }

        private static void Watcher_Created(object sender, FileSystemEventArgs e)
        {
            string fullpath = e.FullPath;
            string extension = Path.GetExtension(fullpath).ToLower();
            if (block_extensions.Any(extension.Contains))
            {
                File.Delete(fullpath);
                Console.WriteLine("File deleted: {0}", fullpath);
            }
        }
    }
}
