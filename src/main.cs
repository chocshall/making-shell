using System.Diagnostics;
using System.Formats.Tar;
using System.IO;
using System.Net.NetworkInformation;
using System.Xml.Linq;

class Program
{
    static void Main()
    {
        

        bool checker = false;
       
        List<string> validCommandsList = new List<string>();
        validCommandsList.Add("exit");
        validCommandsList.Add("echo");
        validCommandsList.Add("type");

        string inputCommand = "";
        string[] splitInputList =  Array.Empty<string>();


        while (true)
        {
            Console.Write("$ ");

           
            inputCommand = Console.ReadLine();
            splitInputList = inputCommand.Split(' ');
            checker = CheckIfStartsWithCommand(splitInputList, inputCommand, validCommandsList);
            

            


            if (checker)
            {
                
                
                

                if(splitInputList.Count() > 1)
                {
                    exitCommand(splitInputList, inputCommand);

                    echoCommand(splitInputList, inputCommand);


                    typeBuiltCommand(splitInputList, validCommandsList);
                }

                else
                {
                    Console.Error.WriteLine(inputCommand + ": command not found");
                }

                


            }

            
        }


    }

    public static bool CheckIfStartsWithCommand(string[] splitInputList, string inputCommand, List<string> validCommandsList)
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
        if (splitInputList[0] == "exit")
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


    static void typeBuiltCommand (string[] splitInputList, List<string> validCommandsList)
    {
        if (splitInputList[0] == "type")
        {
            string[] splitPathList = Array.Empty<string>();

            // if the left is null use the right;
            string pathListString = Environment.GetEnvironmentVariable("PATH") ?? "";

            

           

            if (!false)
            {
                splitPathList = pathListString.Split(Path.PathSeparator,StringSplitOptions.RemoveEmptyEntries);
            }

            else
            {
                //string userInput = $"E:\\Downloads\\testfolder{Path.PathSeparator}E:\\Downloads\\onedollar{Path.PathSeparator}/usr/local/bin{Path.PathSeparator}$PATH";
                string userInput = $"/usr/bin{Path.PathSeparator}/usr/local/bin{Path.PathSeparator}$PATH";


                // path variants to check
                string expandedInput = userInput
                    .Replace("$PATH", pathListString)
                    .Replace("${PATH}", pathListString)
                    .Replace("%PATH%", pathListString);
                splitPathList = expandedInput.Split(Path.PathSeparator,StringSplitOptions.RemoveEmptyEntries);
            }

            
            
            string findFileString = splitInputList[1];
            string changedWord = "";
            bool wordCheckerIsPath = false;

            if(!validCommandsList.Contains(splitInputList[1]))
            {
                foreach(string directoryString in splitPathList)
            {
                    // skip the not existing directories
                    if (!Directory.Exists(directoryString))
                    {
                        continue;
                    }

                    // make the full path
                    changedWord = Path.Join(directoryString, findFileString);

                    //Console.WriteLine(changedWord + "\n");
                    //Console.WriteLine(directoryString);
                    if (File.Exists(changedWord))
                    {
                        wordCheckerIsPath = true;

                        // checks on linux if the program is executable because file exists is not enoguth to check
                        // thats why there was a problem with finding a file in a folder that you didnt have permis and printed
                        // != 0 mean file is exucatable by someone
                        if(Path.PathSeparator == ':')
                        {
                            var mode = File.GetUnixFileMode(changedWord);
                            if ((mode & UnixFileMode.UserExecute) != 0 ||
                            (mode & UnixFileMode.GroupExecute) != 0 ||
                            (mode & UnixFileMode.OtherExecute) != 0)
                            {


                                Console.WriteLine(findFileString + " is " + changedWord);
                                wordCheckerIsPath = true;
                                break;
                            }
                        }

                        if (Path.PathSeparator == ';')
                        {
                            Console.WriteLine(findFileString + " is " + changedWord);
                            wordCheckerIsPath = true;
                            break;
                        }








                    }

                }
            }
            

            bool secondChecker = false;

            if (!wordCheckerIsPath)
            {
                secondChecker = CheckIfStartsWithCommand(splitInputList, splitInputList[1], validCommandsList);
            }
                
            if (validCommandsList.Contains(splitInputList[1]) && splitInputList.Count() == 2)
            {
                Console.WriteLine(splitInputList[1] + " is a shell builtin");
            }


            if (splitInputList.Count() > 2 && !File.Exists(changedWord))
            {
                Console.Error.WriteLine(splitInputList[1] + ": not found");
            }


        }
    }


}


// TODO:  make a unit testing for windows and linux?? or does unit testing check both in one heap.
//  variable  predefined path su unit testing
// folder where files are made cat or others not using predefined.