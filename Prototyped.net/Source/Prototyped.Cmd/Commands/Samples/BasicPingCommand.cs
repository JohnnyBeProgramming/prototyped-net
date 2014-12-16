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
        public string HelpDesc { get; internal set; }
        public string HelpText { get; internal set; }

        public BasicPingCommand()
        {
            HelpDesc = "Basic ping implemented for 'IConsoleCommand'.";
            HelpText = "This command has no additional options.";
        }

        public void RunCommand(string[] args)
        {
            var msg = "Ping!";
            if (args.Length >= 1)
            {
                msg = string.Format("Hello {0}!", args[0]);
            }
            Interop.WriteStatus(" - {0} Current time is: {1}", msg, DateTime.Now.ToLongTimeString());
        }

        public void ShowHelpText(string[] args)
        {
            Interop.WriteHeader(" " + HelpDesc);
            Interop.WriteStatus(" - Please Note: {0}", HelpText);
            Interop.WriteHorzLn();
        }
    }
}
