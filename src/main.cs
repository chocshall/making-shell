
namespace src
{
    public class Program
    {
        private const string StartInput = "$ ";
        static void Main(string[] args)
        {

            ConsoleManager Maker = new ConsoleManager();
            bool noMatches = true;
            bool tabPressed = false;
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
                        tabPressed = true;
                        string partialString = input;
                        foreach (var command in Maker.validCommandsList)
                        {
                            if (command.StartsWith(partialString))
                            {
                                input = command + " ";
                                string commandlastChars = command.Substring(partialString.Length);
                                Console.Write(commandlastChars + " ");
                                noMatches = false;
                                break;

                            }
                           
                        }
                        if(noMatches)
                        {
                            try
                            {
                                foreach (var directory in Maker.splitPathList)
                                {
                                    var files = from file in Directory.EnumerateFiles(directory)
                                                where Path.GetFileName(file).ToLower().StartsWith(partialString)
                                                select file;
                                    foreach (string fileName in files)
                                    {
                                        if (File.Exists(fileName))
                                        {
                                            string newFileName = fileName.Substring(directory.Length + 1);
                                            string filelastChars = newFileName.Substring(partialString.Length);
                                            Console.WriteLine(filelastChars + " ");
                                            noMatches = false;
                                            break;
                                        }

                                    }
                                }
                            }
                            catch (UnauthorizedAccessException UAEx)
                            {
                                Console.WriteLine('\x07');
                            }
                            catch (PathTooLongException PathEx)
                            {
                                Console.WriteLine('\x07');
                            }

                            catch ( DirectoryNotFoundException UEX)
                            {
                                Console.WriteLine('\x07');
                            }
                        }

                            


                    }
                    else
                    {
                        input += key.KeyChar;
                        Console.Write(key.KeyChar);
                    }

                    if (noMatches && tabPressed)
                    {
                        Console.WriteLine('\x07');
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

