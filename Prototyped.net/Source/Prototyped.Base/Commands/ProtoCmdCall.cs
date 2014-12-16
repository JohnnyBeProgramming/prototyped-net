using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Prototyped.Base.Commands
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Event, AllowMultiple = true)]
    public class ProtoCmdCall : ProtoAttr
    {
        public string Name { get; internal set; }
        public string Hint { get; internal set; }

        public virtual string Prefix { get; set; }

        public ProtoCmdCall(string name, string desc)
        {
            Name = name;
            Hint = desc;
        }

        protected internal virtual object InvokeCommand(object cmdObj, string cmdType, string[] args)
        {
            // Check if this is a property
            var eventInfo = RuntimeTarget as EventInfo;
            if (eventInfo != null)
            {
                // Set the calculated property
                return eventInfo.RaiseMethod.Invoke(cmdObj, args);
            }

            // Check if this is a property
            var methodInfo = RuntimeTarget as MethodInfo;
            if (methodInfo != null)
            {
                // Set the calculated property
                return methodInfo.Invoke(cmdObj, new[] { args });
            }

            // No invoker could be found
            throw new Exception("No invoker found for '" + cmdType + "'.");
        }
    }
}
