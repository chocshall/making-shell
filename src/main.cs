using System;
using System.Diagnostics;
using System.Formats.Tar;
using System.IO;
using System.IO.Enumeration;
using System.Linq;
using System.Net.NetworkInformation;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

class Program
{
    static void Main()
    {




        List<string> validCommandsList = new List<string>();
        validCommandsList.Add("exit");
        validCommandsList.Add("echo");
        validCommandsList.Add("type");
        validCommandsList.Add("pwd");
        validCommandsList.Add("cd");
        string inputCommand = "";
        string[] splitInputList = Array.Empty<string>();






        while (true)
        {
            Console.Write("$ ");


            userInput(inputCommand, validCommandsList, splitInputList);




        }


    }

    static void userInput(string inputCommand, List<string> validCommandsList, string[] splitInputList)
    {

        inputCommand = Console.ReadLine();




        string pattern = "echo.+";

        Regex regularExpressionObject = new Regex(pattern, RegexOptions.IgnoreCase);
        Match checkingMatch = regularExpressionObject.Match(inputCommand);



        if (!checkingMatch.Success)
        {
            if (inputCommand.Contains('\''))
            {
                

                SingleQuotes(inputCommand, ref splitInputList);
                //Console.WriteLine(splitInputList[1]);
            }
            else
            {
                splitInputList = inputCommand.Split(' ');
            }



        }
        else
        {
            echoCommand(inputCommand);
            return;


        }




        bool checker = false;

        if (splitInputList.Length == 1 && !validCommandsList.Contains(inputCommand))
        {
            Console.Error.WriteLine(inputCommand + ": command not found");
            return;
        }
        // _exe neveikia taip kaip per windwos linux 
        //if (splitInputList[0].Contains(".exe") && splitInputList.Length > 1 || (splitInputList[0].Contains("_exe") && splitInputList.Length > 1) || (splitInputList[0].Contains("cat") && splitInputList.Length > 1))
        if(!validCommandsList.Contains(inputCommand) && splitInputList.Length > 1 )
        {
            //checker = true;


            typeBuiltCommand(splitInputList, validCommandsList, splitInputList[0]);
            return;
        }

        if (splitInputList[0] == "pwd" && splitInputList.Count() == 1)
        {
            printWorkingDirectory(splitInputList, validCommandsList);
        }

        if (splitInputList[0] == "exit" && splitInputList.Count() == 1)
        {
            exitCommand(splitInputList, inputCommand);
        }



        if (splitInputList.Count() > 1 && CheckDoesCommandExist(splitInputList, inputCommand, validCommandsList) && !checker)
        {
            exitCommand(splitInputList, inputCommand);

          

            // checking the second input
            typeBuiltCommand(splitInputList, validCommandsList, splitInputList[1]);

            if (splitInputList[0] == "cd")
            {
                changeDirectory(splitInputList, validCommandsList);
            }

        }


    }

    public static bool CheckDoesCommandExist(string[] splitInputList, string inputCommand, List<string> validCommandsList)
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
            Console.Error.WriteLine(inputCommand + ": not found");

            return false;
        }

        if ((splitInputList[0].Contains("_exe") && splitInputList.Length > 1) || (splitInputList[0].Contains(".exe") && splitInputList.Length > 1) || splitInputList[0].Contains("cat") && splitInputList.Length > 1)
        {
            return true;
        }

        Console.Error.WriteLine(inputCommand + ": command not found");

        return false;
    }


    static void exitCommand(string[] splitInputList, string inputCommand)
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


    // disable special meaning of single quates make it so that everything in single quates is treated literraly
    // this echo not how it suppose to be basically read again what it need
    static void echoCommand(string inputCommand)
    {
        string[] splitInputList = Array.Empty<string>();
        /// removes the echo from print
        inputCommand = inputCommand.Remove(0, 5);

        if (inputCommand.StartsWith('\'') || inputCommand.EndsWith('\''))
        {
            splitInputList = inputCommand.Split('\'');
            string result = string.Join("", splitInputList);
            Console.WriteLine(result);
            //foreach (string item in splitInputList)
            //{

            //   Console.WriteLine(item);

            //}


            //inputCommand = inputCommand.Remove(0,1);
            //inputCommand = inputCommand.TrimEnd('\'');
            //if (inputCommand.Contains('\''))
            //{
            //    string[] splitInputList = Array.Empty<string>();
            //    inputCommand = inputCommand.Replace('\'', ' ');

            //    splitInputList = inputCommand.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            //    inputCommand = string.Join("", splitInputList);

            //}
            //Console.WriteLine(inputCommand);
            return;
        }



        if (!inputCommand.StartsWith('\'') && !inputCommand.EndsWith('\''))
        {

            splitInputList = inputCommand.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            string joinedItem = string.Join(" ", splitInputList);

            if (joinedItem.Contains('\''))
            {
                joinedItem = joinedItem.Replace('\'', ' ');

                splitInputList = joinedItem.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                joinedItem = string.Join("", splitInputList);

            }

            Console.WriteLine(joinedItem);

            return;
        }





    }
    // todo
    // pastaisyti kad jeigu neieni i commanda tada tirkini executable ar yra
    static void typeBuiltCommand(string[] splitInputList, List<string> validCommandsList, string nameOfFile)
    {
        

        if (splitInputList[0] == "type" || splitInputList[0].Contains(".exe") || splitInputList[0].Contains("_exe"))
        {
            string[] splitPathList = Array.Empty<string>();

            // if the left is null use the right;
            string pathListString = Environment.GetEnvironmentVariable("PATH") ?? "";


            // used for getting a check if there exist atleast one full path
            bool wordCheckerIsPath = false;

            if (false)
            {
                splitPathList = pathListString.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries);
            }

            else
            {
                //string userInput = $"E:\\Downloads\\testfolder{Path.PathSeparator}E:\\Downloads\\onedollar{Path.PathSeparator}/usr/local/bin{Path.PathSeparator}$PATH";
                string userInput = $@"E:\Downloads\c#programs\TestingProccesClass\bin\Debug\net8.0{Path.PathSeparator}$PATH";


                // path variants to check
                string expandedInput = userInput
                    .Replace("$PATH", pathListString)
                    .Replace("${PATH}", pathListString)
                    .Replace("%PATH%", pathListString);
                splitPathList = expandedInput.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries);

            }




            string changedWord = "";


            if (!validCommandsList.Contains(nameOfFile))
            {
                foreach (string directoryString in splitPathList)
                {

                    // skip the not existing directories
                    if (!Directory.Exists(directoryString))
                    {
                        continue;
                    }

                    // make the full path
                    changedWord = Path.Join(directoryString, nameOfFile);


                    if (File.Exists(changedWord))
                    {

                        wordCheckerIsPath = true;

                        // checks on linux if the program is executable because file exists is not enoguth to check
                        // thats why there was a problem with finding a file in a folder that you didnt have permis and printed

                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                        {

                            var mode = File.GetUnixFileMode(changedWord);
                            if ((mode & UnixFileMode.UserExecute) != 0 ||
                            (mode & UnixFileMode.GroupExecute) != 0 ||
                            (mode & UnixFileMode.OtherExecute) != 0)
                            {
                                //05
                                //005
                                //101


                                //01
                                //1000
                                //1001001

                                //1000000


                                if (splitInputList[0] == "type")
                                {
                                    Console.WriteLine(nameOfFile + " is " + changedWord);
                                }
                                else
                                {
                                    string arguments = string.Join(" ", splitInputList.Skip(1));
                                    executesFileIfMeetRequirements(nameOfFile, splitInputList);
                                }


                            }



                        }



                        if (Path.PathSeparator == ';')
                        {
                            if (splitInputList[0] == "type")
                            {
                                Console.WriteLine(nameOfFile + " is " + changedWord);
                                break;
                            }

                            else
                            {
                                // in requirements it should be only filename given, but because of how i placed my downloads need full path to work, when testing locally.
                                string arguments = string.Join(" ", splitInputList.Skip(1));
                                executesFileIfMeetRequirements(changedWord, splitInputList);



                            }



                        }


                    }

                }
            }

            // checks if second word after type is valid if not print not found
            if (splitInputList[0] == "type" && !wordCheckerIsPath)
            {
                // checks the second string given does it exist in commands
                CheckDoesCommandExist(splitInputList, splitInputList[1], validCommandsList);
            }



            if (validCommandsList.Contains(splitInputList[1]) && splitInputList.Count() == 2)
            {
                Console.WriteLine(splitInputList[1] + " is a shell builtin");
            }


            if (splitInputList[0] == "type" && splitInputList.Count() > 2 && !File.Exists(changedWord))
            {
                Console.Error.WriteLine(splitInputList[1] + ": not found");
            }

            

        }

        if (splitInputList[0].Contains("cat"))
        {
            executesFileIfMeetRequirements(splitInputList[0], splitInputList);
        }



    }

    static void executesFileIfMeetRequirements(string nameOfFile, string[] splitInputList)
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
                
            }
        }
        
        if (!found)
        {
            Console.Error.WriteLine("cat: command not found");
            return;
        }
    }
        var processStartInfo = new ProcessStartInfo
        {
            FileName = executable,
            UseShellExecute = false
        };

        foreach (string item in splitInputList.Skip(1))
        {
            processStartInfo.ArgumentList.Add(item);
        }

        var process = Process.Start(processStartInfo);

        process.WaitForExit();
    }


    static void printWorkingDirectory(string[] splitInputList, List<string> validCommandsList)
    {
        if (splitInputList[0] == "pwd" && splitInputList.Count() == 1)
        {
            string pathWorkingDirectory = Directory.GetCurrentDirectory();
            Console.WriteLine(pathWorkingDirectory);
        }



    }

    static void changeDirectory(string[] splitInputList, List<string> validCommandsList)
    {
        if (splitInputList[0] == "cd" && splitInputList[1] == "~")
        {
            
            string homePath = (Environment.OSVersion.Platform == PlatformID.Unix ||
                                Environment.OSVersion.Platform == PlatformID.MacOSX)
                                ? Environment.GetEnvironmentVariable("HOME")
                                : Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");

            Directory.SetCurrentDirectory(homePath);
            return;
        }

        if (splitInputList[0] == "cd" && Directory.Exists(splitInputList[1]))
        {
            
            Directory.SetCurrentDirectory(splitInputList[1]);
            return;
        }

        if (splitInputList[0] == "cd" && !Directory.Exists(splitInputList[1]))
        {

            Console.WriteLine($"cd: {splitInputList[1]}: No such file or directory");
        }


    }


    static void SingleQuotes(string input, ref string[] inputlist)
    {


        string[] splitInputList = input.Split('\'');
        List<string> result = new List<string>();

        foreach (string s in splitInputList)
        {
            // Use Trim() and check for non-empty strings
            string trimmed = s.Trim();
            // checks if blank or nothing
            if (!string.IsNullOrEmpty(trimmed))
            {
                result.Add(trimmed);
            }
        }

        inputlist = result.ToArray();
    }
}

            


























