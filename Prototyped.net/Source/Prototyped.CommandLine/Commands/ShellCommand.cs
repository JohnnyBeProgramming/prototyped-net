using Prototyped.Base.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prototyped.Cmd.Commands
{
    public class ShellCommand : IConsoleCommand
    {
        public string CommandDescription { get; internal set; }
        public string CommandHelpText { get; internal set; }

        public ShellCommand()
        {
            CommandDescription = "Opens a session for running multiple commands in the console.";
            CommandHelpText = @"Usage:
shell <command>

<command>   The start command to run in the shell.
";
        }

        public void RunCommand(string[] args)
        {
            // Start the command shell
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("# Shell started. (type exit to terminate)");

            // Run the shell
            var startCommand = args.Aggregate(String.Empty, (current, arg) => String.Format("{0}{1} ", current, (arg != null) && arg.Contains(" ") ? ("\"" + arg + "\"") : arg));
            string command;
            do
            {
                // Remember old foreground color
                Console.ForegroundColor = ConsoleColor.DarkGray;

                // Get user input
                Console.Write("# ");
                if (!String.IsNullOrEmpty(startCommand))
                {
                    command = startCommand;
                    Console.WriteLine(command);
                    startCommand = null;
                }
                else
                {
                    // Get and service command
                    command = Console.ReadLine();
                }

                // Reset console color
                Console.ForegroundColor = oldColor;
            } while (CheckAndServiceCommand(command));
        }

        private bool CheckAndServiceCommand(string command)
        {
            // Generate the arguments
            var args = command.Split(new[] { ' ' });

            if (command == "exit") return false;
            if (String.IsNullOrEmpty(command)) return true;
            if (command.StartsWith("shell")) return true;
            if (command.StartsWith("cmd ") && (args.Length > 0))
            {
                var subCommand = command.Substring("cmd ".Length).Trim();
                if (!String.IsNullOrEmpty(subCommand))
                {
                    var parts = subCommand.Split(new[] { ' ' }); ;
                    var exe = "cmd.exe";// parts[0];
                    var sub_args = subCommand;// subCommand.Substring(parts[0].Length).Trim();
                    using (var proc = new Process()
                    {
                        StartInfo = new ProcessStartInfo(exe)
                        {
                            //Arguments = sub_args,
                            UseShellExecute = false,
                            WindowStyle = ProcessWindowStyle.Hidden,
                            RedirectStandardError = true,
                            RedirectStandardInput = true,
                            RedirectStandardOutput = true,
                        }
                    })
                    {
                        var hasOutput = false;
                        proc.OutputDataReceived += (s, e) =>
                        {
                            if (!hasOutput) Console.WriteLine(String.Empty);
                            if (!String.IsNullOrEmpty(e.Data)) Console.WriteLine(e.Data);
                            hasOutput = true;
                        };
                        Console.Write(" -> Starting External Process");
                        proc.EnableRaisingEvents = true;
                        proc.Start();
                        proc.StandardInput.WriteLine("@echo off");
                        proc.StandardInput.WriteLine(sub_args);
                        proc.BeginOutputReadLine();
                        proc.StandardInput.WriteLine("exit");
                        while (!proc.HasExited)
                        {
                            proc.WaitForExit(1000);
                            if (!hasOutput) Console.Write(".");
                        }
                        Console.WriteLine(String.Empty);
                        return proc.ExitCode != 0;
                    }
                }
            }

            // Service the command
            Interop.ServiceCommand(args);

            return true;
        }
    }
}
