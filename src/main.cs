class Program
{
    static void Main()
    {
        //TODO: Uncomment the code below to pass the first stage
        Console.Write("$ ");

        List<string> validCommandsList = new List<string>();
        
        string inputCommand = Console.ReadLine();

        if(!validCommandsList.Contains(inputCommand))
        {
            Console.Error.WriteLine(inputCommand + ": command not found");
        }
    }
}
