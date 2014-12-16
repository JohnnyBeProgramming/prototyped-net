using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prototyped.Base.Commands
{
    public class ProtoAttr : Attribute
    {
        public object RuntimeTarget { get; set; }
    }
}
