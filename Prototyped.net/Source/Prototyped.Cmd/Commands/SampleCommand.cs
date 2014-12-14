using Prototyped.Base.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prototyped.Cmd.Commands
{
    public class SampleCommand : IConsoleCommand
    {
        public string CommandName { get; internal set; }
        public string CommandDescription { get; internal set; }
        public string CommandHelpText { get; internal set; }

        public SampleCommand()
        {
            CommandDescription = "Implementation of 'IConsoleCommand' interface used for testing.";
            CommandHelpText = @" - there are no options available";
        }

        public void RunCommand(string[] args)
        {
            Console.WriteLine("This is a console test. Current time is: " + DateTime.Now.ToLongTimeString());
        }

    }
}
