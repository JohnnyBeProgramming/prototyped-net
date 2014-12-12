using Prototyped.Base.Interfaces;
using Prototyped.Cmd.Commands;
using Prototyped.Data.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prototyped.Cmd
{
    public class Interop
    {
        private static string EXE_NAME = "proto.exe";

        /// <summary>
        /// Holds a collection of generic commands
        /// </summary>
        protected static Dictionary<String, IConsoleCommand> Commands;

        /// <summary>
        /// The main Entry point for the coonsole application
        /// </summary>
        /// <param name="args"></param>
        public static void Start(string[] args)
        {
            // Define Commmand-linne options
            DefineOptions();

            // Command line started
            if (args.Length > 0)
            {
                // There is a command defined, so service it
                ServiceCommand(args);
            }
            else
            {
                // No command is defined, so display the help text
                ShowEmptyText();
            }
        }

        /// <summary>
        /// Service the current command
        /// </summary>
        /// <param name="args"></param>
        public static void ServiceCommand(string[] args)
        {
            try
            {
                // Try and service the command
                if ((args.Length > 0) && (Commands.ContainsKey(args[0])))
                {
                    // Get the command and construct the (revised) arguments
                    var command = Commands[args[0]];
                    var newArgs = RemoveFirstArg(args);

                    // Check if help is requested action
                    if (HelpRequested(args))
                    {
                        // Show help for this specific command
                        Console.WriteLine(args[0] + ": " + command.CommandDescription + "\r\n");
                        Console.WriteLine(command.CommandHelpText);
                    }
                    else
                    {
                        // Run the selected command
                        command.RunCommand(newArgs);
                    }
                }
                else if ((args.Length == 1) && HelpRequested(args))
                {
                    // Show general help info
                    ShowDefaultHelpMessage();

                    // Display a list of defined commands
                    Console.WriteLine("List of available commands:\r\n");
                    foreach (var key in Commands.Keys)
                    {
                        Console.WriteLine(String.Format("{0,-11} {1}", key, Commands[key].CommandDescription));
                    }
                    Console.WriteLine("\r\n");
                }
                else
                {
                    // No commmand specified
                    WriteWarning("Command undefined or empty.");
                }
            }
            catch (Exception ex)
            {
                // Display error message on command line
                WriteError(ex.Message);
            }
        }

        /// <summary>
        /// Writes a formatted Error message
        /// </summary>
        /// <param name="message"></param>
        public static void WriteError(string message)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Error: " + message);
            Console.ForegroundColor = oldColor;
        }

        /// <summary>
        /// Writes a formatted Warning message
        /// </summary>
        /// <param name="message"></param>
        public static void WriteWarning(string message)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Warning: " + message);
            Console.ForegroundColor = oldColor;
        }

        private static void ShowDefaultHelpMessage()
        {
            Console.WriteLine("Command-line Help and Information.\r\n");
            Console.WriteLine(@"Usages:
" + EXE_NAME + @" <command> /?
" + EXE_NAME + @" <command> <args>
" + EXE_NAME + @" -a <assembly> <command> <args>

Options:
/? or /help Shows additional help information for the command.
<command>   The name of the command to run.
<args>      The list of arguments specific to that command.
-a          Scan and load all commands defined in <assembly>
<assembly>  Fully qualified assembly name
");
        }

        /// <summary>
        /// Checks if the '/?' or '/help' arg is defined
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static bool HelpRequested(IEnumerable<string> args)
        {
            return args.Any(arg => (arg == "/?") || (arg == "/help"));
        }

        /// <summary>
        /// Remove the first argument from the array
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static string[] RemoveFirstArg(string[] args)
        {
            var newArgs = new string[args.Length];
            for (var i = 1; i < args.Length; i++)
            {
                newArgs[i - 1] = args[i];
            }
            return newArgs;
        }

        /// <summary>
        /// Show the empty text for when nno arguments were sent
        /// </summary>
        private static void ShowEmptyText()
        {
            Console.WriteLine("No command line options specified. \r\nType '" + EXE_NAME + " /?' for more info.\r\n");
        }

        /// <summary>
        /// Defines the list of registered commands
        /// </summary>
        private static void DefineOptions()
        {
            // Definne the list of internal commands
            Commands = new Dictionary<String, IConsoleCommand>
                           {
                               {"sample", new SampleCommand()},
                               {"sql", new SqlCommand()},
                               {"shell", new ShellCommand()},
                               {"winproc", new WinProcessCommand()}
                           };
        }
    }
}
