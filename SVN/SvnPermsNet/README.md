#Introduction
I implemented Subversion on windows to use Active Directory Authentication. I could not get the svnperms.py hook script to function correctly, however, and I've been looking for an excuse to try out .NET Framework 2.0. So here it is: SvnPerms.net.

SvnPerms.net is a C# port of the svnperms.py hook script supplied with Subversion. The only feature I've added is log message validation. This article assumes that you are familliar with SVN.

System requirements
SVN command line client
.NET Framework 2.0
The components
svnperms.xml
SvnPerms.exe
pre-commit.bat
svnperms.xml

The configuration file:
XML
<svnperms>
    <settings>

        <setting name="SvnDir">C:\svn\bin</setting>
        <setting name="LogRegex">[a-zA-Z0-9]</setting>
        <setting name="LogRegexMsg">You failed to supply a valid log message.</setting>
        <setting name="ErrMsgSuffix">Please contact your SVN admin.</setting>

    </settings>
    <groups>
        <group1>dev1,dev2,dev3</group1>
        <group2>dev4,dev5</group2>
    </groups>

    <sections>
        <default repos="repo1,repo2">
            <location path
                =&quot;^\btrunk\b\/.*" actions
                ="add,remove,update">@group1,dev4</location>
            <location path
                =&quot;^\btags\b\/\w+\/" actions
                ="add">@group1,dev4</location>
            <location path
                =&quot;^\bbranches\b\/\w+\/" actions
                ="add,remove,update">@group1,@group2</location>

            <location path
                =".*" actions
                ="add,remove,update">svnadministrator0</location>
        </default>
        <repo4>
            <location path=".*" actions="add,remove,update">*</location>
        </repo4>

    </sections>
</svnperms>
The setting[@name='SvnDir'] element: the path to the SVN command line client.
The setting[@name='LogRegex'] element: regex to validate log messages.
The groups element: groups of users.
The sections element: repos to match.
The default section allows for repos that all have the same structure, e.g. trunk, branches and tags.
Repos with other structures can be defined with more sections.
The sections consist of location elements -- each with a change path regex attribute -- and an actions attribute containing "add," "remove" and "update" or any combination of the three. The text of the location element contains a comma-separated list of groups and users.
SvnPerms.exe
Calling SvnPerms.exe with no arguments:

$>SvnPerms.exe
USAGE: SvnPerms TXN|REV REPOS [ CONF ] [ -r ]
      TXN     Query transaction TXN for commit information.
      REPOS   Use repository at REPOS to check transactions.
      CONF    Config file path;
              defaults to 'svnperms.xml' in application directory.

      -r      Debug mode:
              Uses revision number instead of commit transaction nubmer.
Author: Riaan Lehmkuhl 2006.
Calling the SVN commands:

C#
Shrink ▲   
private static string SvnCmd(string cmd) {
    string fileName = string.Empty;
    string arguments = string.Empty;

    // build Svn command.

    switch (cmd.ToLower()) {
        case "changed" :
        case "author" :
        case "log" :
            fileName = string.Concat(_svnDir, "svnlook");
            arguments = string.Concat(" ", cmd, " ", _switch,
                " \"", _txnRev, "\" \"", _repos, "\"");
            break;
        default :
            _stdOut = null;
            _stdErr = "Svn Command '" + cmd + "' not supported.";
            return _stdOut;
    }

    // call svn command.

    System.Diagnostics.Process process
        = new System.Diagnostics.Process();
    process.StartInfo.FileName = fileName;
    process.StartInfo.Arguments = arguments;
    process.StartInfo.UseShellExecute = false;
    process.StartInfo.RedirectStandardOutput = true;
    process.StartInfo.RedirectStandardError = true;
    process.StartInfo.CreateNoWindow = true;
    process.Start();
    process.WaitForExit();

    // read the output.

    _stdErr = process.StandardError.ReadToEnd();
    _stdOut = process.StandardOutput.ReadToEnd();

    return _stdOut;
}
I've tried to comment the code as much as I had time for, to explain how the application fits together. When the log has been validated sucessfuly and the user has the required permissions, SvnPerms.exe will exit with an exit code of 0. Otherwise, it will exit with a non-zero exit code that will cancel and rollback the commit.

pre-commit.bat
SET REPOS=%1
SET REV=%2

[drive]:\[path]\SvnPerms.exe %REV% %REPOS%
Modify the path to where you have put SvnPerms.exe. Copy the pre-commit.bat file to the repository's "hooks" directory. Any commits will be validated by SvnPerms.exe.
