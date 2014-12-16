using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Prototyped.Base.Commands
{
    public class ArgsParserState
    {
        public int pos { get; set; }
        public string[] args { get; set; }
        public Match regex { get; set; }
    }
}
