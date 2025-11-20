using System.Diagnostics;
using System.Formats.Tar;
using System.IO;
using System.Net.NetworkInformation;
using System.Xml.Linq;
using System;

class Program
{
    static void Main()
    {
        

        
       
        List<string> validCommandsList = new List<string>();
        validCommandsList.Add("exit");
        validCommandsList.Add("echo");
        validCommandsList.Add("type");

        string inputCommand = "";
        string[] splitInputList =  Array.Empty<string>();


        while (true)
        {
            Console.Write("$ ");
            userInput( inputCommand,  validCommandsList, splitInputList);





        }


    }

    static void userInput(string inputCommand, List<string> validCommandsList, string[] splitInputList)
    {
        
        inputCommand = Console.ReadLine();
        splitInputList = inputCommand.Split(' ');
        
        if (splitInputList[0].Contains(".exe") && splitInputList.Length > 1 || (splitInputList[0].Contains("_exe") && splitInputList.Length > 1))
        {
            
            
            typeBuiltCommand(splitInputList, validCommandsList, splitInputList[0]);
        }
        else
        {
            
            if (splitInputList.Count() > 1 && CheckDoesCommandExist(splitInputList, inputCommand, validCommandsList))
            {
                exitCommand(splitInputList, inputCommand);

                echoCommand(splitInputList, inputCommand);


                typeBuiltCommand(splitInputList, validCommandsList, splitInputList[1]);
            }

            else
            {
                Console.Error.WriteLine(inputCommand + ": command not found ");
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
        
        if (splitInputList[0] == "type")
        {
            Console.Error.WriteLine(inputCommand + ": not found");
           
            return false;
        }

        Console.Error.WriteLine(inputCommand + ": command not found");
        
        return false;
    }

    
    static void exitCommand(string[] splitInputList, string inputCommand)
    {
        if (splitInputList[0] == "exit" )
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

    static void echoCommand(string[] splitInputList, string inputCommand)
    {
        if (splitInputList[0] == "echo")
        {
            foreach (string item in splitInputList.Skip(1))
            {
                Console.Write(item + " ");
            }
            Console.Write("\n");


        }
        
    }

    // custom_exe arg1 arg2
    static void typeBuiltCommand (string[] splitInputList, List<string> validCommandsList, string nameOfFile)
    {
        if (splitInputList[0] == "type" || splitInputList[0].Contains(".exe")|| splitInputList[0].Contains("_exe"))
        {
            string[] splitPathList = Array.Empty<string>();

            // if the left is null use the right;
            string pathListString = Environment.GetEnvironmentVariable("PATH") ?? "";



            bool wordCheckerIsPath = false;

            if (false)
            {
                splitPathList = pathListString.Split(Path.PathSeparator,StringSplitOptions.RemoveEmptyEntries);
            }

            else
            {
                //string userInput = $"E:\\Downloads\\testfolder{Path.PathSeparator}E:\\Downloads\\onedollar{Path.PathSeparator}/usr/local/bin{Path.PathSeparator}$PATH";
                string userInput = $"E:\\Downloads\\c#programs\\HowToPublish{Path.PathSeparator}$PATH";


                // path variants to check
                string expandedInput = userInput
                    .Replace("$PATH", pathListString)
                    .Replace("${PATH}", pathListString)
                    .Replace("%PATH%", pathListString);
                splitPathList = expandedInput.Split(Path.PathSeparator,StringSplitOptions.RemoveEmptyEntries);
                
            }

            
            
            
            string changedWord = "";
            

            if(!validCommandsList.Contains(nameOfFile))
            {
                foreach(string directoryString in splitPathList)
            {
                    //Console.WriteLine(directoryString);
                    // skip the not existing directories
                    if (!Directory.Exists(directoryString))
                    {
                        continue;
                    }

                    // make the full path
                    changedWord = Path.Join(directoryString, nameOfFile);
                   
                    //Console.WriteLine(changedWord);

                    //Console.WriteLine(changedWord + "\n");
                    //Console.WriteLine(directoryString);
                    if (File.Exists(changedWord))
                    {

                        wordCheckerIsPath = true;

                        // checks on linux if the program is executable because file exists is not enoguth to check
                        // thats why there was a problem with finding a file in a folder that you didnt have permis and printed
                        // != 0 mean file is exucatable by someone
                        if (Path.PathSeparator == ':')
                        {

                            var mode = File.GetUnixFileMode(changedWord);
                            if ((mode & UnixFileMode.UserExecute) != 0 ||
                            (mode & UnixFileMode.GroupExecute) != 0 ||
                            (mode & UnixFileMode.OtherExecute) != 0)
                            {

                                if (splitInputList[0] == "type")
                                {
                                    Console.WriteLine(nameOfFile + " is " + changedWord);
                                }
                                else
                                {
                                    string arguments = string.Join(" ", splitInputList.Skip(1));
                                    executesFileIfMeetRequirements(nameOfFile, arguments);
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
                                string arguments = string.Join(" ", splitInputList.Skip(1));
                                executesFileIfMeetRequirements(changedWord, arguments);
                               


                            }
                           
                            
                            //break;
                        }


                    }

                }
            }

            // checks if second word after type is valid if not print not found
            if (splitInputList[0] == "type" && File.Exists(changedWord))
            {
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
    }

    static void executesFileIfMeetRequirements(string nameOfFile, string arguments)
    {
       
        
        var process = Process.Start(nameOfFile, arguments);
        
        process.WaitForExit();
    }


}

