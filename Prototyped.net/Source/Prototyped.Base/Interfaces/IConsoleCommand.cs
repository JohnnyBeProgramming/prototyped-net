using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prototyped.Base.Interfaces
{
    public interface IConsoleCommand
    {
        string HelpTitle { get; }

        void RunCommand(string[] args);
        string GetHelpText(string[] args = null);
    }
}
