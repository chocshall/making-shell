using System.Text.RegularExpressions;

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

                string input = Console.ReadLine();
                
               
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

