

using src;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;



public class ConsoleManager
{
    private List<string> validCommandsList;
    private string inputCommand;
    private string[] splitInputList;

    // string extraPath galima ideti i public () ir padaryti kitaip localiai
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

        //this.extraPath = extraPath;
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
        string fileString = "";
        char operatorString = ' ';

        GettingFileTextAndOperator(ref fileString, ref inputCommand, ref operatorString);
        

        splitInputList = inputCommand.Split(' ');

        if (inputCommand.Contains('\"') )
        {
            inputCommand = DealingWithSpaceWhileHasQoutes(inputCommand);
        }
        string[] commandLineArgs = Array.Empty<string>();
       
        

        switch (CheckValidCommandExist(splitInputList,validCommandsList))
        {
            case "echo":
                return EchoCommand(inputCommand,  fileString, operatorString);
                break;
            case "exit":
                ExitCommand(splitInputList, inputCommand);
                return new ConsoleOutput();
                break;
            case "type":
                return new ConsoleOutput { output = TypeBuiltCommand(splitInputList, validCommandsList, splitInputList[1], commandLineArgs, fileString, operatorString) };
                break;
            case "pwd":
                return new ConsoleOutput { output = PrintWorkingDirectory(splitInputList, validCommandsList) };
                break;
            case "cd":
                return new ConsoleOutput { output = ChangeDirectory(splitInputList, validCommandsList) };
                break;
            default:
                
                break;
        }
        commandLineArgs = ParsingInput(inputCommand, ref fileString);
        if (commandLineArgs[0] == "")
        {
            return new ConsoleOutput();
        }
        
        splitInputList = commandLineArgs;




        if (!validCommandsList.Contains(splitInputList[0]) && splitInputList.Length > 1)
        {
            return new ConsoleOutput { output = ExecutesFileIfMeetRequirements(splitInputList[0], commandLineArgs, inputCommand, fileString, operatorString) };
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

    private ConsoleOutput EchoCommand(string inputCommand,  string fileString, char operatorString)
    {
        
        // Remove "echo "
        if(inputCommand.TrimStart().Length == 4)
        {
            return new ConsoleOutput();
        }
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
        if(!string.IsNullOrEmpty(fileString) && operatorString == '1')
        {
            var ConsoleOut = new ConsoleOutput();
            var possibleErrorResult = OutPutToFile(fileString, result.ToString().Trim());
            if (string.IsNullOrEmpty(possibleErrorResult))
            {
                return ConsoleOut;
            }
            ConsoleOut.error = OutPutToFile(fileString, result.ToString().Trim());
            ConsoleOut.HasError = true;
            return ConsoleOut;


        }
        var ConsoleOutput = new ConsoleOutput { output = result.ToString().Trim() };
        

        return ConsoleOutput;

    }

    private string TypeBuiltCommand(string[] splitInputList, List<string> validCommandsList, string nameOfFile, string[] commandLineArgs, string fileString, char operatorString)
    {
        

        if (splitInputList[0] == "type")
        {
            string[] splitPathList = Array.Empty<string>();
            string pathListString = Environment.GetEnvironmentVariable("PATH") ?? "";
            
            bool wordCheckerIsPath = false;
            string userInput = "";


#if DEBUG
            pathListString = $@"C:\cSharp\ConsoleApp1\bin\Debug\net9.0{Path.PathSeparator}" + pathListString;

#else
    
    
#endif
            
            
            splitPathList = pathListString.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries);
            

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
                                    return ExecutesFileIfMeetRequirements(nameOfFile, splitInputList, inputCommand, fileString, operatorString);
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
                                return ExecutesFileIfMeetRequirements(changedWord, splitInputList, inputCommand, fileString, operatorString);
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

    private string ExecutesFileIfMeetRequirements(string nameOfFile, string[] splitInputList, string parsedInput, string fileString, char operatorString)
    {
        string executable = splitInputList[0];

#if DEBUG
        string currentPath = Environment.GetEnvironmentVariable("PATH");
        Environment.SetEnvironmentVariable("PATH", currentPath + ";" + "C:\\cSharp\\ConsoleApp1\\bin\\Debug\\net9.0");

#else
    
    
#endif

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

            
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            process.WaitForExit();

            if (!string.IsNullOrEmpty(error))
            {
                // FIX CAT ERROR FORMAT
                if (nameOfFile == "cat" || executable.Contains("cat"))
                {
                    error = FixCatErrorMessage(error);
                }

                if (!string.IsNullOrEmpty(output) && !string.IsNullOrEmpty(fileString)  && operatorString == '1')
                {
                    var returnedString = OutPutToFile(fileString, output);
                    return error;

                }
                
                return error.Trim();
            }
            
            if (!string.IsNullOrEmpty(fileString) && operatorString == '1')
            {
                
                OutPutToFile(fileString, output.Trim());
                return "";
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

    public string[] ParsingInput(string input, ref string fileString)
    {
        
        List<string> listForArgs = new List<string>();
        if (input[0] == '\"' && input[^1] == '\"')
        {
            string inputCheckForBlank = input.Substring(1);
            bool blank = false;
            inputCheckForBlank = inputCheckForBlank.Remove(inputCheckForBlank.Length - 1);
            foreach (char item in inputCheckForBlank)
            {
                if (item == ' ')
                {
                    blank = true;
                }
                else
                {
                    blank = false;
                }
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
                char prevChar = input[tokenStart-1];
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

            if(firstChar != '\"' && firstChar != '\'')
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

                int count = 0;
                char oneTimeChar = ' ';

                for (int i = 0; i < args.Length; i++)
                {
                    char c = args[i];

                    

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
    private readonly string extraPath;

    public string OutPutToFile (string fileString, string result)
    {
        if (!string.IsNullOrEmpty(fileString))
        {
            
           
            if (result.Contains(noSuchFileOrDirError) ||
                     result.Contains("command not found"))
            {
                // Just error 
                return result;
                
            }
            else
            {
                // Just output 
                File.WriteAllText(fileString, result);
                
            }
        }
        return "";
        
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
    
   internal void GettingFileTextAndOperator (ref string fileString, ref string input, ref char operatorString)
    {
        // geriau nenaudoti ref ir return list kuriuos nori update jei mazai daylku tada tuple naudoti.!!
        int inputIndex = -9999999;
        if (input.Contains('>'))
        {
            inputIndex = input.IndexOf('>');
            if (input.Length > inputIndex + 1)
            {
                
                
                if (!string.IsNullOrEmpty(input) && input[inputIndex + 1] == ' ')
                {
                    
                    // Checks backwards in a string if there is a > with spaces around
                    // example: echo asd asd> does not match
                    // example: echo asd >asd does not match
                    // example: echo asd > asd.txt       matches
                    // only should match if its the redirect operactor
                    string fixedInput = Regex.Replace(input, @"(?<!\S)>\s", "1> ");
                    
                    //Console.WriteLine(fixedInput);
                    int index = fixedInput.IndexOf("1>");
                    if (index - 1 > 0)
                    {
                        operatorString = fixedInput[index];
                    }

                    fileString = fixedInput.Substring(index + 2).TrimStart();
                    
                    input = fixedInput.Substring(0, index).Trim();
                    
                    //Console.WriteLine(input + " wozw");
                }
                inputIndex = input.IndexOf('>');
                
                
                //Console.WriteLine(operatorString);
                





            }
            
            


        }
    }


}



