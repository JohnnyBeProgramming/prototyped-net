using Prototyped.Base;
using Prototyped.Base.Commands;
using Prototyped.Base.Generics;
using Prototyped.Base.Interfaces;
using Prototyped.Cmd.Commands;
using Prototyped.Cmd.Commands.Samples;
using Prototyped.Data;
using Prototyped.Data.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Prototyped.Cmd
{
    public static class Interop
    {
        private static string EXE_NAME = Path.GetFileName(typeof(Interop).Assembly.Location);

        /// <summary>
        /// Holds a collection of generic commands
        /// </summary>
        private static Dictionary<String, IConsoleCommand> Commands { get; set; }

        private static readonly Stack<ConsoleColor> ForeColorStack = new Stack<ConsoleColor>();
        private static readonly Stack<ConsoleColor> BackColorStack = new Stack<ConsoleColor>();

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
                        command.ShowHelpText(args);
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
                    if (Commands.Keys.Any())
                    {
                        Interop.WriteHeader(" Registered Commands:");
                        foreach (var key in Commands.Keys)
                        {
                            Console.WriteLine(String.Format("  {0,-11} {1}", key, Commands[key].HelpDesc));
                        }
                        Interop.WriteHorzLn();
                    }
                }
                else
                {
                    // No commmand specified
                    WriteWarning("Unknown Command.", "# Warnning: ");
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
        public static void WriteError(string message, string prefix = "Error: ")
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(prefix + message);
            Console.ForegroundColor = oldColor;
        }

        /// <summary>
        /// Writes a formatted Warning message
        /// </summary>
        /// <param name="message"></param>
        public static void WriteWarning(string message, string prefix = "Warning: ")
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(prefix + message);
            Console.ForegroundColor = oldColor;
        }

        private static void ShowDefaultHelpMessage()
        {
            Interop.WriteHeader(" Command-line Help and Information.");
            Interop.WriteStatus(" Command Usages:");
            Interop.WriteStatus("  - " + EXE_NAME + @" <command> /?");
            Interop.WriteStatus("  - " + EXE_NAME + @" <command> <args>");
            Interop.WriteStatus("  - " + EXE_NAME + @" -a <assembly> <command> <args>");
            Interop.WriteStatus("");
            Interop.WriteStatus(" Where:");
            Interop.WriteStatus("  <command>   The name of the command to run.");
            Interop.WriteStatus("  <args>      The list of arguments specific to that command.");
            Interop.WriteStatus("  -a          Scan and load all commands defined in <assembly>");
            Interop.WriteStatus("  <assembly>  Fully qualified assembly name or file (.dll) name.");
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
            var newArgs = new string[args.Length - 1];
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
            Interop.WriteStatus("  - No command line options specified.");
            Interop.WriteStatus("  - Type '" + EXE_NAME + " /?' for more info.");
        }

        /// <summary>
        /// Defines the list of registered commands
        /// </summary>
        private static void DefineOptions()
        {
            Commands = new Dictionary<String, IConsoleCommand>();

            // Define a basic sample command
            Commands["ping"] = new BasicPingCommand();

            // Example of how to load commands from a well known type
            Commands["tester"] = ProtoCmd.Instantiate<AttributedDeclarativeCommand>();

            // Definne the list of internal commands
            Commands["shell"] = ProtoCmd.Instantiate<ShellCommand>();
            Commands["winproc"] = ProtoCmd.Instantiate<WinProcessCommand>();

            // Define assemblies to search for attribute-declared commands (uses reflection, slow...)
            foreach (var assembly in new Assembly[]
            { 
                //typeof(Interop).Assembly, 
                typeof(ProtoSqlCmd).Assembly 
            })
            {
                // Find all commands in the assembly and register them
                foreach (var protoCmd in ProtoCmd.FromAssembly(assembly))
                {
                    var name = protoCmd.Name;
                    if (Commands.ContainsKey(name))
                    {
                        throw new Exception("The command '" + name + "' has already been registered. Cannot override another command.");
                    }
                    Commands[name] = protoCmd;
                }
            }
        }

        #region Write Helper Functions

        public static void WriteHorzLn()
        {
            Console.WriteLine("-------------------------------------------------------------------------------");
        }
        public static void WriteHeader(string msg, params object[] args)
        {
            WriteHorzLn();
            WriteStatus(msg, args);
            WriteHorzLn();
        }
        public static void WriteStatus(string msg, params object[] args)
        {
            Console.WriteLine(string.Format(msg, args).PadRight(80 - 1));
        }
        public static void WriteFooter()
        {
            WriteHorzLn();
        }

        #endregion

        #region Color Stack Functionality

        internal static void PushForeColor(ConsoleColor color)
        {
            ForeColorStack.Push(Console.ForegroundColor);
            Console.ForegroundColor = color;
        }
        internal static void PushBackColor(ConsoleColor color)
        {
            BackColorStack.Push(Console.BackgroundColor);
            Console.BackgroundColor = color;
        }
        internal static ConsoleColor PopForeColor()
        {
            var color = Console.ForegroundColor;
            Console.ForegroundColor = ForeColorStack.Pop();
            return color;
        }
        internal static ConsoleColor PopBackColor()
        {
            var color = Console.BackgroundColor;
            Console.BackgroundColor = BackColorStack.Pop();
            return color;
        }

        #endregion
    }
}
