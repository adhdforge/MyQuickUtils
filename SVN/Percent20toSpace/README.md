# TSVN Post-Update hook utility for using svn:externals with a space in the directory name.
Introduction
While setting up my development environment for various BlogEngine.NET sites, I ran into the problem that when setting svn:externals, we cannot have spaces in the directory name or the external URL with TortoiseSVN. For example:

User controls svn://host/User controls
According to a message in the TSVN mailing list, the trick is to replace the space with "%20", like this:

User%20controls svn://host/User%20controls
Now being able to add the external, there is a new problem: when I put the "%20" in the name, I get a directory with the characters "%20" in the name. I could have just used an underscore, but there are references to the "User controls" directory in the project, and for future compatibility, I didn't want to deviate from the original structure.

To overcome this, I've created a tiny console application to rename the directory when the TSVN Post-Update Hook is fired.

Update: As pointed out by MichaelSimons, 'if you have modified any sources in the new "spaced" folder, all those changes will be lost on the next update'. So keep in mind that the point of this would be to add external sources to your development environment and not to work in the externals themselves. They would normally be modified from the project they originated from. An example of this could be a vendor branch.

The code
The application is a simple console app that takes five arguments passed by TSVN when the hook is fired: PATH, DEPTH, REVISION, ERROR, and CWD.

We only work with the fifth one (CWD) which is the path where the SVN update has occurred.

This directory is then recursed to find directories containing "%20" in the name, and added to a LIFO stack so we can do the directory renames from the bottom up.

C#
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
If a directory with the name matching the target exists, it is first deleted. Because the ".svn" or "_svn" directories contain ReadOnly files, the directories to be deleted are first recursed and all the files' attributes set to Normal.

C#
/// <summary>
/// Recursively set ReadOnly files to Normal
/// </summary>
/// <param name="path">Directory to start at</param>
static void PrepDelete(string path) {
    foreach (string file in System.IO.Directory.GetFiles(path)) {
        if ((System.IO.File.GetAttributes(file) & 
                System.IO.FileAttributes.ReadOnly) == System.IO.FileAttributes.ReadOnly)
            System.IO.File.SetAttributes(file, System.IO.FileAttributes.Normal);
    }
    foreach (string dir in System.IO.Directory.GetDirectories(path)) {
        PrepDelete(dir);
    }
}
The found directories are then renamed with the "%20" replaced with a space.

C#
Shrink ▲   
static void Main(string[] args) {
    // argument checks: TSVN will add 5 args
    // to the call: PATH DEPTH REVISION ERROR CWD
    // we will only work with the 5th one which
    // is the path where the svn update has occured
    if (args.Length != 5) {
        Console.Error.WriteLine("Incorect number of arguments. " + 
                "Five expected (PATH DEPTH REVISION ERROR CWD).");
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
            // we can't just ignore, because we always
            // want the latest source from the external
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
Using the utility
Once you have downloaded the code and compiled Percent20toSpace, you can start by adding svn:external to the working copy by clicking the Properties button in the Subversion tab of the directory properties. Add a new svn:externals property:

Directory%20Name svn/http/https://host/svn/path%20to%20directory
svn_externals.png

Update your working copy and commit the property change. You will now notice the "Directory%20Name" directory instead of "Directory%20Name".

Now, we can go to the TSN settings and add the client-side Post-Update hook:

settings.png

hook.png

Enter the working copy path (can be top level as TSVN will return the affected path). Enter the path to Percent20toSpace.exe, click on OK, and click OK again to save.

When you do an SVN update now, you'll notice that "Directory%20Name" is automatically renamed to "Directory Name". If you update a second time, the "Directory Name" is deleted and replaced with the updated external.

Remember: If you already have a directory "Directory Name", it will be replaced with "Directory%20Name"!
