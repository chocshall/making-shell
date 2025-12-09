
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;


public class ConsoleManager
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
    // testing 
    
    public string HandleConsoleLine(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return "";

        return ProcessUserInput(input);
    }


    private string ProcessUserInput(string userInputCommand)
    {
        inputCommand = userInputCommand;
        

        splitInputList = inputCommand.Split(' ');

        if (inputCommand.Contains('\"') )
        {
            inputCommand = DealingWithSpaceWhileHasQoutes(inputCommand);
        }
        string[] commandLineArgs = Array.Empty<string>();
       
        

        switch (CheckValidCommandExist(splitInputList,validCommandsList))
        {
            case "echo":
                return EchoCommand(inputCommand);
                break;
            case "exit":
                ExitCommand(splitInputList, inputCommand);
                return "";
                break;
            case "type":
                return TypeBuiltCommand(splitInputList, validCommandsList, splitInputList[1], commandLineArgs);
                break;
            case "pwd":
                return PrintWorkingDirectory(splitInputList, validCommandsList);
                break;
            case "cd":
                return ChangeDirectory(splitInputList, validCommandsList);
                break;
            default:
                
                break;
        }
        commandLineArgs = ParsingInput(inputCommand);
        splitInputList = commandLineArgs;




        if (!validCommandsList.Contains(splitInputList[0]) && splitInputList.Length > 1)
        {
            return ExecutesFileIfMeetRequirements(splitInputList[0], commandLineArgs, inputCommand);
        }
        

        return $"{splitInputList[0]}: command not found";
    }

    public string CheckValidCommandExist(string[] splitInputList, List<string> validCommandsList)
    {
       
        if (validCommandsList.Contains(splitInputList[0]))
        {
            return splitInputList[0];
        }

        
        return "";

    }

    protected virtual void ExitCommand(string[] splitInputList, string inputCommand)
    {
        int exitNumber = 0;
        if (splitInputList[0] == "exit" && splitInputList.Length == 1)
        {
            Environment.Exit(exitNumber);
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

        int count = 0;
        char oneTimeChar = ' ';

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
                else if (c != '\\')
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
                continue;
            }

            if (c == '\\' && i + 1 < args.Length)
            {
                char nextChar = args[i + 1];

                if (inQuotes && quoteChar == '\'')
                {

                    result.Append(c);
                    continue;
                }

                else if (inQuotes && quoteChar == '\"')
                {

                    if (nextChar == '"' || nextChar == '\\' || nextChar == '$' || nextChar == '`')
                    {

                        result.Append(nextChar);
                        i++;
                        continue;
                    }
                    else
                    {
                        result.Append(c);
                        continue;
                    }

                }

                if (i != 0)
                {
                    if (args[i - 1] != '\\' && nextChar != '\\' && nextChar != ' ' && nextChar != '\'' && nextChar != '"')
                    {
                        continue;
                    }
                }
                if (i == 0 && nextChar != '\\' && nextChar != ' ' && nextChar != '\'' && nextChar != '"')
                {
                    continue;
                }


                if (nextChar == ' ' || nextChar == '\\' || nextChar == '\'' || nextChar == '"')
                {
                    result.Append(nextChar);
                    i++;
                    continue;
                }



            }


            result.Append(c);






        }

        return result.ToString().Trim();

    }

    private string TypeBuiltCommand(string[] splitInputList, List<string> validCommandsList, string nameOfFile, string[] commandLineArgs)
    {
        

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
                                    return ExecutesFileIfMeetRequirements(nameOfFile, splitInputList, inputCommand);
                                }
                            }
                        }

                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        {
                            if (splitInputList[0] == "type")
                            {
                                return $"{nameOfFile} is {changedWord}";
                            }
                            else
                            {
                                string arguments = string.Join(" ", splitInputList.Skip(1));
                                return ExecutesFileIfMeetRequirements(changedWord, splitInputList, inputCommand);
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

    private string ExecutesFileIfMeetRequirements(string nameOfFile, string[] splitInputList, string parsedInput)
    {
        string executable = splitInputList[0];
        
        
        string argsString = inputCommand;
        //foreach (var item in splitInputList)
        //{
        //    Console.WriteLine(item);
        //}

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

            for (int i = 1; i < splitInputList.Length; i++)
            {
                processStartInfo.ArgumentList.Add(splitInputList[i]);
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

    public string[] ParsingInput(string input)
    {
        List<string> listForArgs = new List<string>();
        int firstSpace = input.IndexOf(' ');

        if (firstSpace > -1)
        {
            string commandName = input.Substring(0, input.IndexOf(' '));
            listForArgs.Add(commandName);

            string args = input.Substring(input.IndexOf(' ') + 1);
            if (input.Contains("\'") || input.Contains("\"") && input.Contains('\\'))
            {

                StringBuilder result = new StringBuilder();
                bool inQuotes = false;
                char quoteChar = '\0';

                int count = 0;
                char oneTimeChar = ' ';

                for (int i = 0; i < args.Length; i++)
                {
                    char c = args[i];

                    //checking for ' ' or " " before skipping 2 times and countinue.adding the made word before clearing result

                    if(!input.Contains("echo"))
                    {
                        if (i + 2 < args.Length && result.Length > 0)
                        {
                            if (args[i] == '\"' && args[i + 1] == ' ' && args[i + 2] == '\"')
                            {

                                listForArgs.Add(result.ToString());
                                result.Clear();
                                i += 2;
                                continue;
                            }

                            if (args[i] == '\'' && args[i + 1] == ' ' && args[i + 2] == '\'')
                            {

                                listForArgs.Add(result.ToString());
                                result.Clear();
                                i += 2;
                                continue;
                            }
                        }
                    }
                    

                    

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
                        else if (c != '\\')
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
                        continue;
                    }

                    if(c == '\\' && i + 1 < args.Length)
{
                        char nextChar = args[i + 1];

                        if (inQuotes && quoteChar == '\'')
                        {
                            result.Append(c);
                            continue;
                        }
                        else if (inQuotes && quoteChar == '\"')
                        {
                            if (nextChar == '"' || nextChar == '\\' || nextChar == '$' || nextChar == '`')
                            {
                                result.Append(nextChar);
                                i++;
                                continue;
                            }
                            else
                            {
                                result.Append(c);
                                continue;
                            }
                        }

                        // NOT in quotes - SIMPLIFIED
                        // Remove backslash for escaped special characters
                        if (nextChar == '\\' || nextChar == ' ' || nextChar == '\'' || nextChar == '"')
                        {
                            result.Append(nextChar);  // Keep the escaped character
                            i++;  // Skip it
                            continue;
                        }

                        // For any other character after backslash, keep backslash as literal
                        result.Append(c);
                    }


                    result.Append(c);

                }
                //Console.WriteLine(result + " aa");
                if (result.Length > 0)
                {
                    if(input.StartsWith("echo"))
                    {
                        
                        string[] leftArgsArray = result.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        foreach (var item in leftArgsArray)
                        {
                            listForArgs.Add(item);
                        }
                    }

                    else
                    {
                        listForArgs.Add(result.ToString());
                    }
                   
                    

                }
                //foreach (string c in listForArgs)
                //{
                //    Console.WriteLine(c);
                //}


            }


            else
            {

                if (args.Contains('\\'))
                {
                    //Console.WriteLine(10);
                    if (firstSpace > -1)
                    {
                        commandName = input.Substring(0, input.IndexOf(' '));

                        args = input.Substring(input.IndexOf(' ') + 1);
                        //Console.WriteLine(args + " tebn");
                        string argsResult = "";
                        for (int i = 0; i < args.Length; i++)
                        {
                            if (args[i] == '\\' && args[i + 1] == ' ' && args.Length > i + 1)
                            {
                                argsResult += ' ';
                                i++;
                                continue;
                            }
                            else
                            {
                                argsResult += args[i];
                            }
                        }
                        listForArgs.Add(argsResult);
                        //foreach (var item in listForArgs)
                        //{
                        //    Console.WriteLine(item);
                        //}
                        //Console.WriteLine(argsResult + " cia");
                    }
                    return listForArgs.ToArray();

                }

                else
                {
                    string[] argsArray = args.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                    string commandWord = input.Substring(0, input.IndexOf(' '));
                    string[] fullArray = new string[argsArray.Length + 1];
                    fullArray[0] = commandName;
                    Array.Copy(argsArray, 0, fullArray, 1, argsArray.Length);
                    //foreach (string c in fullArray)
                    //{
                    //    Console.WriteLine(c + "ASD");
                    //}

                    return fullArray;
                }

                    
            }
            return listForArgs.ToArray();
        }

        return new string[] { input };



    }
    public string DealingWithSpaceWhileHasQoutes(string input)
    {
        int spaceIndex = input.IndexOf(' ');
        string noMultipleBlanksString = "";
        if (spaceIndex != -1)
        {
            string firstArg = input.Substring(0, spaceIndex);
            noMultipleBlanksString += firstArg + " ";

        }

        string args = input.Substring(input.IndexOf(' ') + 1);
        List<int> indexList = new List<int>();

        char? currentQuoteType = null;
        bool insideQuotes = false;

        for (int i = 0; i < args.Length; i++)
        {
            // Handle backslashes
            if (args[i] == '\\' && i + 1 < args.Length)
            {
                noMultipleBlanksString += args[i];
                noMultipleBlanksString += args[i + 1];
                i++; // Skip escaped character
                continue;
            }

            // Handle quotes
            if (args[i] == '\"' || args[i] == '\'')
            {
                if (!insideQuotes)
                {
                    // Opening quote
                    currentQuoteType = args[i];
                    insideQuotes = true;
                }
                else if (args[i] == currentQuoteType)
                {
                    // Closing quote (same type)
                    currentQuoteType = null;
                    insideQuotes = false;
                }
                // If we're inside double quotes and encounter single quote, treat as regular char
                // and vice versa

                noMultipleBlanksString += args[i];
                continue;
            }

            // Handle spaces
            if (args[i] == ' ')
            {
                if (insideQuotes)
                {
                    noMultipleBlanksString += args[i];
                }
                else
                {
                    // Only add space if previous wasn't space
                    if (noMultipleBlanksString.Length == 0 ||
                        noMultipleBlanksString[^1] != ' ')
                    {
                        noMultipleBlanksString += args[i];
                    }
                }
                continue;
            }

            // Regular character
            noMultipleBlanksString += args[i];
        }

        input = noMultipleBlanksString;
        //Console.WriteLine(input);
        return input;
    }
}



