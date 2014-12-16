using Prototyped.Base.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prototyped.Cmd.Commands.Samples
{
    /// <summary>
    /// This is a basic implementation of a console-driven command
    /// </summary>
    public class BasicPingCommand : IConsoleCommand
    {
        public string HelpTitle { get; internal set; }

        public BasicPingCommand()
        {
            HelpTitle = "Basic ping implemented for 'IConsoleCommand'.";
        }

        public void RunCommand(string[] args)
        {
            Console.WriteLine("Ping! Current time is: " + DateTime.Now.ToLongTimeString());
        }

        public string GetHelpText()
        {
            return @"Please Note: This command has no additional options.";
        }
    }
}
