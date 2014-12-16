using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prototyped.Base.Commands
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Event, AllowMultiple = false)]
    public class ProtoCmdRunner : ProtoCmdCall
    {
        public ProtoCmdRunner(string desc) : base("(default)", desc) { }
    }
}
