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
            bool noMatchesCommand = false;
            
            while (true)
            {
                Console.Write(StartInput);

                string input = "";
                int tabCount = 0;
                while (true)
                {
                    var key = Console.ReadKey(intercept:true);

                    if (key.Key == ConsoleKey.Enter)
                    {
                        Console.WriteLine();
                        break;
                    }
                    List<string> foundExecutablesList = new List<string>();
                    
                    if (key.Key == ConsoleKey.Tab && input.Length >= 3)
                    {
                        tabCount++; 
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
                                noMatchesCommand = true;
                                break;

                            }
                           
                        }
                        if(!noMatchesCommand)
                        {
                            
                            try
                            {
                                string filelastChars = "";
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
                                                filelastChars = newFileName.Substring(partialString.Length);
                                                foundExecutablesList.Add(partialString + filelastChars);
                                                
                                                //Console.Write(filelastChars + " ");
                                                
                                                noMatches = false;
                                                
                                                
                                            }


                                        }
                                        foundExecutablesList.Sort();



                                    }
                                    
                                }
                                // check it finds two or more items in the executables list then prints bell sound
                                if (foundExecutablesList.Count() >= 2)
                                {
                                    Console.Write('\x07');
                                    
                                   
                                }
                                // counting tab presses on the second tab press print contecation of executables list  string
                                if (tabCount > 1)
                                {
                                    
                                    Console.WriteLine(Environment.NewLine + string.Join("  ", foundExecutablesList));
                                    Console.Write(StartInput + input);
                                }
                                if(tabCount == 1 && foundExecutablesList.Count() == 1)
                                {
                                    Console.Write(filelastChars + " ");
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

