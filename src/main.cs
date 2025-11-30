using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Linq;

namespace CodeCraftersShell
{
    class Program
    {
        private static string currentDirectory = Directory.GetCurrentDirectory();
        private static Dictionary<string, string> builtinCommands = new Dictionary<string, string>
        {
            { "exit", "builtin" },
            { "echo", "builtin" },
            { "type", "builtin" },
            { "pwd", "builtin" },
            { "cd", "builtin" }
        };

        static void Main(string[] args)
        {
            RunREPL();
        }

        static void RunREPL()
        {
            while (true)
            {
                PrintPrompt();
                string input = Console.ReadLine()?.Trim();

                if (string.IsNullOrEmpty(input))
                    continue;

                ProcessInput(input);
            }
        }

        static void PrintPrompt()
        {
            Console.Write("$ ");
        }

        static void ProcessInput(string input)
        {
            string[] parts = ParseInput(input);
            if (parts.Length == 0) return;

            string command = parts[0];
            string[] arguments = parts.Length > 1 ? parts.Skip(1).ToArray() : new string[0];

            switch (command)
            {
                case "exit":
                    HandleExit();
                    break;
                case "echo":
                    HandleEcho(arguments);
                    break;
                case "type":
                    HandleType(arguments);
                    break;
                case "pwd":
                    HandlePwd();
                    break;
                case "cd":
                    HandleCd(arguments);
                    break;
                default:
                    HandleExternalCommand(command, arguments);
                    break;
            }
        }

        static string[] ParseInput(string input)
        {
            List<string> parts = new List<string>();
            bool inQuotes = false;
            string currentPart = "";

            foreach (char c in input)
            {
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (char.IsWhiteSpace(c) && !inQuotes)
                {
                    if (!string.IsNullOrEmpty(currentPart))
                    {
                        parts.Add(currentPart);
                        currentPart = "";
                    }
                }
                else
                {
                    currentPart += c;
                }
            }

            if (!string.IsNullOrEmpty(currentPart))
            {
                parts.Add(currentPart);
            }

            return parts.ToArray();
        }

        static void HandleExit()
        {
            Environment.Exit(0);
        }

        static void HandleEcho(string[] arguments)
        {
            Console.WriteLine(string.Join(" ", arguments));
        }

        static void HandleType(string[] arguments)
        {
            if (arguments.Length == 0)
            {
                Console.WriteLine("type: missing argument");
                return;
            }

            string command = arguments[0];

            if (builtinCommands.ContainsKey(command))
            {
                Console.WriteLine($"{command} is a shell builtin");
            }
            else
            {
                string executablePath = FindExecutable(command);
                if (executablePath != null)
                {
                    Console.WriteLine($"{command} is {executablePath}");
                }
                else
                {
                    Console.WriteLine($"{command}: not found");
                }
            }
        }

        static void HandlePwd()
        {
            Console.WriteLine(currentDirectory);
        }

        static void HandleCd(string[] arguments)
        {
            string targetPath;

            if (arguments.Length == 0)
            {
                // No arguments - go to home directory
                targetPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            }
            else
            {
                targetPath = arguments[0];

                // Handle ~ as home directory
                if (targetPath == "~" || targetPath.StartsWith("~/"))
                {
                    string homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                    if (targetPath == "~")
                    {
                        targetPath = homeDir;
                    }
                    else
                    {
                        targetPath = Path.Combine(homeDir, targetPath.Substring(2));
                    }
                }

                // Convert relative paths to absolute
                if (!Path.IsPathRooted(targetPath))
                {
                    targetPath = Path.Combine(currentDirectory, targetPath);
                }
            }

            try
            {
                if (Directory.Exists(targetPath))
                {
                    currentDirectory = Path.GetFullPath(targetPath);
                    Directory.SetCurrentDirectory(currentDirectory);
                }
                else
                {
                    Console.WriteLine($"cd: {targetPath}: No such file or directory");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"cd: {targetPath}: {ex.Message}");
            }
        }

        static void HandleExternalCommand(string command, string[] arguments)
        {
            string executablePath = FindExecutable(command);

            if (executablePath != null)
            {
                try
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        FileName = executablePath,
                        Arguments = string.Join(" ", arguments),
                        UseShellExecute = false,
                        RedirectStandardOutput = false,
                        WorkingDirectory = currentDirectory
                    };

                    Process process = Process.Start(startInfo);
                    process.WaitForExit();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error executing {command}: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine($"{command}: command not found");
            }
        }

        static string FindExecutable(string command)
        {
            // Check if it's an absolute or relative path
            if (Path.IsPathRooted(command))
            {
                if (File.Exists(command) && IsExecutable(command))
                    return command;
                return null;
            }

            // Check relative to current directory
            string relativePath = Path.Combine(currentDirectory, command);
            if (File.Exists(relativePath) && IsExecutable(relativePath))
                return relativePath;

            // Check in PATH directories
            string pathEnv = Environment.GetEnvironmentVariable("PATH");
            if (!string.IsNullOrEmpty(pathEnv))
            {
                string[] pathDirectories = pathEnv.Split(Path.PathSeparator);

                foreach (string directory in pathDirectories)
                {
                    if (string.IsNullOrEmpty(directory)) continue;

                    string fullPath = Path.Combine(directory, command);
                    if (File.Exists(fullPath) && IsExecutable(fullPath))
                        return fullPath;

                    // On Windows, also check with .exe extension
                    if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                    {
                        string exePath = fullPath + ".exe";
                        if (File.Exists(exePath) && IsExecutable(exePath))
                            return exePath;
                    }
                }
            }

            return null;
        }

        static bool IsExecutable(string filePath)
        {
            try
            {
                // On Unix-like systems, check if file has execute permission
                if (Environment.OSVersion.Platform == PlatformID.Unix ||
                    Environment.OSVersion.Platform == PlatformID.MacOSX)
                {
                    var fileInfo = new FileInfo(filePath);
                    return (fileInfo.Attributes & FileAttributes.Directory) == 0;
                }

                // On Windows, check file extensions
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    string extension = Path.GetExtension(filePath).ToLower();
                    return extension == ".exe" || extension == ".com" || extension == ".bat" || extension == ".cmd";
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}