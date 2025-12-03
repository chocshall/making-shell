namespace ConsoleApp2
{
    internal class Program
    {
        static void Main(string[] args)
        {


            ConsoleManager Maker = new ConsoleManager();

             

            while (true)
            {

                Console.Write("$ ");
                string result = Maker.HandleConsoleLine(Console.ReadLine());

               
                if (!string.IsNullOrEmpty(result))
                {
                    Console.WriteLine(result);
                }
            }
        }
    }
}
