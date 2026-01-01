using System.Diagnostics;
using System.Runtime.InteropServices;
namespace src
{
    public class Program
    {
        private const string StartInput = "$ ";
        private static bool isWaitingForDelay = false;
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
                int linePostion = 0;
                while (true)
                {
                    var key = Console.ReadKey(intercept:true);

                    // start a time on w press wait 5 secs and then print
                    if (key.Key == ConsoleKey.Enter)
                    {
                        Console.WriteLine();
                        linePostion = 0;
                        break;
                    }

                    if (key.Key == ConsoleKey.UpArrow)
                    {
                        // checks if it doesnt go out of bounds 
                        if (linePostion <Maker.inputLines.Count)
                        {
                            linePostion++;
                            string previousCommand = Maker.inputLines[Maker.inputLines.Count - linePostion];
                            
                            
                            // Backspace to clear current line
                            for (int i = 0; i < input.Length; i++)
                            {
                                // example with line : hello
                                // first loop moves the cursor on o makes it a blank space hell(blank space)
                                // the cursor is before the space so the second \b moves the cursor on the space
                                Console.Write("\b \b");
                            }
                            // then we have $(cursor position)(however many blank spaces)

                            // Write new command the spaces become irrevant, they dont get saved to the prompt, history
                            Console.Write(previousCommand);

                            input = previousCommand;
                        }
                        // if  it does 
                        else
                        {
                            // saves the uppresses to the count of available lines, so the ifs would valid which chekcs bounds range after
                            linePostion = Maker.inputLines.Count;
                            // nothing new get written to console
                        }

                    }

                    if (key.Key == ConsoleKey.DownArrow)
                    {
                        if(linePostion == Math.Max(0,linePostion))
                        {
                            linePostion = Math.Max(0, --linePostion);

                            string previousCommand = "";
                            if (linePostion != 0)
                            {
                                previousCommand = Maker.inputLines[Maker.inputLines.Count() - linePostion];
                            }
                            else
                            {
                                previousCommand = Maker.inputLines[Maker.inputLines.Count() - 1];
                            }

                            for (int i = 0; i < input.Count(); i++)
                                {
                                    Console.Write("\b \b");
                                }

                            Console.Write(previousCommand);
                            input = previousCommand;
                            
                        }

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

                    else if(key.Key != ConsoleKey.Tab && key.Key != ConsoleKey.UpArrow && key.Key != ConsoleKey.DownArrow)
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
                if (input.Contains('|'))
                {
                    string[] checkIfAtlastTwoList = input.Split('|');
                    if(checkIfAtlastTwoList.Length > 1 && RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    {
                        ProcessPipeCommand(input).Wait();
                        
                    }
  
                }
                else
                {
                    ConsoleOutput result = Maker.HandleConsoleLine(input);

                    if (!string.IsNullOrEmpty(result.output))
                    {
                        Maker.inputLinesThatRan.Add(input);

                        Console.WriteLine(result.output);
                    }

                    if (result.HasError && !string.IsNullOrEmpty(result.error))
                    {
                        Maker.inputLinesThatRan.Add(input);
                        Console.Error.WriteLine(result.error);
                    }

                    if (result.showHistory)
                    {
                        if (result.limitHistory != 0)
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
                    }
                }

            }
        }
        // basically async Task is used so we could use pipes it doesnt block other procceses start sending or receiving data
        // and can see errors
        static async Task ProcessPipeCommand(string line)
        {
            string[] splitList = line.Split('|', StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < splitList.Length; i++)
            {
                splitList[i] = splitList[i].Trim();
            }

            List<Process> processesToStart = new List<Process>();

            for (int i = 0; i < splitList.Length; i++)
            {
                // checks if cat is without arguments and if it is the first command then the variables becomes true
                bool isCatWithoutFile = (splitList[i] == "cat" && i == 0);
                // if command is first then sets its redirected input to false
                bool needsInput = (i > 0);
                if (isCatWithoutFile)
                {
                    needsInput = true;
                }

                Process process = new Process();

                if (splitList[i].StartsWith("cat"))
                {
                    string argument = splitList[i].Substring(3).Trim();
                    process.StartInfo = new ProcessStartInfo
                    {
                        FileName = "cat",
                        Arguments = argument,
                        RedirectStandardInput = needsInput,
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                }
                else if (splitList[i].StartsWith("wc"))
                {
                    string argument = splitList[i].Substring(2).Trim();
                    process.StartInfo = new ProcessStartInfo
                    {
                        FileName = "wc",
                        Arguments = argument,
                        RedirectStandardInput = needsInput,
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                }
                else if (splitList[i].StartsWith("ls"))
                {
                    string argument = splitList[i].Substring(2).Trim();
                    process.StartInfo = new ProcessStartInfo
                    {

                        FileName = "ls",
                        Arguments = argument,
                        // exception for ls the first command rule, it neved redirects stdinput
                        RedirectStandardInput = false,
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                }
                else if (splitList[i].StartsWith("tail"))
                {
                    string argument = splitList[i].Substring(4).Trim();
                    process.StartInfo = new ProcessStartInfo
                    {
                        FileName = "tail",
                        Arguments = argument,
                        RedirectStandardInput = needsInput,
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                }
                else if (splitList[i].StartsWith("grep"))
                {
                    string argument = splitList[i].Substring(4).Trim();
                    process.StartInfo = new ProcessStartInfo
                    {
                        FileName = "grep",
                        Arguments = argument,
                        RedirectStandardInput = needsInput,
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                }
                else if (splitList[i].StartsWith("head"))
                {
                    string argument = splitList[i].Substring(4).Trim();
                    process.StartInfo = new ProcessStartInfo
                    {
                        FileName = "head",
                        Arguments = argument,
                        RedirectStandardInput = needsInput,
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                }
                processesToStart.Add(process);
            }
            // checks if there are at least two commands for a pipe
            if (processesToStart.Count < 2)
            {
                return;
            }

            try
            {
              
                foreach (var process in processesToStart)
                {
                    process.Start();
                }

               
                var pipeTask = Task.Run(async () =>
                {
                    try
                    {
                        for (int i = 0; i < processesToStart.Count - 1; i++)
                        {
                            var sourceProcess = processesToStart[i];
                            var destProcess = processesToStart[i + 1];
                            // start copies a from the first to the second to the n output into that command input and immediately continues
                            await sourceProcess.StandardOutput.BaseStream.CopyToAsync(
                                destProcess.StandardInput.BaseStream);
                            // closes the proccess that got the copy after the copying is done
                            destProcess.StandardInput.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                      
                        Console.WriteLine($"Pipe error: {ex.Message}");
                    }
                });

               
                var readTask = Task.Run(async () =>
                {
                    try
                    {
                        var lastProcess = processesToStart.Last();
                        var reader = lastProcess.StandardOutput;

                        while (true)
                        {
                            // then we read line by line the output of the last proccess
                            string lineOutput = await reader.ReadLineAsync();
                            
                            if (lineOutput == null) 
                                break;

                            Console.WriteLine(lineOutput);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Read error: {ex.Message}");
                    }
                });

                await Task.WhenAll(pipeTask, readTask);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
               
                foreach (var process in processesToStart)
                {
                    try
                    {
                        if (!process.HasExited)
                        {
                            process.Kill();
                            process.WaitForExit();
                        }
                        // lets finally use the resources that were used for all the proccesses again
                        process.Dispose();
                    }
                    catch { }
                }
            }
        }

    }
}

