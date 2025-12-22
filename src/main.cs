
namespace src
{
    public class Program
    {
        private const string StartInput = "$ ";
        static void Main(string[] args)
        {
            string pathListString = Environment.GetEnvironmentVariable("PATH") ?? "";
            ConsoleManager Maker = new ConsoleManager(pathListString);
            bool noMatches = true;
            bool tabPressed = false;
            bool noMatchesCommand = false;
            while (true)
            {
                Console.Write(StartInput);

                string input = "";
                int tabCount = 0;
                int i = 0;
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
                        //#123 because of updating input to longer and longer string one there could be only match so in the files
                        // if (foundExecutablesList.Count() == 1) it could that the list comes as one item 
                        string partialString = input;

                        foreach (var command in Maker.validCommandsList)
                        {
                            if (command.StartsWith(partialString))
                            {
                               
                                string commandlastChars = command.Substring(partialString.Length);
                                input += commandlastChars + " ";
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

                                        // finds every file in a single dir if accesible gets the filename and changes it to lower checks if starts with the partialstring
                                        // if it maches the select return that one saves to files
                                        var files = Directory.EnumerateFiles(directory).Where(file => Path.GetFileName(file).ToLower().StartsWith(partialString));

                                        foreach (string fileName in files)
                                        {
                                            
                                            if (File.Exists(fileName))
                                            {
                                                
                                                string newFileName = fileName.Substring(directory.Length + 1);
                                                filelastChars = newFileName.Substring(partialString.Length);
                                                foundExecutablesList.Add(partialString + filelastChars);

                                                noMatches = false;
                                            }
                                        }
                                        foundExecutablesList.Sort();
                                    }
                                }

                                List<string> executableListByLength = new List<string>(foundExecutablesList);
                                // compares the a with b if a i comes first return -1 if they they same unchanged return 0
                                // if a longer a comes after b returns 1 the lambda expression
                                // then the sort gets the value of the pair compared an changes it 
                                executableListByLength.Sort((a, b) => a.Length.CompareTo(b.Length));
                                int rangeOfList = executableListByLength.Count();

                                if (rangeOfList > i + 1)
                                {
                                    string exelastChars = "";
                                    if (executableListByLength[1].StartsWith(executableListByLength[0]) && tabCount == 1)
                                    {
                                        // need to use partialString because input doesnt get updated here 
                                        exelastChars = executableListByLength[0].Substring(partialString.Length);
                                        input += exelastChars;
                                        Console.Write(exelastChars);
                                    }

                                    if (executableListByLength[i + 1].StartsWith(executableListByLength[i]) && tabCount > 1)
                                    {
                                        // need to add not the second but the first item in the list 
                                        string completion = executableListByLength[i].ToString().Substring(input.Length);
                                        input += completion;

                                        Console.Write(completion);
                                        i++;
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
                                //#123 we check every time because it could be the last long file name and input became too long for others
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
                        // after pressing any key other than  the tab becomes 0
                        i = 0;
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

