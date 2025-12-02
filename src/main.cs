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
                Console.WriteLine(Maker.HandleConsoleLine(Console.ReadLine()));
            }
        }
    }
}
