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
                string fileString = "";
                if (!string.IsNullOrEmpty(input) && input.IndexOf('>') != -1 && input[input.IndexOf('>')+1] == ' ')
                {
                    string fixedInput = Regex.Replace(input, @"(?<!1)>\s", "1> ");
                    //Console.WriteLine(fixedInput);
                    int index = input.IndexOf('>');
                    fileString = input.Substring(index+1).TrimStart();
                    input = input.Substring(0, index-1).Trim();
                }
                string theCheck = input;




                string result = Maker.HandleConsoleLine(input);
                
                

                if (!string.IsNullOrEmpty(result))
                {



                    if (!string.IsNullOrEmpty(fileString))
                    {
                        // Check if result contains both error and output
                        if (result.Contains("|||"))
                        {
                            string[] parts = result.Split(new[] { "|||" }, 2, StringSplitOptions.None);
                            string error = parts[0];
                            string output = parts[1];

                            // Write output to file
                            File.WriteAllText(fileString, output);

                            // Print error
                            Console.WriteLine(error);
                        }
                        else if (result.Contains("No such file or directory") ||
                                 result.Contains("command not found"))
                        {
                            // Just error 
                            Console.WriteLine(result);
                        }
                        else
                        {
                            // Just output 
                            File.WriteAllText(fileString, result);
                        }
                    }
                    else
                    {
                        
                        if (result.Contains("|||"))
                        {
                            result = result.Replace("|||", "\n");
                        }
                        Console.WriteLine(result);
                    }


                }
            }
        }
    }
}

