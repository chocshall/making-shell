using System.Net.NetworkInformation;

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

        while (true)
        {
            Console.Write("$ ");
            string inputCommand = Console.ReadLine();
            checker = CheckIfStartsWithCommand(inputCommand, validCommandsList);


            if (checker)
            {
                
                string[] splitInputList = inputCommand.Split(' ');

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

                        if (splitInputList[0] == "exit" && Int32.Parse(splitInputList[1]) > 1)
                        {
                            Console.Error.WriteLine(inputCommand + ": command not found");
                            break;
                        }


                    }
                }
                    
                if (splitInputList[0] == "echo")
                {
                    if (splitInputList.Count() >= 2)
                    {
                        foreach (string item in splitInputList.Skip(1))
                        {
                            Console.Write(item + " ");
                        }
                        Console.Write("\n");
                    }
                    

                     else
                    {
                        Console.Error.WriteLine(inputCommand + ": command not found ");
                    }
                }

                


                if (splitInputList[0] == "type")
                {
                    bool secondChecker = CheckIfStartsWithCommand(splitInputList[1], validCommandsList);

                    if (secondChecker && splitInputList.Count() == 2)
                    {
                        Console.WriteLine(splitInputList[1] + " is a shell builtin");
                    }

                    if (secondChecker && splitInputList.Count() > 2)
                    {
                        Console.Error.WriteLine(splitInputList[1] + ": command not found ");
                    }

                    
                }
                

                    
                
                    




            }

            


        }







    }

    public static bool CheckIfStartsWithCommand(string inputCommand, List<string> validCommandsList)
    {
        foreach (string item in validCommandsList)
        {
            if (inputCommand.StartsWith(item))
            {
                return true;
            }

        }
        Console.Error.WriteLine(inputCommand + ": command not found ");
        return false;
    }


}
