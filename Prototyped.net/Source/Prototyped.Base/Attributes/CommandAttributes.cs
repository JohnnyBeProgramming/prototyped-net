using Prototyped.Base.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Prototyped.Base
{
    public enum AttrParser
    {
        UseAsValue,
        UseNextArg,
        UseHitValue,
        UseRegexGroupId,
        Ignore,
    }

    public class ArgsParserState
    {
        public int pos { get; set; }
        public string[] args { get; set; }
        public Match regex { get; set; }
    }

    public class ProtoAttr : Attribute
    {
        public object Target { get; set; }
    }

    public delegate void RunCommand(string[] args);

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class ProtoCmd : ProtoAttr, IConsoleCommand
    {
        public string Name { get; internal set; }
        public string HelpTitle { get; internal set; }
        public string ActionType { get; internal set; }

        protected List<ProtoCmdArg> Args { get; set; }
        protected List<ProtoCmdCall> Callers { get; set; }

        public ProtoCmd(string name, string desc)
        {
            Name = name;
            HelpTitle = desc;
        }

        public void RunCommand(string[] args)
        {
            try
            {
                // Create a new instance of the command object
                var attrs = Args ?? (Args = GetCommandArgs(this));
                var callers = Callers ?? (Callers = GetCommandActions(this));
                var cmdObj = Activator.CreateInstance(Target as Type);

                // Parse and set the options
                ParseArguments(cmdObj, args);

                // Check and confirm connection
                if (!string.IsNullOrEmpty(ActionType))
                {
                    InvokeCommmand(cmdObj, ActionType, args);
                }
                else
                {
                    // Special case: No command specified, so call base method
                    RunBaseCommand(cmdObj, args);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        protected virtual void InvokeCommmand(object cmdObj, string cmdType, string[] args)
        {
            // Create a list of call mappers for fast matching
            var mapper = new Dictionary<String, ProtoCmdCall>();
            foreach (var caller in Callers)
            {
                mapper[caller.Name] = caller;
            }

            // Check if the command exists
            if (mapper.ContainsKey(cmdType))
            {
                mapper[cmdType].InvokeCommand(cmdObj, cmdType, args);
            }
            else throw new Exception("Command '" + cmdType + "' not recondised.");
        }

        protected virtual void ParseArguments(object cmdTarget, string[] args)
        {
            // Make sure we have a target caller
            if (args.Length > 0)
            {
                // Create a list of argument mappers for fast matching
                var mapper = new List<KeyValuePair<Regex, ProtoCmdArg>>();
                foreach (var arg in Args)
                {
                    mapper.Add(new KeyValuePair<Regex, ProtoCmdArg>(new Regex(arg.Matches), arg));
                }

                // Get the command target
                ActionType = args[0];

                // Now we traverse the list of args supplied and try to match them
                var state = new ArgsParserState { pos = 1, args = args };
                while (state.pos < args.Length)
                {
                    // Get all matches and apply option
                    var arg = args[state.pos];
                    if (!string.IsNullOrEmpty(arg))
                    {
                        // Try and match the current arg to any attributes
                        foreach (var match in mapper.Where(m => m.Key.Match(arg).Success))
                        {
                            state.regex = match.Key.Match(arg);
                            match.Value.ApplyCommandArg(cmdTarget, state);
                        }
                    }

                    // Step next
                    state.pos++;
                }
            }
        }

        protected virtual void RunBaseCommand(object cmdTarget, string[] args)
        {
            // The command was run without any parameters, show help text
            ShowHelpText();
        }

        protected virtual void GetHelpText()
        {
            var prefix = string.Empty;
            var suffix = string.Empty;

            // Usse a function to break the hibt text into more readable lines
            Func<string, string, int, string> lineParser = (input, pfx, pdr) =>
            {
                var sep = string.Empty;
                var txt = string.Empty;
                var inp = pfx + input;
                var max = 80 - 5 - pdr;
                while (inp.Length > max)
                {
                    var sec = inp.Substring(0, max);
                    var lbr = sec.LastIndexOf(" ");
                    if (lbr > 0)
                    {
                        inp = sec.Substring(lbr + 1, sec.Length - lbr - 1) + inp.Substring(max);
                        sec = sec.Substring(0, lbr).PadRight(max);
                    }
                    txt += sep + sec;
                    sep = "\r\n".PadRight(80 - max);
                }
                txt += sep + inp.PadRight(max);
                return txt;
            };

            // Parse the command list
            var cmdOpts = string.Empty;
            if (Callers.Any())
            {
                prefix += "<command> ";
                cmdOpts = "\r\n Command Actions:\r\n";
                foreach (var cmd in Callers)
                {
                    var pdr = 12;
                    var pfx = string.Empty;
                    var txt = lineParser(cmd.Hint, pfx, pdr);
                    cmdOpts += string.Format("\r\n  {0} {1}", cmd.Name.PadRight(pdr), txt);
                }
                cmdOpts += "\r\n";
            }

            // Parse the command arguments
            var cmdArgs = string.Empty;
            if (Args.Any())
            {
                //suffix = "[ -conn <conn_str_name> | -db <db_name> ] [ -trusted | -wa | -un <user> -pw <password> ]";
                suffix = "<options and switches> ";
                cmdArgs = "\r\n Options and Switches:\r\n";
                foreach (var cmd in Args)
                {
                    var pdr = 12;
                    var pfx = string.Empty;
                    var txt = lineParser(cmd.Hint, pfx, pdr);                    
                    cmdArgs += string.Format("\r\n  {0} {1}", cmd.Matches.PadRight(pdr), txt);
                }
                cmdArgs += "\r\n";
            }

            // Build up the current help text data
            var text = string.Format(@"-------------------------------------------------------------------------------
 Command Usage: {0}
-------------------------------------------------------------------------------
 proto.exe {0} {1}{2} 
{3}{4}
-------------------------------------------------------------------------------", Name, prefix, suffix, cmdOpts, cmdArgs);

            Console.WriteLine(text);
        }

        #region Static helper methods

        public static List<ProtoCmdArg> GetCommandArgs(ProtoCmd cmdAttr)
        {
            var list = new List<ProtoCmdArg>();
            if (cmdAttr == null) throw new Exception("Command attribute was not set.");
            if (cmdAttr.Target == null) throw new Exception("Target for the attribute was not set.");

            var attrType = cmdAttr.Target as Type;
            var attrFields = attrType.GetAttributedFields<ProtoCmdArg>();
            foreach (var attr in attrFields.Select(i => { i.Key.Target = i.Value; return i.Key; }))
            {
                list.Add(attr);
            }

            var attrProps = attrType.GetAttributedProperties<ProtoCmdArg>();
            foreach (var attr in attrProps.Select(i => { i.Key.Target = i.Value; return i.Key; }))
            {
                list.Add(attr);
            }

            return list;
        }

        public static List<ProtoCmdCall> GetCommandActions(ProtoCmd cmdAttr)
        {
            var list = new List<ProtoCmdCall>();
            if (cmdAttr == null) throw new Exception("Command attribute was not set.");
            if (cmdAttr.Target == null) throw new Exception("Target for the attribute was not set.");

            var attrType = cmdAttr.Target as Type;
            var attrMethods = attrType.GetAttributedMethods<ProtoCmdCall>();
            foreach (var attr in attrMethods.Select(i => { i.Key.Target = i.Value; return i.Key; }))
            {
                list.Add(attr);
            }

            var attrEvents = attrType.GetAttributedEvents<ProtoCmdCall>();
            foreach (var attr in attrEvents.Select(i => { i.Key.Target = i.Value; return i.Key; }))
            {
                list.Add(attr);
            }

            return list;
        }

        public static IConsoleCommand Instantiate<TCommmand>()
        {
            var attrs = typeof(TCommmand).GetAttributedTypes<ProtoCmd>().Select(i =>
            {
                var cmd = i.Key;

                cmd.Target = i.Value;

                return cmd;
            });
            if (!attrs.Any()) throw new Exception("Instantiation failed. The specified type does not contain the required attribute(s).");
            return attrs.First();
        }

        public static IEnumerable<ProtoCmd> FromAssembly(Assembly assembly)
        {
            var commands = assembly.GetAttributedTypes<ProtoCmd>();
            return commands.Select(i => { i.Key.Target = i.Value; return i.Key; });
        }

        #endregion
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class ProtoCmdArg : ProtoAttr
    {
        public string Matches { get; internal set; }
        public object HitValue { get; internal set; }
        public AttrParser ParseType { get; internal set; }

        public virtual string Hint { get; set; }

        public ProtoCmdArg(string matches) : this(matches, AttrParser.UseAsValue) { }
        public ProtoCmdArg(string matches, int groupIndex) : this(matches, AttrParser.UseRegexGroupId, groupIndex) { }
        public ProtoCmdArg(string matches, object value) : this(matches, AttrParser.UseHitValue, value) { }
        public ProtoCmdArg(string matches, AttrParser parserType) : this(matches, parserType, null) { }
        public ProtoCmdArg(string matches, AttrParser parserType, object value)
        {
            Matches = matches;
            HitValue = value;
            ParseType = parserType;
        }

        protected internal virtual void ApplyCommandArg(object cmdTarget, ArgsParserState state)
        {
            if (ParseType == AttrParser.Ignore) return;

            // Resolve the value (to apply)
            var val = ResolveValue(state);

            // Check if this is a property
            var propInfo = Target as PropertyInfo;
            if (propInfo != null)
            {
                // Set the calculated property
                propInfo.SetValue(cmdTarget, val);
            }

            // Check if this is a property
            var fieldInfo = Target as FieldInfo;
            if (fieldInfo != null)
            {
                // Set the calculated property
                fieldInfo.SetValue(cmdTarget, val);
            }
        }

        protected virtual object ResolveValue(ArgsParserState state)
        {
            // Check if there is a dynamic getter
            var val = HitValue;
            var arg = state.args[state.pos];
            var args = state.args;
            switch (ParseType)
            {
                case AttrParser.Ignore:
                    // Do nothing
                    return null;
                case AttrParser.UseAsValue:
                    // Use arg as the actual value
                    return arg;
                case AttrParser.UseHitValue:
                    // Use hit value (if set)
                    return val;
                case AttrParser.UseNextArg:
                    // Use the next argument in the list as the value
                    state.pos++; // Advance one place
                    if (args.Length <= state.pos) throw new Exception("Invalid nummber of arguments.");
                    return args[state.pos];
                case AttrParser.UseRegexGroupId:
                    var grpIndex = (int)val;
                    if (state.regex.Groups.Count <= grpIndex) throw new Exception("Invalid regular expression group index.");
                    return state.regex.Groups[grpIndex].Value;
                default: throw new Exception("The attribute parser type was not recodnised");
            }
        }
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Event, AllowMultiple = true)]
    public class ProtoCmdCall : ProtoAttr
    {
        public string Name { get; internal set; }
        public string Hint { get; internal set; }

        public virtual string[] Args { get; set; }
        public virtual Action<string[]> Callback { get; set; }

        public ProtoCmdCall(string name, string desc)
        {
            Name = name;
            Hint = desc;
        }

        protected internal virtual object InvokeCommand(object cmdObj, string cmdType, string[] args)
        {
            // Check if this is a property
            var eventInfo = Target as EventInfo;
            if (eventInfo != null)
            {
                // Set the calculated property
                return eventInfo.RaiseMethod.Invoke(cmdObj, args);
            }

            // Check if this is a property
            var methodInfo = Target as MethodInfo;
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
