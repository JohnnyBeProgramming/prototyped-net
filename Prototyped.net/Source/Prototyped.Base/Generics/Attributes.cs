using Prototyped.Base.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prototyped.Base.Generics
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class ProtoCommand : Attribute//, IConsoleCommand
    {
        public string CommandName { get; internal set; }
        public string CommandDescription { get; internal set; }
        public string CommandHelpText { get; internal set; }
        
        public ProtoCommand(string name, string desc)
        {
            CommandName = name;
            CommandDescription = desc;
        }

        public virtual string Name
        {
            get { return CommandName; }
        }

        public virtual string Description
        {
            get { return CommandDescription; }
        }

        public virtual string HelpText
        {
            get { return CommandHelpText; }
            set { CommandHelpText = value; }
        }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class ProtoCmdArg : Attribute
    {
        public string Matches { get; internal set; }
        public object HitValue { get; internal set; }

        public virtual string Hint { get; set; }
        public virtual Action<object> Callback { get; set; }

        public ProtoCmdArg(string matches, object value)
        {
            Matches = matches;
            HitValue = value;
        }
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Event | AttributeTargets.ReturnValue | AttributeTargets.Delegate, AllowMultiple = true)]
    public class ProtoCmdAction : Attribute
    {
        public string Name { get; internal set; }
        public string Hint { get; internal set; }

        public virtual string[] Args { get; set; }
        public virtual Action<string[]> Callback { get; set; }

        public ProtoCmdAction(string name, string desc)
        {
            Name = name;
            Hint = desc;
        }
    }
}
