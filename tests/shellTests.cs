using System;
using System.IO;
using System.Runtime.InteropServices;
using Xunit;

namespace src
{
    public class ConsoleManagerTests
    {
        // Subclass to override ExitCommand so tests don't terminate
        private class TestConsoleManager : ConsoleManager
        {
            protected override void ExitCommand(string[] splitInputList, string inputCommand)
            {
                // Do nothing in tests
            }
        }

        private ConsoleManager Create() => new TestConsoleManager();

        [Fact]
        public void InvalidCommand_ReturnsCommandNotFound()
        {
            var cm = Create();
            Assert.Equal("invalid_banana_command: command not found",
                         cm.HandleConsoleLine("invalid_banana_command"));
        }

        [Fact]
        public void MultipleInvalidCommands_ReturnCorrectMessages()
        {
            var cm = Create();

            Assert.Equal("invalid_command_1: command not found",
                cm.HandleConsoleLine("invalid_command_1"));

            Assert.Equal("invalid_command_2: command not found",
                cm.HandleConsoleLine("invalid_command_2"));

            Assert.Equal("invalid_command_3: command not found",
                cm.HandleConsoleLine("invalid_command_3"));
        }

        [Fact]
        public void ExitCommand_ReturnsEmptyString()
        {
            var cm = Create();
            Assert.Equal("", cm.HandleConsoleLine("exit"));
        }

        [Fact]
        public void EchoCommand_ReturnsCorrectOutput()
        {
            var cm = Create();

            Assert.Equal("orange mango",
                cm.HandleConsoleLine("echo orange mango"));

            Assert.Equal("pineapple strawberry",
                cm.HandleConsoleLine("echo pineapple strawberry"));

            Assert.Equal("strawberry blueberry",
                cm.HandleConsoleLine("echo strawberry blueberry"));
        }

        [Fact]
        public void TypeBuiltin_ReturnsShellBuiltin()
        {
            var cm = Create();

            Assert.Equal("echo is a shell builtin", cm.HandleConsoleLine("type echo"));
            Assert.Equal("exit is a shell builtin", cm.HandleConsoleLine("type exit"));
            Assert.Equal("type is a shell builtin", cm.HandleConsoleLine("type type"));
        }

        [Fact]
        public void Type_InvalidCommand_NotFound()
        {
            var cm = Create();

            Assert.Equal("invalid_banana_command: not found",
                cm.HandleConsoleLine("type invalid_banana_command"));
        }

        [Fact]
        public void Pwd_ReturnsWorkingDirectory()
        {
            var cm = Create();
            string expected = Directory.GetCurrentDirectory();
            Assert.Equal(expected, cm.HandleConsoleLine("pwd"));
        }

        [Fact]
        public void Cd_AbsolutePath_Works()
        {
            var cm = Create();

            string path = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? Path.Combine(Path.GetTempPath(), "test_cd_abs")
                : "/tmp/test_cd_abs";

            Directory.CreateDirectory(path);

            cm.HandleConsoleLine($"cd {path}");

            Assert.Equal(path, cm.HandleConsoleLine("pwd"));
        }

        [Fact]
        public void Cd_InvalidPath_ShowsError()
        {
            var cm = Create();
            string path = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? @"C:\does_not_exist"
                : "/does_not_exist";

            Assert.Equal($"cd: {path}: No such file or directory",
                cm.HandleConsoleLine($"cd {path}"));
        }

        [Fact]
        public void Cd_RelativePaths_Work()
        {
            var cm = Create();

            string baseDir = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? Path.Combine(Path.GetTempPath(), "cd_rel_test")
                : "/tmp/cd_rel_test";

            string targetDir = Path.Combine(baseDir, "inner", "dir");
            Directory.CreateDirectory(targetDir);

            cm.HandleConsoleLine($"cd {baseDir}");
            cm.HandleConsoleLine("cd ./inner/dir");

            Assert.Equal(targetDir, cm.HandleConsoleLine("pwd"));
        }

        [Fact]
        public void Cd_Tilde_GoesToHomeDirectory()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Skip test on Windows
                return;
            }

            var cm = Create();
            string fakeHome = "/tmp/home_test";
            Directory.CreateDirectory(fakeHome);
            Environment.SetEnvironmentVariable("HOME", fakeHome);

            string result = cm.HandleConsoleLine("cd ~");

            Assert.Equal("", result);
            Assert.Equal(fakeHome, cm.HandleConsoleLine("pwd"));
        }

        [Fact]
        public void Echo_SingleQuotes()
        {
            var cm = Create();
            Assert.Equal("test script", cm.HandleConsoleLine("echo 'test script'"));
        }

        [Fact]
        public void Echo_DoubleQuotes()
        {
            var cm = Create();
            Assert.Equal("shell world", cm.HandleConsoleLine("echo \"shell world\""));
        }
    }
}
