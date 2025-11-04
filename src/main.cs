class Program
{
    static void Main()
    {
        //TODO: Uncomment the code below to pass the first stage
        

        List<string> validCommandsList = new List<string>();

        while (true)
        {
            Console.Write("$ ");
            string inputCommand = Console.ReadLine();

            if (!validCommandsList.Contains(inputCommand))
            {
                Console.Error.WriteLine(inputCommand + ": command not found");
            }
        }
            

           

           

        
    }
}
