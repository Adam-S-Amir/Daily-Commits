using System;
using System.Diagnostics;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
class Program
{
    private const string FILE_TO_COMMIT_NAME = "../update_me.yaml";
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

        // Update the YAML file
        var updatedYamlData = UpdateFileToCommit();
        CommitRepository(updatedYamlData);

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

    static dynamic UpdateFileToCommit()
    {
        Console.WriteLine("Reading the YAML file...");

        int updateTimes = 0; // Initialize with a default value

        try
        {
            using (StreamReader reader = new StreamReader(FILE_TO_COMMIT_NAME))
            {
                var deserializer = new DeserializerBuilder().Build();
                dynamic currentData = deserializer.Deserialize(reader);

                // Check if the UPDATE_TIMES key exists and if its value is an integer
                if (currentData.ContainsKey("UPDATE_TIMES") && currentData["UPDATE_TIMES"] is int)
                {
                    updateTimes = (int)currentData["UPDATE_TIMES"] + 1; // Increment the value by 1
                }
                else
                {
                    // Handle the case where the key UPDATE_TIMES is missing or not an integer
                    Console.WriteLine("Error: UPDATE_TIMES key is missing or not an integer.");
                    return null;
                }

                string lastUpdate = DateTime.Now.ToString("dddd MMMM dd yyyy 'at' hh:mm:ss tt");

                dynamic updatedData = new
                {
                    UPDATE_TIMES = updateTimes,
                    LAST_UPDATE = lastUpdate
                };

                Console.WriteLine("Writing to the YAML file...");

                var serializer = new SerializerBuilder().Build();
                using (StreamWriter writer = new StreamWriter(FILE_TO_COMMIT_NAME))
                {
                    serializer.Serialize(writer, updatedData);
                }

                Console.WriteLine("YAML file updated successfully.");
                return updatedData;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error reading the YAML file: {e.Message}");
            return null;
        }
    }

    static void CommitRepository(dynamic yamlData)
    {
        if (yamlData == null)
        {
            Console.WriteLine("No data to commit.");
            return;
        }

        Console.WriteLine("Adding file to Git index...");

        var processStartInfo = new ProcessStartInfo
        {
            FileName = "git",
            Arguments = $"add {FILE_TO_COMMIT_NAME}",
            UseShellExecute = false,
            RedirectStandardOutput = true
        };

        using (var process = Process.Start(processStartInfo))
        {
            process.WaitForExit();
        }

        string commitMessage = $"Updated {yamlData.UPDATE_TIMES} times. Last update was on {yamlData.LAST_UPDATE}.";

        processStartInfo = new ProcessStartInfo
        {
            FileName = "git",
            Arguments = $"commit -m \"{commitMessage}\"",
            UseShellExecute = false,
            RedirectStandardOutput = true
        };

        using (var process = Process.Start(processStartInfo))
        {
            process.WaitForExit();
        }

        processStartInfo = new ProcessStartInfo
        {
            FileName = "git",
            Arguments = "push",
            UseShellExecute = false,
            RedirectStandardOutput = true
        };

        using (var process = Process.Start(processStartInfo))
        {
            process.WaitForExit();
        }

        Console.WriteLine("Changes committed and pushed to remote repository.");
    }
}