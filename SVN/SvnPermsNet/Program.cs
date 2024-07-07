// Disclaimer and Copyright Information
// SvnPerms : 
//
// All rights reserved.
//
// Written by Riaan Lehmkuhl
// Version 1.0
//
// Distribute freely, except: don't remove my name from the source or
// documentation (don't take credit for my work), mark your changes (don't
// get me blamed for your possible bugs), don't alter or remove this
// notice.
// No warrantee of any kind, express or implied, is included with this
// software; use at your own risk, responsibility for damages (if any) to
// anyone resulting from the use of this software rests entirely with the
// user.
//
// Send bug reports, bug fixes, enhancements, requests, flames, etc. to
// riaanl@gmail.com.com

using System;
using System.Collections.Generic;
using System.Text;

namespace SvnPerms {
	/// <summary>
	/// Holds info about a change in a Svn commit.
	/// </summary>
	class SvnChange {
		private string _action = string.Empty;
		private string _path = string.Empty;
		private bool _allowed = false;
		
		/// <summary>
		/// SvnChange ctor.
		/// </summary>
		/// <param name="action">"update" / "remove" / "add".</param>
		/// <param name="path">path of change in repos.</param>
		public SvnChange(string action, string path) {
			_action = null == action ? string.Empty : action.Trim();
			_path = null == path ? string.Empty : path.Trim();
		}
		
		/// <summary>
		/// Action taken ("update" / "remove" / "add").
		/// </summary>
		public string Action {
			get { return _action; }
			set { _action = null == value ? string.Empty : value.Trim(); }
		}
		
		/// <summary>
		/// Path of change in repos.
		/// </summary>
		public string Path {
			get { return _path; }
			set { _path = null == value ? string.Empty : value.Trim(); }
		}
		
		/// <summary>
		/// True if change allowed; False if not.
		/// </summary>
		public bool Allowed {
			get { return _allowed; }
			set { _allowed = value; }
		}
	}
	
	/// <summary>
	/// Main SvnPerms Application.
	/// </summary>
	class Program {
		private static string _stdOut = string.Empty;
		private static string _stdErr = string.Empty;

#if DEBUG
		private static string _switch = "-r";
#else
		private static string _switch = "-t";
#endif

		private static string _svnDir = string.Empty;
		private static string _txnRev = string.Empty;
		private static string _repos = string.Empty;
		
		/// <summary>
		/// Print SvnPerms usage to stderr;
		/// </summary>
		static void PrintUsage() {
			string usage = "USAGE: SvnPerms TXN|REV REPOS [ CONF ] [ -r ] \n      TXN     Query transaction TXN for commit information.\n      REPOS   Use repository at REPOS to check transactions.\n      CONF    Config file path; \n              defaults to 'svnperms.xml' in application directory.\n\n      -r      Debug mode: \n              Uses revision number instead of commit transaction nubmer.\nAuthor: Riaan Lehmkuhl 2006.\nReport bugs to <riaanl@gmail.com>.";
			Console.Error.Write(usage);
			System.Environment.ExitCode = 65;
		}
		
		/// <summary>
		/// Print log error to stderr;
		/// </summary>
		static void PrintLogError(string msg) {
			//string msg = "You failed to supply a valid log message. You shall cease and desist from similar errors of omission in the future.";
			Console.Error.Write(msg);
			System.Environment.ExitCode = 64;
		}
		
		/// <summary>
		/// Print misc errors to stderr;
		/// </summary>
		static void PrintError(string error) {
			Console.Error.Write(error);
			System.Environment.ExitCode = 1;
		}
		
		/// <summary>
		/// Main entry point of the application.
		/// </summary>
		/// <param name="args">TXN REPOS [ CONF ]</param>
		static void Main(string[] args) {
			// check arguments
			if ((null == args) || !(2 <= args.Length)) {
				PrintUsage();
				goto DebugLabel;
			}
			
			// get commit transaction.
			_txnRev = args[0];
			// get repos path.
			_repos = args[1];
			
			// sanity check supplied arguments.
			if (string.IsNullOrEmpty(_txnRev) || string.IsNullOrEmpty(_repos)) {
				PrintUsage();
				goto DebugLabel;
			}
			
			string permsFile = string.Empty;
			
			// check if config file path was supplied and if to use bebug mode.
			if (3 == args.Length) {
				if (!string.IsNullOrEmpty(args[2]) && args[2].Trim().ToLower() == "-r") {
					_switch = "-r";
				} else {
					permsFile = args[2];
				}
			} else if (4 == args.Length) {
				permsFile = args[2];
				if (!string.IsNullOrEmpty(args[3]) && args[3].Trim().ToLower() == "-r") {
					_switch = "-r";
				}
			}
			
			// if no config file path was supplied, use default.
			if (string.IsNullOrEmpty(permsFile)) {
				permsFile = System.Reflection.Assembly.GetExecutingAssembly().Location;
				permsFile = System.IO.Path.GetDirectoryName(permsFile) + "\\svnperms.xml";
			}

			// look for config file.
			if (!System.IO.File.Exists(permsFile)) {
				PrintError("PermsFile does not exist.\n" + permsFile);
				goto DebugLabel;
			}
			
			System.Xml.XmlDocument permsConf = new System.Xml.XmlDocument();

			try {
				// load config file.
				permsConf.Load(permsFile);
			} catch (Exception ex) {
				PrintError("Failed to load PermsFile:\n" + ex.Message);
				goto DebugLabel;
			}
			
			// where are the svn binaries.
			System.Xml.XmlNode tempNode = permsConf.SelectSingleNode("//settings/setting[@name='SvnDir']");
			if (null == tempNode) {
				PrintError("Required setting not found: SvnDir");
				goto DebugLabel;
			}
			_svnDir = tempNode.InnerText.Trim();
			if (string.Empty == _svnDir) {
				PrintError("Required setting not found: SvnDir");
				goto DebugLabel;
			}
			if (!_svnDir.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString())) {
				_svnDir = string.Concat(_svnDir, System.IO.Path.DirectorySeparatorChar.ToString());
			}
			if (!System.IO.Directory.Exists(_svnDir)) {
				PrintError("SvnDir not found: \"" + _svnDir + "\"");
				goto DebugLabel;
			}
			
			// read the commit log message.
			string logMsg = SvnCmd("log");
			if (null == logMsg) {
				PrintError(_stdErr);
				goto DebugLabel;
			}

			string errMsgSuffix = string.Empty;
			tempNode = permsConf.SelectSingleNode("//settings/setting[@name='ErrMsgSuffix']");
			if (null != tempNode) {
				errMsgSuffix = tempNode.InnerText.Trim();
			}

			if (!string.IsNullOrEmpty(errMsgSuffix))
				errMsgSuffix = "\n" + errMsgSuffix;

			System.Text.RegularExpressions.Regex regex = null;
			
			// get log message validation regex.
			string logRegex = null;
			tempNode = permsConf.SelectSingleNode("//settings/setting[@name='LogRegex']");
			if (null != tempNode) {
				logRegex = tempNode.InnerText.Trim();
			}

			string logRegexMsg = null;
			tempNode = permsConf.SelectSingleNode("//settings/setting[@name='LogRegexMsg']");
			if (null != tempNode) {
				logRegexMsg = tempNode.InnerText.Trim();
			}
			
			if (string.IsNullOrEmpty(logRegexMsg))
				logRegexMsg = "You failed to supply a valid log message. You shall cease and desist from similar errors of omission in the future.";
			
			if (!string.IsNullOrEmpty(logRegex)) {
				regex = new System.Text.RegularExpressions.Regex(
					logRegex,
					System.Text.RegularExpressions.RegexOptions.Multiline
					| System.Text.RegularExpressions.RegexOptions.IgnorePatternWhitespace
					| System.Text.RegularExpressions.RegexOptions.Compiled);

				if (!regex.IsMatch(logMsg)) {
					PrintLogError(logRegexMsg);
					goto DebugLabel;
				}
			}

			// read the commit author.
			string author = SvnCmd("author");
			if (null == author) {
				PrintError(_stdErr);
				goto DebugLabel;
			}
			// get rid of any extra whitespace (Marcus Maday)
			author = author.Trim();

			// read the commit changes.
			string changed = SvnCmd("changed");
			if (null == changed) {
				PrintError(_stdErr);
				goto DebugLabel;
			}
			
			// get the repo name.
			string sectionName = System.IO.Path.GetFileName(_repos.TrimEnd(new char[] { '\\' }));
			
			// look for the repo in default section if any.
			System.Xml.XmlNode sectionNode = permsConf.SelectSingleNode("//sections/default[@repos]");

			if (null != sectionNode) {
				regex = new System.Text.RegularExpressions.Regex("\\b" + sectionName + "\\b");
				if (!regex.IsMatch(sectionNode.Attributes["repos"].Value)) {
					sectionNode = null;
				}
			}
			
			// repo not in default section, look for it in it's own section.
			if (null == sectionNode) {
				sectionNode = permsConf.SelectSingleNode("//sections/" + sectionName);
			}
			
			// have we found the repo's section?.
			if (null == sectionNode) {
				PrintError("Section '" + sectionName + "' not found in " + permsFile);
				goto DebugLabel;
			}

			// Collection of changes in the commit.
			List<SvnChange> changes = new List<SvnChange>();
			
			// read the commit changes.
			foreach (string c in changed.Split('\n')) {
				string l = c.Trim();
				if (string.IsNullOrEmpty(l)) continue;
				string a = string.Empty;
				string p = string.Empty;
				if (!l.StartsWith("_")) {
					a = l.Substring(0, 1);
					p = l.Substring(1);
				} else {
					a = l.Substring(1, 1);
					p = l.Substring(2);
				}
				changes.Add(new SvnChange(a, p));
			}

			// check permissions for each change commited.
			foreach (SvnChange sc in changes) {
				
				string err = string.Empty;
				
				// go through locations in repo to check permissions on.
				foreach (System.Xml.XmlNode location in sectionNode.SelectNodes("location")) {
					// the path in the repos regex.
					string locPath = location.Attributes["path"].Value.Trim();
					// allowed actions.
					string locActions = location.Attributes["actions"].Value.Trim();
					System.Text.StringBuilder users = new System.Text.StringBuilder();
					
					if (location.InnerText.Trim() == "*") { // if set to all users then just use current author.
						users.Append(author);
					} else { // get a list of allowed authors
						// group names are prefixed with a @, 
						// so expand them with the users in the supplied group definition.
						string[] userList = location.InnerText.Split(new char[] { ',' });
						foreach (string u in userList) {
							if (users.Length > 0) users.Append(",");
							if (u.Trim().StartsWith("@")) {
								users.Append(permsConf.SelectSingleNode("//groups/" + u.Trim().Substring(1)).InnerText.Trim());
							} else {
								users.Append(u.Trim());
							}
						}
					}
					string p = locPath;
					//Paths never start with /, so remove it if provided.
					if (p.StartsWith("/")) {
						p = p.TrimStart(new char[] { '/' });
					}

					// now check if the commit matches the permissions.
					regex = new System.Text.RegularExpressions.Regex(p, System.Text.RegularExpressions.RegexOptions.Compiled);
					if (regex.IsMatch(sc.Path)) {
						switch (sc.Action) {
							case "U":
								sc.Allowed = (users.ToString().IndexOf(author) >= 0) && (locActions.IndexOf("update") >= 0);
								err = "You (" + author + ") can't update \n" + sc.Path;
								break;
							case "D":
								sc.Allowed = (users.ToString().IndexOf(author) >= 0) && (locActions.IndexOf("remove") >= 0);
								err = "You (" + author + ") can't remove \n" + sc.Path;
								break;
							case "A":
								sc.Allowed = (users.ToString().IndexOf(author) >= 0) && (locActions.IndexOf("add") >= 0);
								// fixed error message (Marcus Maday)
								err = "You (" + author + ") can't add \n" + sc.Path;
								break;
						}
						// changed to break on allowed to cater for multiple permissions specified.
						if (sc.Allowed) break;
					}
				}
				if (!sc.Allowed) {
					if (err.Length == 0)
						err = sc.Path;
					err = string.Concat("You don't have enough permissions for this transaction: \n", err, errMsgSuffix);
					// commit not allowed.
					PrintError(err);
					goto DebugLabel;
				}
			}

			// commit allowed.
			System.Environment.ExitCode = 0;
		DebugLabel:
#if DEBUG
			if (System.Environment.ExitCode == 0)
				Console.WriteLine("All OK!");
			Console.ReadLine();
#endif
			return;
		}
		
		/// <summary>
		/// Shells a Svn command and reads Error and Standard Output.
		/// </summary>
		/// <param name="cmd">Svn Command to execute.</param>
		/// <returns>Svn Command's Standard Output.</returns>
		private static string SvnCmd(string cmd) {
			string fileName = string.Empty;
			string arguments = string.Empty;
			
			// build Svn command.
			switch (cmd.ToLower()) {
				case "changed" :
				case "author" :
				case "log" :
					fileName = string.Concat(_svnDir, "svnlook");
					arguments = string.Concat(" ", cmd, " ", _switch, " \"", _txnRev, "\" \"", _repos, "\"");
					break;
				default :
					_stdOut = null;
					_stdErr = "Svn Command '" + cmd + "' not supported.";
					return _stdOut;
			}
			
			// call svn command.
			System.Diagnostics.Process process = new System.Diagnostics.Process();
			process.StartInfo.FileName = fileName;
			process.StartInfo.Arguments = arguments;
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.RedirectStandardError = true;
			process.StartInfo.CreateNoWindow = true;
			process.Start();
			process.WaitForExit();
#if DEBUG
			Console.WriteLine(fileName + " " + arguments);
#endif
			// read the output.
			_stdErr = process.StandardError.ReadToEnd();
			_stdOut = process.StandardOutput.ReadToEnd();
			
			return _stdOut;
		}
	}
}
