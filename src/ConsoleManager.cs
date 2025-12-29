using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace src
{
    public class ConsoleManager
    {
        public List<string> validCommandsList;
        private string inputCommand;
        private string[] splitInputList;
        public List<string> splitPathList;
        public List<string> inputLines = new List<string>();
        public List<string> inputLinesThatRan = new List<string>();
        public int flagACount = 0;

        public ConsoleManager(string pathListString)
        {
            validCommandsList = new List<string>
        {
            "exit",
            "echo",
            "type",
            "pwd",
            "cd",
            "history"
        };
            inputCommand = "";
            splitInputList = Array.Empty<string>();
            splitPathList = pathListString.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries).ToList();

#if DEBUG

            splitPathList.Add($@"C:\cSharp\ConsoleApp1\bin\Debug\net9.0");
            splitPathList.Add($@"C:\cSharp\newfile.txt");
#endif

        }
        public ConsoleOutput HandleConsoleLine(string? input)
        {
            var output = new ConsoleOutput();


            if (string.IsNullOrEmpty(input))
                return output;
            output = ProcessUserInput(input);

            return output;
        }
        private ConsoleOutput ProcessUserInput(string userInputCommand)
        {
            inputCommand = userInputCommand;
            string fileName = "";
            string operatorString = "";

            if (!inputCommand.StartsWith("history"))
            {
                var GettingFileNameAndOperator = GettingFileTextAndOperator(inputCommand, fileName, operatorString);
                inputCommand = GettingFileNameAndOperator.Item1;
                fileName = GettingFileNameAndOperator.Item2;
                operatorString = GettingFileNameAndOperator.Item3;
            }
            
            

            splitInputList = inputCommand.Split(' ');

            if (inputCommand.Contains('\"'))
            {
                inputCommand = DealingWithSpaceWhileHasDoubleQoutes(inputCommand);
            }
            string[] commandLineArgs = Array.Empty<string>();


            switch (CheckValidCommandExist(splitInputList, validCommandsList))
            {
                case "echo":
                    return EchoCommand(inputCommand, fileName, operatorString);
                    break;
                case "exit":
                    ExitCommand(splitInputList, inputCommand);
                    return new ConsoleOutput();
                    break;
                case "type":
                    var output = new ConsoleOutput();
                    return TypeBuiltCommand(splitInputList, validCommandsList, splitInputList[1], commandLineArgs, fileName, operatorString, splitPathList);
                    break;
                case "pwd":
                    return new ConsoleOutput { output = PrintWorkingDirectory(splitInputList, validCommandsList) };
                    break;
                case "cd":
                    return new ConsoleOutput { output = ChangeDirectory(splitInputList, validCommandsList) };
                    break;

                case "history":
                    
                    return historyCommand(splitInputList);
                    break;
                default:

                    break;
            }
            commandLineArgs = ParsingInput(inputCommand, fileName);
            if (commandLineArgs[0] == "")
            {
                return new ConsoleOutput();
            }

            splitInputList = commandLineArgs;

            if (!validCommandsList.Contains(splitInputList[0]) && splitInputList.Length > 1)
            {
                var output = new ConsoleOutput();
                return output = ExecutesFileIfMeetRequirements(splitInputList[0], commandLineArgs, inputCommand, fileName, operatorString, splitPathList);
            }

            return new ConsoleOutput { output = $"{splitInputList[0]}: command not found" };
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

        private ConsoleOutput EchoCommand(string inputCommand, string fileString, string operatorChar)
        {

            // Remove "echo "
            if (inputCommand.TrimStart().Length == 4)
            {
                return new ConsoleOutput();
            }
            string args = inputCommand.Substring(5).TrimStart();

            // Using stringbuilder because it creates a mutable string that can be changed and doesnt create a new one
            // Because the code will check char by char the string and append the char or not
            StringBuilder result = new StringBuilder();
            bool inQuotes = false;
            char quoteChar = '\0';

            for (int i = 0; i < args.Length; i++)
            {
                char c = args[i];
                // "hello's" "hello"
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
                // BackSpace handling
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
            if (!string.IsNullOrEmpty(fileString) && operatorChar.Contains("2"))
            {
                var ConsoleOut = new ConsoleOutput();
                var possibleErrorResult = OutputToFile(fileString, result.ToString().Trim(), operatorChar);

            }

            if (!string.IsNullOrEmpty(fileString) && operatorChar.Contains("1"))
            {
                var ConsoleOut = new ConsoleOutput();
                var possibleOutputResult = OutputToFile(fileString, result.ToString().Trim(), operatorChar);

                return ConsoleOut;

            }
            var ConsoleOutput = new ConsoleOutput { output = result.ToString().Trim() };

            return ConsoleOutput;

        }

        private ConsoleOutput TypeBuiltCommand(string[] splitInputList, List<string> validCommandsList, string nameOfFile, string[] commandLineArgs, string fileString, string operatorString, List<string> splitPathList)
        {

            if (splitInputList[0] == "type")
            {

                bool wordCheckerIsPath = false;

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
                                // will need to check on this again
                                if ((mode & UnixFileMode.UserExecute) != 0 ||
                                    (mode & UnixFileMode.GroupExecute) != 0 ||
                                    (mode & UnixFileMode.OtherExecute) != 0)
                                {
                                    if (splitInputList[0] == "type")
                                    {
                                        return new ConsoleOutput { output = $"{nameOfFile} is {changedWord}" };
                                    }
                                    else
                                    {
                                        string arguments = string.Join(" ", splitInputList.Skip(1));
                                        return ExecutesFileIfMeetRequirements(nameOfFile, splitInputList, inputCommand, fileString, operatorString, splitPathList);
                                    }
                                }
                            }

                            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                            {
                                if (splitInputList[0] == "type")
                                {
                                    return new ConsoleOutput { output = $"{nameOfFile} is {changedWord}" };
                                }
                                else
                                {
                                    string arguments = string.Join(" ", splitInputList.Skip(1));
                                    return ExecutesFileIfMeetRequirements(changedWord, splitInputList, inputCommand, fileString, operatorString, splitPathList);
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
                        return new ConsoleOutput { output = $"{splitInputList[1]} is a shell builtin" };
                    }
                    else
                    {
                        return new ConsoleOutput { output = $"{splitInputList[1]}: not found" };
                    }
                }

                if (splitInputList[0] == "type" && splitInputList.Count() > 2 && !File.Exists(changedWord))
                {
                    return new ConsoleOutput { output = $"{splitInputList[1]}: not found" };
                }
            }

            return new ConsoleOutput { output = "" };

        }

        private ConsoleOutput ExecutesFileIfMeetRequirements(string nameOfFile, string[] splitInputList, string parsedInput, string fileString, string operatorChar, List<string> splitPathList)
        {
            string executable = splitInputList[0];

            string changedWord = "";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                foreach (string directoryString in splitPathList)
                {
                    if (!Directory.Exists(directoryString))
                    {
                        continue;
                    }

                    changedWord = Path.Join(directoryString, executable);

                }
                if (!string.IsNullOrEmpty(changedWord))
                {
                    executable = changedWord;
                }

            }

            if (nameOfFile == "cat")
            {
                // Check common locations for cat on linux
                splitPathList.Add("/bin/cat");
                splitPathList.Add("/usr/bin/cat");
                splitPathList.Add("cat");
                //string[] possiblePaths = { "/bin/cat", "/usr/bin/cat", "cat" };
                bool found = false;

                foreach (string path in splitPathList)
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

                    return new ConsoleOutput { output = "cat: command not found" };
                }
            }
            if (parsedInput.StartsWith("ls"))
            {
                executable = "ls";
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

                if (parsedInput.Contains("ls"))
                {
                    for (int i = 1; i < splitInputList.Length; i++)
                    {
                        processStartInfo.ArgumentList.Add(splitInputList[i]);
                    }
                }

                else
                {
                    for (int i = 1; i < splitInputList.Length; i++)
                    {
                        processStartInfo.ArgumentList.Add(splitInputList[i]);
                    }

                }
                var process = Process.Start(processStartInfo);

                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                process.WaitForExit();

                // FIX CAT ERROR FORMAT
                if (!string.IsNullOrEmpty(error) && nameOfFile == "cat" || executable.Contains("cat"))
                {
                    error = FixCatErrorMessage(error);
                }


                if (!string.IsNullOrEmpty(error) && operatorChar.Contains("2"))
                {

                    // checking if ls was typed so it would not make empty file string and then still print the error 
                    if (operatorChar.Contains("2") && !string.IsNullOrEmpty(fileString))
                    {
                        OutputToFile(fileString, error, operatorChar);
                        // if its sderr we still need to give output to main because there can a mix
                        // one file with cat works the other doesnt.
                        return new ConsoleOutput { HasError = false, output = output.Trim() };
                    }
                    if (!string.IsNullOrEmpty(output) && !string.IsNullOrEmpty(fileString))
                    {

                        OutputToFile(fileString, output, operatorChar);
                        if (operatorChar.Contains("2"))
                        {
                            return new ConsoleOutput { HasError = false, output = output };
                        }
                        return new ConsoleOutput { error = error, HasError = true };

                    }

                    return new ConsoleOutput { error = error.Trim(), HasError = true };
                }

                if (!string.IsNullOrEmpty(fileString) && operatorChar.Contains("1"))
                {

                    OutputToFile(fileString, output.Trim(), operatorChar);
                    // same with stdout there can still be an error thats why we need to give the error back
                    return new ConsoleOutput { HasError = true, output = error.Trim() };
                }

                return new ConsoleOutput { output = output.Trim() };
            }
            catch (Exception ex)
            {

                return new ConsoleOutput { error = $"{nameOfFile}: error executing - {ex.Message}", HasError = true };

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

        public string[] ParsingInput(string input, string fileString)
        {

            List<string> listForArgs = new List<string>();
            if (input[0] == '\"' && input[^1] == '\"')
            {
                string inputCheckForBlank = input.Substring(1);
                bool blank = false;
                inputCheckForBlank = inputCheckForBlank.Remove(inputCheckForBlank.Length - 1);
                foreach (char item in inputCheckForBlank)
                {
                    blank = item != ' ';

                }
                if (blank)
                {
                    return new string[] { "" };
                }
            }

            int firstSpace = input.IndexOf(' ');

            if (firstSpace > -1)
            {

                int iterator = 0;
                char firstChar = input[iterator];
                char currentChar = ' ';
                string commandName = "";
                string args = "";
                if (firstChar == '\"')
                {
                    currentChar = firstChar;


                    int tokenStart = iterator + 1;
                    char nextChar = input[tokenStart];
                    char prevChar = input[tokenStart - 1];
                    while (nextChar != '\"' || (prevChar == '\\') || (nextChar == '\''))
                    {
                        tokenStart++;
                        nextChar = input[tokenStart];
                        prevChar = input[tokenStart - 1];

                    }
                    int length = tokenStart;
                    char lastChar = input[tokenStart];

                    commandName = input.Substring(iterator + 1, length - 1);
                    listForArgs.Add(commandName);

                    args = input.Remove(iterator, length + 2);

                }

                if (firstChar == '\'')
                {
                    currentChar = firstChar;


                    int tokenStart = iterator + 1;
                    char nextChar = input[tokenStart];
                    char prevChar = input[tokenStart - 1];
                    while (nextChar != '\'' || (prevChar == '\\') || (nextChar == '\"'))
                    {
                        tokenStart++;
                        nextChar = input[tokenStart];
                        prevChar = input[tokenStart - 1];

                    }
                    int length = tokenStart;
                    char lastChar = input[tokenStart];

                    commandName = input.Substring(iterator + 1, length - 1);
                    listForArgs.Add(commandName);

                    args = input.Remove(iterator, length + 2);

                }

                if (firstChar != '\"' && firstChar != '\'')
                {
                    commandName = input.Substring(0, input.IndexOf(' '));


                    listForArgs.Add(commandName);

                    args = input.Substring(input.IndexOf(' ') + 1);

                }


                if (input.Contains("\'") || input.Contains("\"") && input.Contains('\\'))
                {

                    StringBuilder result = new StringBuilder();
                    bool inQuotes = false;
                    char quoteChar = '\0';

                    for (int i = 0; i < args.Length; i++)
                    {
                        char c = args[i];



                        if (!input.Contains("echo"))
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

                            // not in quotes

                            if (nextChar == '\\' || nextChar == ' ' || nextChar == '\'' || nextChar == '"')
                            {
                                result.Append(nextChar);  // Keep the escaped character
                                i++;
                                continue;
                            }

                            // For any other character after backslash, keep backslash as literal
                            result.Append(c);
                        }


                        result.Append(c);

                    }
                    if (result.Length > 0)
                    {
                        listForArgs.Add(result.ToString());
                    }

                }

                else
                {

                    if (args.Contains('\\'))
                    {

                        if (firstSpace > -1)
                        {
                            commandName = input.Substring(0, input.IndexOf(' '));

                            args = input.Substring(input.IndexOf(' ') + 1);

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

                        return fullArray;
                    }

                }

                return listForArgs.ToArray();
            }

            return new string[] { input };

        }
        public string DealingWithSpaceWhileHasDoubleQoutes(string input)
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

            return input;
        }
        const string CatCantOpenError = "cat: can't open '";
        const string noSuchFileOrDirError = "No such file or directory";

        public ConsoleOutput OutputToFile(string fileString, string result, string operatorChar)
        {

            if (!string.IsNullOrEmpty(fileString))
            {
                if (!string.IsNullOrEmpty(result))
                {
                    result += Environment.NewLine;
                }


                // for sterr
                if (operatorChar.Contains("2"))
                {
                    // if error is result
                    if (result.Contains(noSuchFileOrDirError) ||
                         result.Contains("command not found"))
                    {
                        // write if the error message
                        if (operatorChar == "2>")
                        {
                            // add to file 
                            File.AppendAllText(fileString, result);
                        }
                        else
                        {
                            File.WriteAllText(fileString, result);
                        }

                    }
                    else
                    {
                        //if not just create the file if there is none
                        File.WriteAllText(fileString, "");
                    }
                    return new ConsoleOutput { HasError = false };
                }

                // for stdout

                if (operatorChar.Contains("1"))
                {
                    // output
                    if (operatorChar == "1>" && !string.IsNullOrEmpty(result))
                    {
                        // add to file 
                        File.AppendAllText(fileString, result);

                    }
                    if (operatorChar == "1" && !string.IsNullOrEmpty(result))
                    {
                        File.WriteAllText(fileString, result);
                    }

                    if (string.IsNullOrEmpty(result) || result.Contains("nonexistent"))
                    {
                        //if not just create the file if there is none
                        File.WriteAllText(fileString, "");
                    }

                }
            }
            return new ConsoleOutput();

        }

        private string FixCatErrorMessage(string error)
        {
            // Transform: "cat: can't open 'filename': No such file or directory"
            // To: "cat: filename: No such file or directory"

            if (error.StartsWith(CatCantOpenError) && error.Contains($"': {noSuchFileOrDirError}"))
            {
                int start = CatCantOpenError.Length;
                int quoteIndex = error.IndexOf("': ", start);

                if (quoteIndex > start)
                {
                    string filename = error.Substring(start, quoteIndex - start);
                    return $"cat: {filename}: {noSuchFileOrDirError}";
                }
            }

            return error;
        }

        private (string, string, string) GettingFileTextAndOperator(string input, string fileName, string operatorChar)
        {
            int inputIndex = int.MinValue;

            if (input.Contains('>'))
            {
                // checks if > exist if it does 
                // saves the place where it first finds the >
                inputIndex = input.IndexOf('>');
                //checks > is not the last char so that it wouldnt go out of bounds
                if (input.Length > inputIndex + 1)
                {
                    if (!string.IsNullOrEmpty(input) && input[inputIndex + 1] == ' ' && !input.Contains("2>") && !input.Contains(">>"))
                    {
                        // Checks backwards in a string if there is a > with spaces around
                        // example: echo asd asd> does not match
                        // example: echo asd >asd does not match
                        // example: echo asd > asd.txt       matches
                        // only should match if its the redirect operactor

                        string fixedInput = Regex.Replace(input, @"(?<!\S)>\s", "1> ");

                        int newMadeIndex = fixedInput.IndexOf("1>");
                        operatorChar += fixedInput[newMadeIndex];
                       
                        fileName = fixedInput.Substring(newMadeIndex + 2).TrimStart();

                        input = fixedInput.Substring(0, newMadeIndex).Trim();
                    }
                    else if (!input.Contains(">>"))
                    {
                        operatorChar += input[input.IndexOf('>') - 1];
                        fileName = input.Substring(inputIndex + 2).TrimStart();
                        input = input.Substring(0, inputIndex - 1).Trim();
                    }

                    if (input.Contains(">>") && input[inputIndex + 2] == ' ' && !input.Contains("2>"))
                    {
                        string fixedInput = Regex.Replace(input, @"(?<!\S)>>\s", "1>> ");
                        int newMadeIndex = fixedInput.IndexOf("1>>");
                        if (newMadeIndex - 1 > 0)
                        {
                            operatorChar += fixedInput[newMadeIndex];
                            operatorChar += fixedInput[newMadeIndex + 1];
                        }
                        fileName = input.Substring(inputIndex + 2).TrimStart();
                        input = input.Substring(0, newMadeIndex).Trim();
                    }

                    if (input.Contains("2>>"))
                    {
                        operatorChar += input[input.IndexOf(">>") - 1];
                        operatorChar += input[input.IndexOf(">>")];
                        fileName = input.Substring(inputIndex + 2).TrimStart();
                        input = input.Substring(0, inputIndex - 1).Trim();
                    }

                }

                else
                {
                    return (input, "", "");
                }

                return (input, fileName, operatorChar);

            }

            return (input, "", "");

        }
        public ConsoleOutput historyCommand(string[] splitInputList)
        {
            if(splitInputList.Length == 1)
            {
                string histFilePath = Environment.GetEnvironmentVariable("HISTFILE");
                if (!string.IsNullOrEmpty(histFilePath))
                {
                    string[] readText = File.ReadAllLines(histFilePath);
                    foreach (string s in readText)
                    {
                        if (!string.IsNullOrEmpty(s))
                        {

                            inputLines.Add(s);
                        }
                    }
                    if (inputLines[0] == "history")
                    {
                        inputLines.RemoveAt(0);
                    }
                    if (inputLines[^1] != "history")
                    {
                        inputLines.Add("history");
                    }
                        

                }
                
                return new ConsoleOutput { history = inputLines, limitHistory = 0, showHistory = true};
            }
            
            if(splitInputList.Length > 2)
            {
                string inputOfFullFilePath = splitInputList[2];
                if (splitInputList[1] == "-r")
                {
                    
                    if (File.Exists(inputOfFullFilePath))
                    {
                        string[] readText = File.ReadAllLines(inputOfFullFilePath);
                        foreach (string s in readText)
                        {
                            if (!string.IsNullOrEmpty(s))
                            {

                                inputLines.Add(s);
                            }
                        }
                    }
                    return new ConsoleOutput { history = inputLines, flag = splitInputList[1], showHistory = false };
                }

                if (splitInputList[1] == "-w")
                {
                    if (!File.Exists(inputOfFullFilePath))
                    {
                        
                        File.WriteAllLines(inputOfFullFilePath, inputLines);
                    }

                    else
                    {
                        File.AppendAllLines(inputOfFullFilePath, inputLines);
                    }
                }

                if (splitInputList[1] == "-a")
                {
                    // checks if its a first time that history -a was used
                    if (flagACount == 0)
                    {
                        File.AppendAllLines(inputOfFullFilePath, inputLines);
                        
                        flagACount = 1;
                        inputLinesThatRan = new List<string>();
                    }
                    else
                    {
                        inputLinesThatRan.Add(string.Join(' ', splitInputList));
                        File.AppendAllLines(inputOfFullFilePath, inputLinesThatRan);
                    }

                }
            }
            if (splitInputList.Length == 2)
            {
                return new ConsoleOutput { history = inputLines, limitHistory = Convert.ToInt32(splitInputList[1]), showHistory = true };
            }
            return new ConsoleOutput();
        }

    }

}

