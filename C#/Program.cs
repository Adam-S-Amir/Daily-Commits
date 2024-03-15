using System;
using System.Diagnostics;

class Program
{
    static void Main(string[] args)
    {
        // Check if Git is installed
        bool? gitInstalled = IsGitInstalled();

        if (gitInstalled.HasValue)
        {
            // Check if user is logged into Git
            bool? userLoggedIn = IsUserLoggedInToGit();

            if (userLoggedIn.HasValue)
            {
                Console.WriteLine("User is logged into Git.");
            }
            else
            {
                Console.WriteLine("User is not logged into Git.");
            }
        }
        if (!gitInstalled.HasValue)
        {
            Console.WriteLine("Git is not installed.");
        }
    }

    static bool? IsGitInstalled()
    {
        try
        {
            Process.Start("git", "--version").WaitForExit();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    static bool? IsUserLoggedInToGit()
    {
        try
        {
            ProcessStartInfo startInfo = new()
            {
                FileName = "git",
                Arguments = "config --get user.email",
                RedirectStandardOutput = true,
                UseShellExecute = false
            };

            using (Process process = Process.Start(startInfo))
            {
                process.WaitForExit();
                string output = process.StandardOutput.ReadToEnd();
                return !string.IsNullOrWhiteSpace(output);
            }
        }
        catch (Exception)
        {
            return false;
        }
    }
}
