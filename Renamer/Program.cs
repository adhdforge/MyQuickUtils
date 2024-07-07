using System.Collections.Generic;

namespace Renamer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string path = args[0];
            string from = args[1];
            string to = args[2];

            List<string> dirs = new List<string>();
            dirs.Add(path);
            
            foreach (var dir in dirs)
            {
                string[] files = System.IO.Directory.GetFiles(path, $"*{from}*");
                RenameFiles(files, from, to);
            }
            RenameDirs(dirs, from, to);
        }

        static void GetDirs(string path, ref List<string> dirs)
        {
            string[] subDirs = System.IO.Directory.GetDirectories(path);
            dirs.AddRange(subDirs);
            foreach (string dir in subDirs)
            {
                GetDirs(dir, ref dirs);
            }
        }

        static void RenameFiles(string[] files, string from, string to)
        {
            foreach (string file in files)
            {
                string name = System.IO.Path.GetFileName(file).Replace(from, to);
                string dir = System.IO.Path.GetDirectoryName(file);
                System.IO.File.Move(file, System.IO.Path.Combine(dir, name));
            }
        }

        static void RenameDirs(List<string> dirs, string from, string to)
        {
            dirs.Reverse();
            foreach (string dir in dirs)
            {

                string name = System.IO.Path.GetFileName(dir);
                if (!name.Contains(from)) continue;
                name = name.Replace(from, to);
                string dir2 = System.IO.Path.GetDirectoryName(dir);
                System.IO.Directory.Move(dir, System.IO.Path.Combine(dir2, name));
            }
        }
    }
}
