using Prototyped.Base.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Prototyped.Base.Commands
{
    public delegate void RunCommand(string[] args);

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class ProtoCmd : ProtoAttr, IConsoleCommand
    {
        public string Name { get; internal set; }
        public string HelpDesc { get; internal set; }
        public string ActionType { get; internal set; }
        public bool ActionRequired { get; internal set; }

        protected List<ProtoCmdArg> Args
        {
            get
            {
                if (_args == null)
                {
                    _args = GetCommandArgs(this);
                }
                return _args;
            }
        }
        protected List<ProtoCmdCall> Callers
        {
            get
            {
                if (_callers == null)
                {
                    _callers = GetCommandActions(this);
                }
                return _callers;
            }
        }

        private List<ProtoCmdArg> _args;
        private List<ProtoCmdCall> _callers;

        public ProtoCmd(string name, string desc)
        {
            Name = name;
            HelpDesc = desc;
        }

        #region Command Actions

        public virtual void RunCommand(string[] args)
        {
            // Create a new instance of the command object
            var attrs = Args;
            var callers = Callers;
            var cmdObj = Activator.CreateInstance(RuntimeTarget as Type);

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

        public virtual void ParseArguments(object cmdTarget, string[] args)
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

        public virtual object RunBaseCommand(object cmdTarget, string[] args)
        {
            var cmdRunnner = GetCommandRunner(this);
            if (cmdRunnner != null)
            {
                // Invoke the help text on the command
                return cmdRunnner.InvokeCommand(cmdTarget, "default", args);
            }
            else
            {
                // The command was run without any parameters, show help text
                ShowHelpText(args);
                return 0;
            }
        }

        public virtual void ShowHelpText(string[] args)
        {
            var cmdHelpText = GetCommandHelpText(this);
            if (cmdHelpText != null)
            {
                cmdHelpText.InvokeCommand(RuntimeTarget, "help", args);
            }
            else
            {
                var prefix = string.Empty;
                var suffix = string.Empty;

                // Parse the command list
                var cmdOpts = string.Empty;
                if (Callers.Any())
                {
                    prefix += "<action> ";
                    cmdOpts = "\r\n Command Actions:\r\n";
                    foreach (var cmd in Callers)
                    {
                        var pdr = 12;
                        var pfx = cmd.Prefix ?? string.Empty;
                        var txt = cmd.Hint.PrintLinesPaddedLeft(pfx, pdr);
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
                        var pfx = cmd.Prefix ?? string.Empty;
                        var txt = cmd.Hint.PrintLinesPaddedLeft(pfx, pdr);
                        cmdArgs += string.Format("\r\n  {0} {1}", cmd.Matches.PadRight(pdr), txt);
                    }
                    cmdArgs += "\r\n";
                }

                // Build up the current help text data
                var horz = "-------------------------------------------------------------------------------";
                var text = string.Format(@"{0}
 {1}
{0}
 Command Usage: {2} {3}{4}
{5}{6}
{0}", horz, HelpDesc, Name, prefix, suffix, cmdOpts, cmdArgs);

                Console.WriteLine(text);
            }
        }

        public virtual object InvokeCommmand(object cmdTarget, string cmdType, string[] args)
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
                return mapper[cmdType].InvokeCommand(cmdTarget, cmdType, args);
            }
            else if (!ActionRequired)
            {
                // Defaults to the base command
                return RunBaseCommand(cmdTarget, args);
            }

            // No action matching that type was found
            throw new Exception("Command '" + cmdType + "' not recondised.");
        }

        #endregion

        #region Static helper methods

        public static List<ProtoCmdArg> GetCommandArgs(ProtoCmd cmdAttr)
        {
            var list = new List<ProtoCmdArg>();
            if (cmdAttr == null) throw new Exception("Command attribute was not set.");
            if (cmdAttr.RuntimeTarget == null) throw new Exception("Target for the attribute was not set.");

            var attrType = cmdAttr.RuntimeTarget as Type;
            if (attrType == null)
            {
                //  Try and detect from runtime object if not a type
                attrType = cmdAttr.RuntimeTarget != null ? cmdAttr.RuntimeTarget.GetType() : null;
            }

            var attrFields = attrType.GetAttributedFields<ProtoCmdArg>();
            foreach (var attr in attrFields.Select(i => { i.Key.RuntimeTarget = i.Value; return i.Key; }))
            {
                list.Add(attr);
            }

            var attrProps = attrType.GetAttributedProperties<ProtoCmdArg>();
            foreach (var attr in attrProps.Select(i => { i.Key.RuntimeTarget = i.Value; return i.Key; }))
            {
                list.Add(attr);
            }

            return list;
        }

        public static List<ProtoCmdCall> GetCommandActions(ProtoCmd cmdAttr)
        {
            var list = new List<ProtoCmdCall>();
            if (cmdAttr == null) throw new Exception("Command attribute was not set.");
            if (cmdAttr.RuntimeTarget == null) throw new Exception("Target for the attribute was not set.");

            var attrType = cmdAttr.RuntimeTarget as Type;
            if (attrType == null)
            {
                //  Try and detect from runtime object if not a type
                attrType = cmdAttr.RuntimeTarget != null ? cmdAttr.RuntimeTarget.GetType() : null;
            }
            if (attrType != null)
            {
                var attrMethods = attrType.GetAttributedMethods<ProtoCmdCall>();
                foreach (var attr in attrMethods.Select(i => { i.Key.RuntimeTarget = i.Value; return i.Key; }))
                {
                    list.Add(attr);
                }

                var attrEvents = attrType.GetAttributedEvents<ProtoCmdCall>();
                foreach (var attr in attrEvents.Select(i => { i.Key.RuntimeTarget = i.Value; return i.Key; }))
                {
                    list.Add(attr);
                }
            }

            return list;
        }

        public static ProtoCmdRunner GetCommandRunner(ProtoCmd cmdAttr)
        {
            if (cmdAttr == null) throw new Exception("Command attribute was not set.");
            if (cmdAttr.RuntimeTarget == null) throw new Exception("Target for the attribute was not set.");

            var attrType = cmdAttr.RuntimeTarget as Type;
            if (attrType == null)
            {
                //  Try and detect from runtime object if not a type
                attrType = cmdAttr.RuntimeTarget != null ? cmdAttr.RuntimeTarget.GetType() : null;
            }
            if (attrType != null)
            {
                // Search in methods
                var attrMethods = attrType.GetAttributedMethods<ProtoCmdRunner>().Select(i =>
                {
                    i.Key.RuntimeTarget = i.Value;
                    return i.Key;
                }).FirstOrDefault();
                if (attrMethods != null) return attrMethods;

                // Search in events                
                var attrEvents = attrType.GetAttributedEvents<ProtoCmdRunner>().Select(i =>
                {
                    i.Key.RuntimeTarget = i.Value;
                    return i.Key;
                }).FirstOrDefault();
                if (attrEvents != null) return attrEvents;
            }

            return null;
        }

        public static ProtoCmdHelpText GetCommandHelpText(ProtoCmd cmdAttr)
        {
            if (cmdAttr == null) throw new Exception("Command attribute was not set.");
            if (cmdAttr.RuntimeTarget == null) throw new Exception("Target for the attribute was not set.");

            var attrType = cmdAttr.RuntimeTarget as Type;
            if (attrType == null)
            {
                //  Try and detect from runtime object if not a type
                attrType = cmdAttr.RuntimeTarget != null ? cmdAttr.RuntimeTarget.GetType() : null;
            }
            if (attrType != null)
            {
                var attrMethods = attrType.GetAttributedMethods<ProtoCmdHelpText>().Select(i =>
                {
                    i.Key.RuntimeTarget = i.Value;
                    return i.Key;
                }).FirstOrDefault();
                if (attrMethods != null) return attrMethods;

                var attrEvents = attrType.GetAttributedEvents<ProtoCmdHelpText>().Select(i =>
                {
                    i.Key.RuntimeTarget = i.Value;
                    return i.Key;
                }).FirstOrDefault();
                if (attrEvents != null) return attrEvents;
            }

            return null;
        }

        public static IConsoleCommand Instantiate<TCommmand>()
        {
            var attrs = typeof(TCommmand).GetAttributedTypes<ProtoCmd>().Select(i =>
            {
                var cmd = i.Key;

                cmd.RuntimeTarget = i.Value;

                return cmd;
            });
            if (!attrs.Any()) throw new Exception("Instantiation failed. The specified type does not contain the required attribute(s).");
            return attrs.First();
        }

        public static IEnumerable<ProtoCmd> FromAssembly(Assembly assembly)
        {
            var commands = assembly.GetAttributedTypes<ProtoCmd>();
            return commands.Select(i => { i.Key.RuntimeTarget = i.Value; return i.Key; });
        }

        #endregion
    }

    public static class ProtoCmdExtender
    {
        public static string PrintLinesPaddedLeft(this string input, string pfx, int pdr, int lim = 80)
        {
            var sep = string.Empty;
            var txt = string.Empty;
            var inp = pfx + input;
            var max = lim - 5 - pdr;
            while (inp.Length > max)
            {
                var sec = inp.Substring(0, max);
                var nwl = sec.LastIndexOf(Environment.NewLine);
                var lbr = sec.LastIndexOf(" ");
                if (nwl > 0 && nwl < max)
                {
                    inp = sec.Substring(nwl + Environment.NewLine.Length, sec.Length - nwl - Environment.NewLine.Length) + inp.Substring(max);
                    sec = sec.Substring(0, nwl).PadRight(max);
                }
                else if (lbr > 0)
                {
                    inp = sec.Substring(lbr + 1, sec.Length - lbr - 1) + inp.Substring(max);
                    sec = sec.Substring(0, lbr).PadRight(max);
                }
                txt += sep + sec;
                sep = "\r\n".PadRight(80 - max);
            }
            txt += sep + inp.PadRight(max);
            return txt;
        }
    }
}
