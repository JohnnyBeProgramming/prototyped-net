using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prototyped.Base
{
    /// <summary>
    /// Define the base app runtime
    /// </summary>
    public interface IProtoAPI
    {
        void Init();
        void Destroy();
    }

    public static class Proto
    {
        static Proto()
        {
            //Runtimme = new ProtoRuntime();
        }

        public static IProtoAPI Runtimme { get; set; }

        public class Attr : ProtoAttr { }

        public class Command : ProtoCmd
        {
            public Command(string name, string description) : base(name, description) { }

            public class Arg : ProtoCmdArg
            {
                public Arg(string matches) : this(matches, AttrParser.UseAsValue) { }
                public Arg(string matches, int groupIndex) : this(matches, AttrParser.UseRegexGroupId, groupIndex) { }
                public Arg(string matches, object value) : this(matches, AttrParser.UseHitValue, value) { }
                public Arg(string matches, AttrParser parserType) : this(matches, parserType, null) { }
                public Arg(string matches, AttrParser parserType, object value) : base(matches, parserType, value) { }
            }

            public class Call : ProtoCmdCall
            {
                public Call(string name, string description) : base(name, description) { }
            }
        }
    }
}
