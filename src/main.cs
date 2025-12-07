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
                string result = Maker.HandleConsoleLine(Console.ReadLine());

               
                if (!string.IsNullOrEmpty(result))
                {
                    Console.WriteLine(result);
                }
            }
        }
    }
}

