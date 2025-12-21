
public class ConsoleManagerTests
{
    private ConsoleManager consoleManager;

    public ConsoleManagerTests()
    {
        consoleManager = new ConsoleManager();
    }

    // ---------- Simple built-in command tests ----------

    [Fact]
    public void Pwd_ReturnsCurrentDirectory()
    {
        var result = consoleManager.HandleConsoleLine("pwd");
        Assert.Equal(Environment.CurrentDirectory, result.output);
    }

    [Fact]
    public void Cd_ToHomeDirectory()
    {
        string home = (Environment.OSVersion.Platform == PlatformID.Unix ||
                       Environment.OSVersion.Platform == PlatformID.MacOSX)
                       ? Environment.GetEnvironmentVariable("HOME")
                       : Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");

        var result = consoleManager.HandleConsoleLine("cd ~");
        Assert.Equal("", result.output);
        Assert.Equal(home, Environment.CurrentDirectory);
    }

    [Fact(Skip = "Exit command would terminate test runner")]
    public void ExitCommand_Skipped()
    {
        consoleManager.HandleConsoleLine("exit");
    }

    // ---------- Echo command tests ----------

    [Theory]
    [InlineData("echo hello world", "hello world")]
    [InlineData("echo \"hello world\"", "hello world")]
    [InlineData("echo 'hello world'", "hello world")]
    [InlineData("echo \"hello   world\"", "hello   world")]
    [InlineData("echo hello\\ world", "hello world")]
    [InlineData("echo \"hello \\\"world\\\"\"", "hello \"world\"")]

    public void EchoCommand_HandlesQuotesAndBackslashes(string input, string expected)
    {
        var result = consoleManager.HandleConsoleLine(input);
        Assert.Equal(expected, result.output);
    }

    // ---------- Type command tests ----------

    [Fact]
    public void TypeCommand_KnownBuiltin()
    {
        var result = consoleManager.HandleConsoleLine("type pwd");
        Assert.Equal("pwd is a shell builtin", result.output);
    }

    [Fact]
    public void TypeCommand_Unknown()
    {
        var result = consoleManager.HandleConsoleLine("type nonexistentcommand");
        Assert.Equal("nonexistentcommand: not found", result.output);
    }

    // ---------- Handling of invalid commands ----------

    [Fact]
    public void UnknownCommand_ReturnsError()
    {
        var result = consoleManager.HandleConsoleLine("foobar");
        Assert.Equal("foobar: command not found", result.output);
    }

    // ---------- Redirection tests ----------


    [Fact]
    public void Echo_WithFileRedirection_WritesFile()
    {
        string tempFile = Path.GetTempFileName();
        try
        {
            var result = consoleManager.HandleConsoleLine($"echo hello > {tempFile}");
            Assert.Equal("", result.output);
            Assert.Equal("hello", File.ReadAllText(tempFile).Trim());
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    // ---------- Edge-case input ----------

    [Theory]
    [InlineData("", "")]
    [InlineData("   ", "")]
    [InlineData("echo", "")]
    public void EmptyOrWhitespaceInput_ReturnsEmpty(string input, string expected)
    {
        var result = consoleManager.HandleConsoleLine(input);
        Assert.Equal(expected, result.output);
    }

    // ---------- Change directory invalid ----------

    [Fact]
    public void Cd_ToInvalidDirectory_ReturnsError()
    {
        var result = consoleManager.HandleConsoleLine("cd /nonexistentpath12345");
        Assert.Contains("No such file or directory", result.output);
    }
}
