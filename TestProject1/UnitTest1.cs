using Xunit;

namespace src
{
    public class ConsoleManagerTests
    {
        [Theory]
        [InlineData("echo hello world", "hello world")]
        [InlineData("echo 'hello world'", "hello world")]
        [InlineData("echo \"hello world\"", "hello world")]
        [InlineData("echo hello", "hello")]
        [InlineData("echo hello   world", "hello world")]
        [InlineData("echo    hello    world   ", "hello world")]
        [InlineData("echo ''", "")]
        [InlineData("echo \"\"", "")]
        [InlineData("echo 'single quoted'", "single quoted")]
        [InlineData("echo \"double quoted\"", "double quoted")]
        [InlineData("echo hello\\ world", "hello world")]
        [InlineData("echo hello\\'world", "hello'world")]
        [InlineData("echo hello\\\"world", "hello\"world")]
        [InlineData("echo hello\\\\world", "hello\\world")]
        [InlineData("echo 'hello\\nworld'", "hello\\nworld")]
        [InlineData("echo \"hello\\nworld\"", "hello\\nworld")]
        public void EchoCommand_VariousInputs_ReturnsExpectedOutput(string input, string expected)
        {
            // Arrange
            var consoleManager = new ConsoleManager();

            // Act
            string result = consoleManager.HandleConsoleLine(input);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("pwd")]
        public void PwdCommand_ReturnsCurrentDirectory(string input)
        {
            // Arrange
            var consoleManager = new ConsoleManager();
            string expected = Directory.GetCurrentDirectory();

            // Act
            string result = consoleManager.HandleConsoleLine(input);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("unknowncommand", "unknowncommand: command not found")]
        [InlineData("notacommand arg1 arg2", "notacommand: command not found")]
        public void InvalidCommand_ReturnsCommandNotFound(string input, string expected)
        {
            // Arrange
            var consoleManager = new ConsoleManager();

            // Act
            string result = consoleManager.HandleConsoleLine(input);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void EmptyOrNullInput_ReturnsEmptyString(string input)
        {
            // Arrange
            var consoleManager = new ConsoleManager();

            // Act
            string result = consoleManager.HandleConsoleLine(input);

            // Assert
            Assert.Equal("", result);
        }
    }
}