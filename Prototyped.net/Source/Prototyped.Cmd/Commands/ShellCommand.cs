using Prototyped.Base;
using Prototyped.Base.Interfaces;
using System;
using System.Diagnostics;
using System.Linq;

namespace Prototyped.Cmd.Commands
{
    [Proto.Command("shell", "Session for running multiple commands in the console.")]
    public class ShellCommand
    {
        protected string Prefix = " # ";
        protected string Header = "Command Shell Active. (type exit to terminate)";

        public ShellCommand()
        {
        }

        [Proto.Command.RunDefault("Starts up the command shell")]
        public void RunCommand(string[] args)
        {
            // Run the shell
            var firstUse = true;
            var command = string.Empty;
            var startCommand = args.Aggregate(string.Empty, (current, arg) =>
            {
                var arglist = (arg != null) && arg.Contains(" ") ? ("\"" + arg + "\"") : arg;
                return string.Format("{0}{1} ", current, arglist);
            });
            do
            {
                try
                {
                    // Set custom foreground color
                    Interop.PushForeColor(ConsoleColor.DarkGray);

                    // Display header (if first time)
                    if (firstUse)
                    {
                        firstUse = false;
                        Interop.WriteHeader(Prefix + Header);
                    }

                    // Get user input (or display command)
                    Console.Write(Prefix);
                    if (!string.IsNullOrEmpty(startCommand))
                    {
                        // Display the command about to be run
                        command = startCommand;
                        Console.WriteLine(command);
                        startCommand = null;
                    }
                    else
                    {
                        // Get and service command
                        command = Console.ReadLine();
                    }
                }
                finally
                {
                    // Reset console color
                    Interop.PopForeColor();
                }
            } while (CheckAndServiceCommand(command));
        }

        private bool CheckAndServiceCommand(string commmand)
        {
            // Generate the arguments
            var args = commmand.Split(new[] { ' ' });
            var cmd = (commmand ?? "").Trim();

            if (cmd == "exit") return false;
            if (String.IsNullOrEmpty(cmd)) return true;
            if (cmd.StartsWith("shell"))
            {
                if (cmd.Length == "shell".Length)
                {
                    return true;
                }
                cmd = cmd.Substring("shell".Length).TrimStart();
            }
            if (cmd == "cls")
            {
                Console.Clear();
                Interop.WriteHeader(Prefix + Header);
                return true;
            }
            if (cmd.StartsWith("cmd ") && (args.Length > 0))
            {
                var subCommand = cmd.Substring("cmd ".Length).Trim();
                if (!String.IsNullOrEmpty(subCommand))
                {
                    return StartProcess(subCommand);
                }
            }

            // Service the command
            Interop.ServiceCommand(args);

            return true;
        }

        private bool StartProcess(string sub_args)
        {
            var exe = "cmd.exe";// parts[0];
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
}
