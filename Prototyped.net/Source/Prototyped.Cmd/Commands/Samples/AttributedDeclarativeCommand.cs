using Prototyped.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Prototyped.Cmd.Commands.Samples
{
    /// <summary>
    /// This sample class is decorated with some declaritive attributes to
    /// demonstrate how to hook up a command with bound parameters and call 
    /// functions also declared.
    /// </summary>
    [Proto.Command("tester", "Sample command using declarative attributes.")]
    public class AttributedDeclarativeCommand
    {
        /// <summary>
        /// Multiple variations can be bound to the same member.
        /// Regular expressions can also be used for matching...
        /// </summary>
        [Proto.Command.Arg("-debug", Hint = "Debug Mode will be active.")]
        [Proto.Command.Arg("-notes", Hint = "Notes are displayed.")]
        [Proto.Command.Arg("-warns", Hint = "Warnings will be shown.")]
        [Proto.Command.Arg("-context:(.*)", 1, Hint = "Wildcard mode")]
        public string Contextual { get; set; }

        /// <summary>
        /// Example of binding a specific value to an attribute
        /// </summary>
        [Proto.Command.Arg("/Q", true, Hint = "Silent Mode")]
        [Proto.Command.Arg("/Silent", true, Hint = "Silent Mode")]
        [Proto.Command.Arg("/Loud", false, Hint = "Loud Mode")]
        public bool SilentMode { get; set; }

        /// <summary>
        /// Match the first argument and use the second as the value
        /// Example: -conn "value to use"
        /// </summary>
        [Proto.Command.Arg("-conn", AttrParser.UseNextArg)]
        public string Connection { get; set; }

        public AttributedDeclarativeCommand() { }

        [Proto.Command.Call("info", "Displays the current state of the command object.")]
        public void ShowCurrentInfo(string[] args)
        {
            Console.WriteLine("-------------------------------------------------------------------------------");
            Console.WriteLine(" - Current State for '{0}'", this.GetType().Name);
            Console.WriteLine(" - Args: {0}", string.Join(" ", args));
            Console.WriteLine("-------------------------------------------------------------------------------");
            Console.WriteLine(" - SilentMode: {0}", SilentMode);
            Console.WriteLine(" - Connection: {0}", Connection);
            Console.WriteLine(" - Contextual: {0}", Contextual);
            Console.WriteLine("-------------------------------------------------------------------------------");
        }
    }
}
