using System;
using System.Diagnostics;
using System.Formats.Tar;
using System.IO;
using System.IO.Enumeration;
using System.Linq;
using System.Net.NetworkInformation;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

class ConsoleManager
{
    private List<string> validCommandsList;
    private string inputCommand;
    private string[] splitInputList;

    public ConsoleManager()
    {
        validCommandsList = new List<string>
        {
            "exit",
            "echo",
            "type",
            "pwd",
            "cd"
        };
        inputCommand = "";
        splitInputList = Array.Empty<string>();
    }

    public string HandleConsoleLine(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return "";

        return ProcessUserInput(input);
    }

    private string ProcessUserInput(string inputCommand)
    {
        this.inputCommand = inputCommand;

        // Handle echo command with regex
        string pattern = "echo.+";
        Regex regularExpressionObject = new Regex(pattern, RegexOptions.IgnoreCase);
        Match checkingMatch = regularExpressionObject.Match(inputCommand);

        if (!checkingMatch.Success)
        {
            if (inputCommand.Contains('\'') || inputCommand.Contains('\"'))
            {
                WhatIsInQuotes(inputCommand, ref splitInputList);
            }
            else
            {
                splitInputList = inputCommand.Split(' ');
            }
        }
        else
        {
            return EchoCommand(inputCommand);
        }

        // Check for invalid single-word commands
        if (splitInputList.Length == 1 && !validCommandsList.Contains(inputCommand))
        {
            return $"{inputCommand}: command not found";
        }

        // Check for executable files
        bool checker = false;
        if ((splitInputList[0].Contains(".exe") && splitInputList.Length > 1) ||
            (splitInputList[0].Contains("_exe") && splitInputList.Length > 1) ||
            (splitInputList[0].Contains("cat") && splitInputList.Length > 1))
        {
            checker = true;
            return TypeBuiltCommand(splitInputList, validCommandsList, splitInputList[0]);
        }

        // Handle pwd command
        if (splitInputList[0] == "pwd" && splitInputList.Count() == 1)
        {
            return PrintWorkingDirectory(splitInputList, validCommandsList);
        }

        // Handle exit command
        if (splitInputList[0] == "exit" && splitInputList.Count() == 1)
        {
            ExitCommand(splitInputList, inputCommand);
            return "";
        }

        // Handle other commands
        if (splitInputList.Count() > 1 && CheckDoesCommandExist(splitInputList, inputCommand, validCommandsList) && !checker)
        {
            if (splitInputList[0] == "exit")
            {
                ExitCommand(splitInputList, inputCommand);
                return "";
            }

            if (splitInputList[0] == "type")
            {
                return TypeBuiltCommand(splitInputList, validCommandsList, splitInputList[1]);
            }

            if (splitInputList[0] == "cd")
            {
                return ChangeDirectory(splitInputList, validCommandsList);
            }
        }

        return "";
    }

    public bool CheckDoesCommandExist(string[] splitInputList, string inputCommand, List<string> validCommandsList)
    {
        foreach (string item in validCommandsList)
        {
            if (inputCommand.StartsWith(item))
            {
                return true;
            }
        }

        // checks the second string given does it exist in commands
        if (splitInputList[0] == "type")
        {
            return false;
        }

        if ((splitInputList[0].Contains("_exe") && splitInputList.Length > 1) ||
            (splitInputList[0].Contains(".exe") && splitInputList.Length > 1) ||
            (splitInputList[0].Contains("cat") && splitInputList.Length > 1))
        {
            return true;
        }

        return false;
    }

    private void ExitCommand(string[] splitInputList, string inputCommand)
    {
        if (splitInputList[0] == "exit" && splitInputList.Length == 1)
        {
            Environment.Exit(0);
        }
        if (splitInputList[0] == "exit" && splitInputList.Length > 1)
        {
            foreach (string item in splitInputList)
            {
                if (splitInputList[0] == "exit" && splitInputList[1] == "0")
                {
                    Environment.Exit(Int32.Parse(splitInputList[1]));
                }

                if (splitInputList[0] == "exit" && splitInputList[1] == "1")
                {
                    Environment.Exit(Int32.Parse(splitInputList[1]));
                }
            }
        }
    }

    private string EchoCommand(string inputCommand)
    {
        // Remove "echo "
        string args = inputCommand.Substring(5).TrimStart();

        StringBuilder result = new StringBuilder();
        bool inQuotes = false;
        char quoteChar = '\0';

        for (int i = 0; i < args.Length; i++)
        {
            char c = args[i];

            // Check for quotes
            if (c == '\'' || c == '"')
            {
                if (!inQuotes)
                {
                    // Starting quotes
                    inQuotes = true;
                    quoteChar = c;
                }
                else if (c == quoteChar)
                {
                    // Ending quotes of same type
                    inQuotes = false;
                }
                else
                {
                    // Different quote char inside quotes - treat as literal
                    result.Append(c);
                }
                continue;
            }

            // Handle spaces
            if (c == ' ')
            {
                if (inQuotes)
                {
                    // Inside quotes: preserve all spaces
                    result.Append(' ');
                }
                else
                {
                    // Outside quotes: collapse multiple spaces
                    if (result.Length == 0 || result[result.Length - 1] != ' ')
                    {
                        result.Append(' ');
                    }
                }
            }
            else
            {
                // Regular character
                result.Append(c);
            }
        }

        return result.ToString().Trim();
    }

    private string TypeBuiltCommand(string[] splitInputList, List<string> validCommandsList, string nameOfFile)
    {
        if (!validCommandsList.Contains(splitInputList[0]))
        {
            return ExecutesFileIfMeetRequirements(splitInputList[0], splitInputList);
        }

        if (splitInputList[0] == "type")
        {
            string[] splitPathList = Array.Empty<string>();
            string pathListString = Environment.GetEnvironmentVariable("PATH") ?? "";
            bool wordCheckerIsPath = false;

            // Use custom path configuration
            string userInput = $@"E:\Downloads\c#programs\TestingProccesClass\bin\Debug\net8.0{Path.PathSeparator}$PATH";
            string expandedInput = userInput
                .Replace("$PATH", pathListString)
                .Replace("${PATH}", pathListString)
                .Replace("%PATH%", pathListString);
            splitPathList = expandedInput.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries);

            string changedWord = "";

            if (!validCommandsList.Contains(nameOfFile))
            {
                foreach (string directoryString in splitPathList)
                {
                    if (!Directory.Exists(directoryString))
                    {
                        continue;
                    }

                    changedWord = Path.Join(directoryString, nameOfFile);

                    if (File.Exists(changedWord))
                    {
                        wordCheckerIsPath = true;

                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                        {
                            var mode = File.GetUnixFileMode(changedWord);
                            if ((mode & UnixFileMode.UserExecute) != 0 ||
                                (mode & UnixFileMode.GroupExecute) != 0 ||
                                (mode & UnixFileMode.OtherExecute) != 0)
                            {
                                if (splitInputList[0] == "type")
                                {
                                    return $"{nameOfFile} is {changedWord}";
                                }
                                else
                                {
                                    string arguments = string.Join(" ", splitInputList.Skip(1));
                                    return ExecutesFileIfMeetRequirements(nameOfFile, splitInputList);
                                }
                            }
                        }

                        if (Path.PathSeparator == ';')
                        {
                            if (splitInputList[0] == "type")
                            {
                                return $"{nameOfFile} is {changedWord}";
                            }
                            else
                            {
                                string arguments = string.Join(" ", splitInputList.Skip(1));
                                return ExecutesFileIfMeetRequirements(changedWord, splitInputList);
                            }
                        }
                    }
                }
            }

            // Check if second word after type is valid
            if (splitInputList[0] == "type" && !wordCheckerIsPath)
            {
                if (validCommandsList.Contains(splitInputList[1]) && splitInputList.Count() == 2)
                {
                    return $"{splitInputList[1]} is a shell builtin";
                }
                else
                {
                    return $"{splitInputList[1]}: not found";
                }
            }

            if (splitInputList[0] == "type" && splitInputList.Count() > 2 && !File.Exists(changedWord))
            {
                return $"{splitInputList[1]}: not found";
            }
        }

        return "";
    }

    private string ExecutesFileIfMeetRequirements(string nameOfFile, string[] splitInputList)
    {
        string executable = nameOfFile;

        if (nameOfFile == "cat")
        {
            // Check common locations for cat on linux
            string[] possiblePaths = { "/bin/cat", "/usr/bin/cat", "cat" };
            bool found = false;

            foreach (string path in possiblePaths)
            {
                try
                {
                    if (File.Exists(path) || path == "cat")
                    {
                        executable = path;
                        found = true;
                        break;
                    }
                }
                catch
                {
                    // Ignore errors
                }
            }

            if (!found)
            {
                return "cat: command not found";
            }
        }

        try
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = executable,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            foreach (string item in splitInputList.Skip(1))
            {
                processStartInfo.ArgumentList.Add(item);
            }

            var process = Process.Start(processStartInfo);

            // Capture the output
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            process.WaitForExit();

            if (!string.IsNullOrEmpty(error))
            {
                return error.Trim();
            }

            return output.Trim();
        }
        catch (Exception ex)
        {
            return $"{nameOfFile}: error executing - {ex.Message}";
        }
    }

    private string PrintWorkingDirectory(string[] splitInputList, List<string> validCommandsList)
    {
        if (splitInputList[0] == "pwd" && splitInputList.Count() == 1)
        {
            string pathWorkingDirectory = Directory.GetCurrentDirectory();
            return pathWorkingDirectory;
        }
        return "";
    }

    private string ChangeDirectory(string[] splitInputList, List<string> validCommandsList)
    {
        if (splitInputList[0] == "cd" && splitInputList[1] == "~")
        {
            string homePath = (Environment.OSVersion.Platform == PlatformID.Unix ||
                                Environment.OSVersion.Platform == PlatformID.MacOSX)
                                ? Environment.GetEnvironmentVariable("HOME")
                                : Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");

            try
            {
                Directory.SetCurrentDirectory(homePath);
                return "";
            }
            catch (Exception ex)
            {
                return $"cd: {homePath}: {ex.Message}";
            }
        }

        if (splitInputList[0] == "cd" && Directory.Exists(splitInputList[1]))
        {
            try
            {
                Directory.SetCurrentDirectory(splitInputList[1]);
                return "";
            }
            catch (Exception ex)
            {
                return $"cd: {splitInputList[1]}: {ex.Message}";
            }
        }

        if (splitInputList[0] == "cd" && !Directory.Exists(splitInputList[1]))
        {
            return $"cd: {splitInputList[1]}: No such file or directory";
        }

        return "";
    }

    private void WhatIsInQuotes(string input, ref string[] inputlist)
    {
        if (input.Contains('\"'))
        {
            string[] splitInputList = input.Split('\"');
            List<string> result = new List<string>();

            foreach (string s in splitInputList)
            {
                string trimmed = s.Trim();
                if (!string.IsNullOrEmpty(trimmed))
                {
                    result.Add(trimmed);
                }
            }

            inputlist = result.ToArray();
        }
        else
        {
            string[] splitInputList = input.Split('\'');
            List<string> result = new List<string>();

            foreach (string s in splitInputList)
            {
                string trimmed = s.Trim();
                if (!string.IsNullOrEmpty(trimmed))
                {
                    result.Add(trimmed);
                }
            }

            inputlist = result.ToArray();
        }
    }
}