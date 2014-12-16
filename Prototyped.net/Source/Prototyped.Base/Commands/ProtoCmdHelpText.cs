using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prototyped.Base.Commands
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Event, AllowMultiple = false)]
    public class ProtoCmdHelpText : ProtoCmdCall
    {
        public ProtoCmdHelpText() : base("(help)", "Shows the help text.") { }
    }
}
