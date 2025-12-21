namespace src
{
    public class Program
    {
        private const string StartInput = "$ ";
        static void Main(string[] args)
        {

            ConsoleManager Maker = new ConsoleManager();
            
            while (true)
            {
                Console.Write(StartInput);

                string input = "";

                while (true)
                {
                    var key = Console.ReadKey(intercept:true);

                    if (key.Key == ConsoleKey.Enter)
                    {
                        Console.WriteLine();
                        break;
                    }
                       
                    if (key.Key == ConsoleKey.Tab && input.Length >= 3)
                    {
                        string partialString = input;
                        foreach (var command in Maker.validCommandsList)
                        {
                            if(command.StartsWith(partialString))
                        {
                                input = command + " ";
                                string commandlastChars = command.Substring(partialString.Length);
                                Console.Write(commandlastChars + " ");
                                
                                break;
          
                            }
                        }
                       
                    }
                    else
                    {
                        input += key.KeyChar;
                        Console.Write(key.KeyChar);
                    }
                }
                
                ConsoleOutput result = Maker.HandleConsoleLine(input);
                
                if (!string.IsNullOrEmpty(result.output))
                {

                    Console.WriteLine(result.output);

                }

                if(result.HasError && !string.IsNullOrEmpty(result.error))
                {
                    Console.Error.WriteLine(result.error);
                    
                }

            }
        }
    }
}

