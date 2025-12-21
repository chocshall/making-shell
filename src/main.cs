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
                                    // when testing locally what u have in path or added sometimes the paths listed are old and they dont get updated when the dirs are deleted on file explorer
                                    if(Directory.Exists(directory))
                                    {
                                        // finds every file in a single dir if accesible
                                        var files = from file in Directory.EnumerateFiles(directory)
                                                    // gets the filename and changes it to lower checks if starts with the partialstring
                                                    where Path.GetFileName(file).ToLower().StartsWith(partialString)
                                                    // if it maches the select return that one saves to files
                                                    select file;
                                        foreach (string fileName in files)
                                        {
                                            
                                            if (File.Exists(fileName))
                                            {
                                                string newFileName = fileName.Substring(directory.Length + 1);
                                                string filelastChars = newFileName.Substring(partialString.Length);
                                                Console.Write(filelastChars + " ");
                                                
                                                noMatches = false;
                                                break;
                                            }

                                        }
                                    }
                                    
                                }
                            }
                            catch (UnauthorizedAccessException UAEx)
                            {
                                //Console.WriteLine(UAEx);
                                Console.WriteLine('\x07');
                            }
                            catch (PathTooLongException PathEx)
                            {
                                //Console.WriteLine(PathEx);
                                Console.WriteLine('\x07');
                            }

                            catch ( DirectoryNotFoundException UEX)
                            {
                                //Console.WriteLine(UEX);
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

