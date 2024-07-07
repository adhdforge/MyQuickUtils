using System;
using System.Collections.Generic;
using System.Text;

namespace Percent20toSpace {
	class Program {
		static void Main(string[] args) {
			// argument checks: TSVN will add 5 args to the call: PATH DEPTH REVISION ERROR CWD
			// we will only work with the 5th one which is the path where the svn update has occured
			if (args.Length != 5) {
				Console.Error.WriteLine("Incorect number of arguments. Five expected (PATH DEPTH REVISION ERROR CWD).");
				Environment.ExitCode = -66;
				return;
			}
			
			string path = args[4];
			// check that we've got a valid directory path
			if (!System.IO.Directory.Exists(path)) {
				Console.Error.WriteLine("Invalid argument (CWD).");
				Environment.ExitCode = -67;
				return;
			}
			
			// list of directories containing "%20";
			// using a LIFO stack so we can work from the bottom up
			Stack<string> dirs = new Stack<string>();
			// get the directories
			GetDirs(path, ref dirs);
			// start doing the magic
			while (dirs.Count > 0) {
				string from = dirs.Pop();
				string to = from.Replace("%20", " ");
				try {
					// check for existing renamed svn:external directory;
					// we can't just ignore, because we always want the latest source from the external
					if (System.IO.Directory.Exists(to)) {
						// the files in the _svn/.svn directories are ReadOnly, 
						// so we have to set them to Normal before attempting a delete.
						PrepDelete(to);
						// delete the old external
						System.IO.Directory.Delete(to, true);
					}
					// rename the "%20" directory
					System.IO.Directory.Move(from, to);
				} catch {
					// exceptions are never displayed.
				}
			}
		}
		
		/// <summary>
		/// Recursively get the directory list.
		/// </summary>
		/// <param name="path">Directory to start at</param>
		/// <param name="dirs">List of directories</param>
		static void GetDirs(string path, ref Stack<string> dirs) {
			string[] dirList = System.IO.Directory.GetDirectories(path, "*%20*");
			// add the current found directories
			foreach (string dir in dirList) {
				dirs.Push(dir);
			}
			// look for more
			foreach (string dir in dirList) {
				GetDirs(dir, ref dirs);
			}
		}
		
		/// <summary>
		/// Recursively set ReadOnly files to Normal
		/// </summary>
		/// <param name="path">Directory to start at</param>
		static void PrepDelete(string path) {
			foreach (string file in System.IO.Directory.GetFiles(path)) {
				if ((System.IO.File.GetAttributes(file) & System.IO.FileAttributes.ReadOnly) == System.IO.FileAttributes.ReadOnly)
					System.IO.File.SetAttributes(file, System.IO.FileAttributes.Normal);
			}
			foreach (string dir in System.IO.Directory.GetDirectories(path)) {
				PrepDelete(dir);
			}
		}
	}
}
