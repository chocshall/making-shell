using System.Diagnostics;
using System.Formats.Tar;
using System.IO;
using System.Net.NetworkInformation;
using System.Xml.Linq;

class Program
{
    static void Main()
    {
        //TODO: Uncomment the code below to pass the first stage

        bool checker = false;
       
        List<string> validCommandsList = new List<string>();
        validCommandsList.Add("exit");
        validCommandsList.Add("echo");
        validCommandsList.Add("type");

        string inputCommand = "";
        string[] splitInputList =  Array.Empty<string>();
        //string[] splitInputByCommaList = Array.Empty<string>();

        // nedaryti taip kad paduotu variable jau turi predefined path su unit testing
        // folder kur real parasai pats savo failus kaip cat ar kitus. nenaudojant visu jau dabar automatiskai priimtu.

       

        
        //string pathListString = Environment.GetEnvironmentVariable("PATH");
        //Console.WriteLine(pathListString + "\n");
        //splitInputList = pathListString.Split(';');
        //string findFileString = "cat";
        //string changedWord = "";
        

        //foreach (string directoryString in splitInputList)
        //{

        //    changedWord = Path.Join(directoryString, findFileString);
        //    Console.WriteLine(changedWord + "\n");
        //    if (File.Exists(changedWord))
        //    {
        //        Console.WriteLine("found one " + directoryString);
        //    }
            

        //}


        while (true)
        {
            Console.Write("$ ");

            if (!false)
            {

                inputCommand = Console.ReadLine();
                splitInputList = inputCommand.Split(' ');
                checker = CheckIfStartsWithCommand(splitInputList, inputCommand, validCommandsList);
            }

            else
            {
                
            }

            

            

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
                // for now probably dont need it
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

            string pathListString = Environment.GetEnvironmentVariable("PATH");
            string userInput = "/usr/bin:/usr/local/bin:$PATH";

            // this doesnt really work for testing on windows on how i did it because im no running not linux so it doesnt check : 
            string expandedInput = userInput.Replace("$PATH", pathListString);
            

            //Console.WriteLine(expandedInput);




            splitPathList = expandedInput.Split(Path.PathSeparator);
            
            string findFileString = splitInputList[1];
            string changedWord = "";
            bool wordCheckerIsPath = false;


            foreach (string directoryString in splitPathList.Reverse())
            {

                changedWord = Path.Join(directoryString, findFileString);

                //Console.WriteLine(changedWord + "\n");
                //Console.WriteLine(directoryString);
                if (File.Exists(changedWord))
                {
                    wordCheckerIsPath = true;
                    Console.WriteLine(findFileString + " is " + changedWord);
                    break;
                }
                
            }

            bool secondChecker = false;

            if (!wordCheckerIsPath)
            {
                secondChecker = CheckIfStartsWithCommand(splitInputList, splitInputList[1], validCommandsList);
            }
                
            
            
            
            
            // probably still need this because (type echo example) can be
            
            if (validCommandsList.Contains(splitInputList[1]) && splitInputList.Count() == 2)
            {
                Console.WriteLine(splitInputList[1] + " is a shell builtin");
            }



            //string[] splitPathListString = pathListString.Split(";");

            ////if (File.Exists("page.txt"))
            ////{
            ////    Console.WriteLine("iterator exists ");
            ////}
            ///
            





            if (splitInputList.Count() > 2 && !File.Exists(changedWord))
            {
                Console.Error.WriteLine(splitInputList[1] + ": not found");
            }


        }
    }


}
