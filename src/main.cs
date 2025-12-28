namespace src
{
    public class Program
    {
        private const string StartInput = "$ ";
        static void Main(string[] args)
        {
            string stringOfPaths = Environment.GetEnvironmentVariable("PATH") ?? "";
            
            ConsoleManager Maker = new ConsoleManager(stringOfPaths);

            
            bool noMatches = true;
            bool tabPressed = false;
            bool noMatchesCommand = true;
            while (true)
            { 
                Console.Write(StartInput);

                string input = "";
                int tabCount = 0;
                int countForFoundSimilarFileNamesOnTabPresses = 0;
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

                        // getting all the chars before tab press
                        string partialString = input;

                        foreach (var command in Maker.validCommandsList)
                        {
                            if (command.StartsWith(partialString))
                            {
                                string commandlastChars = command.Substring(partialString.Length);
                                // autocompleting if starts to the command and saving to input
                                input += commandlastChars + " ";
                                // but because of tab completion works and there is already on screen letters, then only need the last chars that were missing after completion
                                Console.Write(commandlastChars + " ");
                                noMatches = false;
                                noMatchesCommand = false;
                                break;

                            }
                           
                        }
                        if(noMatchesCommand)
                        {

                            try
                            {
                                string filelastChars = "";
                                foreach (var directory in Maker.splitPathList)
                                {
                                    // when testing locally what u have in path or added sometimes the paths listed are old and they dont get updated when the dirs are deleted on file explorer
                                    if(Directory.Exists(directory))
                                    {
                                        // files is just holding a blueprint how in a loop the program will accces the files and with what spefications
                                        // right not it only reserved memory only for the variable 
                                        var files = Directory.EnumerateFiles(directory).Where(file => Path.GetFileName(file).StartsWith(partialString));

                                        // when accesing the files in a for loop the EnumerateFiles gives the ability to acces the directory to check the files with the created enumerable
                                        // in a loop it checks if a file has the spefication of  Where(file => Path.GetFileName(file).StartsWith(partialString));
                                        // if it doesnt skip. Else return the value and  at the same time saves the position on where the enumerable
                                        // ended so that way it doesnt need to iterate over all the files again to that position and check if the file found needs to be skipped
                                        foreach (string fullPathToFileName in files)
                                        {
                                            if (File.Exists(fullPathToFileName))
                                            {
                                                string FileName = fullPathToFileName.Substring(directory.Length + 1);
                                                filelastChars = FileName.Substring(partialString.Length);
                                                foundExecutablesList.Add(partialString + filelastChars);
                                                noMatches = false;
                                            }
                                        }
                                       
                                        foundExecutablesList.Sort();
                                    }
                                }

                                List<string> executableListByLength = new List<string>(foundExecutablesList);
                                // compares the a with b if a i comes first return -1
                                // if they are the same return 0
                                // if a longer than b returns 1 the lambda expression
                                // then the sort gets the value of the pair compared an changes it 
                                executableListByLength.Sort((a, b) => a.Length.CompareTo(b.Length));
                                int rangeOfList = executableListByLength.Count();

                                if (rangeOfList > countForFoundSimilarFileNamesOnTabPresses + 1)
                                {
                                    string exelastChars = "";
                                    string completion = "";
                                    if (executableListByLength[1].StartsWith(executableListByLength[0]) && tabCount == 1)
                                    {
                                        exelastChars = executableListByLength[0].Substring(input.Length);
                                        input += exelastChars;
                                        Console.Write(exelastChars);
                                        countForFoundSimilarFileNamesOnTabPresses++;
                                    }

                                    if (executableListByLength[countForFoundSimilarFileNamesOnTabPresses + 1].StartsWith(executableListByLength[countForFoundSimilarFileNamesOnTabPresses]) && tabCount > 1)
                                    {
                                        // need to add not the second but the first item in the list 
                                        completion = executableListByLength[countForFoundSimilarFileNamesOnTabPresses].Substring(input.Length);
                                        input += completion;

                                        Console.Write(completion);
                                        countForFoundSimilarFileNamesOnTabPresses++;
                                    }
                                    else if(tabCount > 1)
                                    {
                                        Console.WriteLine(Environment.NewLine + string.Join("  ", executableListByLength));
                                        Console.Write(StartInput + input);
                                    }
                                    if (foundExecutablesList.Count() >= 2 && !executableListByLength[1].StartsWith(executableListByLength[0]) && tabCount == 1)
                                    {
                                        Console.Write('\x07');
                                    }
                                }

                                // check for the last element in a list if there are atleast two
                                if (executableListByLength[^1].StartsWith(input) && tabCount > 1 && rangeOfList < countForFoundSimilarFileNamesOnTabPresses + 1)
                                {
                                    string completion = executableListByLength[^1].Substring(input.Length);
                                    input += completion;

                                    Console.Write(completion);
                                }
                                ///because of updating input to longer and longer string after tab presses
                                ///the input could have only one match so we need to check if the list has found only one item
                                if (foundExecutablesList.Count() == 1)
                                {
                                    string completion = foundExecutablesList[0].Substring(input.Length);
                                    input += completion;
                                    Console.Write(completion);

                                    if (input == foundExecutablesList[0])
                                    {
                                        // add space if input is the right name
                                        input += " ";
                                        Console.Write(" ");
                                    }
                                }

                            }
                            catch (Exception ex)
                            {
                                if (ex is PathTooLongException || ex is UnauthorizedAccessException || ex is DirectoryNotFoundException)
                                {
                                    Console.WriteLine('\x07');
                                }
                            }
                           
                        }
                        
                    }

                    else 
                    {
                        input += key.KeyChar;
                        
                        countForFoundSimilarFileNamesOnTabPresses = 0;
                        tabCount = 0;
                        Console.Write(key.KeyChar);
                    }

                    if (noMatches && tabPressed)
                    {
                        Console.WriteLine('\x07');
                    }

                }
               
                Maker.inputLines.Add(input);
            
                

                ConsoleOutput result = Maker.HandleConsoleLine(input);

                if (!string.IsNullOrEmpty(result.output))
                {
                    Console.WriteLine(result.output);
                }

                if(result.HasError && !string.IsNullOrEmpty(result.error))
                {
                    Console.Error.WriteLine(result.error);
                }

                if (result.showHistory)
                {
                    if(result.limitHistory != 0)
                    {
                        for (int i = result.history.Count() - result.limitHistory; i < result.history.Count(); i++)
                        {
                            Console.WriteLine($"    {i + 1}  {result.history[i]}");
                        }
                    }
                    else
                    {
                        for (int i = result.limitHistory; i < result.history.Count(); i++)
                        {
                            Console.WriteLine($"    {i + 1}  {result.history[i]}");
                        }
                    }
                    result.history.RemoveAt(result.history.Count() - 1);


                }
                

            }
        }
    }
}

