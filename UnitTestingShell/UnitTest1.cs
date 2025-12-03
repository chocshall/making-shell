using Xunit;
using System;
using System.IO;
using System.Collections.Generic;
using ConsoleApp2;

namespace ConsoleApp2.Tests
{
    public class ShellUnitTestsFromTester
    {
        private readonly ConsoleManager _consoleManager;

        public ShellUnitTestsFromTester()
        {
            _consoleManager = new ConsoleManager();
        }

        // ===== Test Stage #IZ3 - Implement echo (simple cases) =====
        [Theory]
        [InlineData("echo apple mango", "apple mango")]
        [InlineData("echo apple orange", "apple orange")]
        public void HandleEcho_SimpleCases_ReturnsExpected(string input, string expected)
        {
            string result = _consoleManager.HandleEcho(input);
            Assert.Equal(expected, result);
        }

        // ===== Test Stage #EZ5 - Implement type for builtins =====
        // Note: The tester shows these should print specific messages
        [Fact]
        public void TypeCommand_ForBuiltin_Echo_PrintsCorrectMessage()
        {
            var stringWriter = new StringWriter();
            var originalOut = Console.Out;
            Console.SetOut(stringWriter);

            try
            {
                string[] splitInput = { "type", "echo" };
                _consoleManager.typeBuiltCommand(splitInput, new List<string> { "echo", "exit", "type", "pwd", "cd" }, "echo");

                string output = stringWriter.ToString().Trim();
                Assert.Equal("echo is a shell builtin", output);
            }
            finally
            {
                Console.SetOut(originalOut);
            }
        }

        [Fact]
        public void TypeCommand_ForBuiltin_Exit_PrintsCorrectMessage()
        {
            var stringWriter = new StringWriter();
            var originalOut = Console.Out;
            Console.SetOut(stringWriter);

            try
            {
                string[] splitInput = { "type", "exit" };
                _consoleManager.typeBuiltCommand(splitInput, new List<string> { "echo", "exit", "type", "pwd", "cd" }, "exit");

                string output = stringWriter.ToString().Trim();
                Assert.Equal("exit is a shell builtin", output);
            }
            finally
            {
                Console.SetOut(originalOut);
            }
        }

        [Fact]
        public void TypeCommand_ForBuiltin_Type_PrintsCorrectMessage()
        {
            var stringWriter = new StringWriter();
            var originalOut = Console.Out;
            Console.SetOut(stringWriter);

            try
            {
                string[] splitInput = { "type", "type" };
                _consoleManager.typeBuiltCommand(splitInput, new List<string> { "echo", "exit", "type", "pwd", "cd" }, "type");

                string output = stringWriter.ToString().Trim();
                Assert.Equal("type is a shell builtin", output);
            }
            finally
            {
                Console.SetOut(originalOut);
            }
        }

        // ===== Test Stage #EI0 - The pwd builtin =====
        [Fact]
        public void PrintWorkingDirectory_ValidCommand_PrintsCurrentDirectory()
        {
            var stringWriter = new StringWriter();
            var originalOut = Console.Out;
            Console.SetOut(stringWriter);

            try
            {
                string[] splitInput = { "pwd" };
                _consoleManager.printWorkingDirectory(splitInput, new List<string> { "pwd" });

                string output = stringWriter.ToString().Trim();
                // Should print current directory (not hardcoded /app)
                Assert.NotEmpty(output);
                Assert.True(Directory.Exists(output));
            }
            finally
            {
                Console.SetOut(originalOut);
            }
        }

        // ===== Test Stage #RA6 - cd builtin (absolute paths) =====
        [Fact]
        public void ChangeDirectory_ValidAbsolutePath_ChangesDirectory()
        {
            string originalDir = Directory.GetCurrentDirectory();
            string tempDir = Path.Combine(Path.GetTempPath(), "test_cd_" + Guid.NewGuid().ToString());

            try
            {
                Directory.CreateDirectory(tempDir);

                string[] splitInput = { "cd", tempDir };
                _consoleManager.changeDirectory(splitInput, new List<string> { "cd" });

                Assert.Equal(tempDir, Directory.GetCurrentDirectory());
            }
            finally
            {
                Directory.SetCurrentDirectory(originalDir);
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir);
            }
        }

        [Fact]
        public void ChangeDirectory_InvalidDirectory_PrintsErrorMessage()
        {
            var stringWriter = new StringWriter();
            var originalOut = Console.Out;
            Console.SetOut(stringWriter);

            try
            {
                string invalidPath = "/non-existing-directory-" + Guid.NewGuid().ToString();
                string[] splitInput = { "cd", invalidPath };
                _consoleManager.changeDirectory(splitInput, new List<string> { "cd" });

                string output = stringWriter.ToString().Trim();
                Assert.Contains("No such file or directory", output);
                Assert.Contains(invalidPath, output);
            }
            finally
            {
                Console.SetOut(originalOut);
            }
        }

        // ===== Test Stage #GQ9 - cd builtin (relative paths) =====
        [Fact]
        public void ChangeDirectory_RelativePath_ChangesDirectory()
        {
            string originalDir = Directory.GetCurrentDirectory();
            string parentDir = Directory.GetParent(originalDir)?.FullName;

            if (parentDir != null)
            {
                try
                {
                    string[] splitInput = { "cd", ".." };
                    _consoleManager.changeDirectory(splitInput, new List<string> { "cd" });

                    Assert.Equal(parentDir, Directory.GetCurrentDirectory());
                }
                finally
                {
                    Directory.SetCurrentDirectory(originalDir);
                }
            }
        }

        // ===== Test Stage #GP4 - cd builtin (home directory) =====
        [Fact]
        public void ChangeDirectory_HomeDirectory_ChangesToHome()
        {
            string originalDir = Directory.GetCurrentDirectory();

            try
            {
                string[] splitInput = { "cd", "~" };
                _consoleManager.changeDirectory(splitInput, new List<string> { "cd" });

                string homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                Assert.Equal(homePath, Directory.GetCurrentDirectory());
            }
            finally
            {
                Directory.SetCurrentDirectory(originalDir);
            }
        }

        // ===== Test Stage #NI6 - Single quotes =====
        [Theory]
        [InlineData("echo 'test script'", "test script")]
        [InlineData("echo script     hello", "script hello")] // Collapse spaces outside quotes
        [InlineData("echo 'world     example'", "world     example")] // Preserve spaces inside single quotes
        [InlineData("echo 'hello''test'", "hellotest")] // Adjacent single-quoted strings
        [InlineData("echo 'world     example' 'hello''test' script''shell", "world     example hellotest scriptshell")]
        public void HandleEcho_SingleQuotes_HandlesCorrectly(string input, string expected)
        {
            string result = _consoleManager.HandleEcho(input);
            Assert.Equal(expected, result);
        }

        // ===== Test Stage #TG6 - Double quotes =====
        [Theory]
        [InlineData("echo \"example shell\"", "example shell")]
        [InlineData("echo \"shell  script\"", "shell  script")] // Preserve spaces inside double quotes
        [InlineData("echo \"world\"  \"hello's\"  shell\"\"script", "world hello's shellscript")]
        public void HandleEcho_DoubleQuotes_HandlesCorrectly(string input, string expected)
        {
            string result = _consoleManager.HandleEcho(input);
            Assert.Equal(expected, result);
        }

        // ===== Test Stage #YT5 - Backslash outside quotes =====
        // THIS IS THE FAILING TEST - Needs to be fixed
        [Theory]
        [InlineData("echo script\\ \\ \\ \\ \\ \\ shell", "script      shell")] // 6 escaped spaces
        public void HandleEcho_BackslashEscapingSpaces_HandlesCorrectly(string input, string expected)
        {
            string result = _consoleManager.HandleEcho(input);
            Assert.Equal(expected, result);
        }

        // ===== Additional tests from other stages =====

        // Test that invalid commands return "bad"
        [Theory]
        [InlineData("invalid_grape_command")]
        [InlineData("invalid_command_1")]
        [InlineData("invalid_mango_command")]
        [InlineData("invalid_orange_command")]
        [InlineData("invalid_pineapple_command")]
        public void HandleConsoleLine_InvalidCommand_ReturnsBad(string input)
        {
            string result = _consoleManager.HandleConsoleLine(input);
            Assert.Equal("bad", result);
        }

        // Test exit command detection
        [Theory]
        [InlineData("exit", true)]
        [InlineData("exit 0", true)]
        public void IsExitCommand_ValidExitCommands_ReturnsTrue(string input, bool expected)
        {
            bool result = _consoleManager.IsExitCommand(input);
            Assert.Equal(expected, result);
        }

        // Test echo command detection
        [Theory]
        [InlineData("echo test", true)]
        [InlineData("echo apple mango", true)]
        [InlineData("echo apple orange", true)]
        [InlineData("justecho", false)] // Not an echo command
        public void IsEchoCommand_VariousInputs_ReturnsExpected(string input, bool expected)
        {
            bool result = _consoleManager.IsEchoCommand(input);
            Assert.Equal(expected, result);
        }

        // Test WhatIsInQuotes method with double quotes
        [Fact]
        public void WhatIsInQuotes_DoubleQuotes_SplitsCorrectly()
        {
            string[] resultArray = Array.Empty<string>();
            string input = "echo \"hello world\"";

            _consoleManager.WhatIsInQuotes(input, ref resultArray);

            Assert.Equal(2, resultArray.Length);
            Assert.Equal("echo", resultArray[0]);
            Assert.Equal("hello world", resultArray[1]);
        }

        // Test WhatIsInQuotes method with single quotes
        [Fact]
        public void WhatIsInQuotes_SingleQuotes_SplitsCorrectly()
        {
            string[] resultArray = Array.Empty<string>();
            string input = "echo 'hello world'";

            _consoleManager.WhatIsInQuotes(input, ref resultArray);

            Assert.Equal(2, resultArray.Length);
            Assert.Equal("echo", resultArray[0]);
            Assert.Equal("hello world", resultArray[1]);
        }

        // Test that pwd command is recognized
        [Fact]
        public void HandleConsoleLine_PwdCommand_ReturnsPwdCommandExecuted()
        {
            string result = _consoleManager.HandleConsoleLine("pwd");
            Assert.Equal("pwd command executed", result);
        }

        // Test that cd command is recognized
        [Fact]
        public void HandleConsoleLine_CdCommand_ReturnsCdCommandExecuted()
        {
            string result = _consoleManager.HandleConsoleLine("cd /tmp");
            Assert.Equal("cd command executed", result);
        }

        // Test that type command is recognized
        [Fact]
        public void HandleConsoleLine_TypeCommand_ReturnsTypeCommandExecuted()
        {
            string result = _consoleManager.HandleConsoleLine("type echo");
            Assert.Equal("type command executed", result);
        }

        // Test echo with just spaces returns "bad" (as shown in test - echo with only spaces)
        [Fact]
        public void HandleConsoleLine_EchoOnlySpaces_ReturnsBad()
        {
            string result = _consoleManager.HandleConsoleLine("echo   ");
            Assert.Equal("bad", result);
        }
    }
}