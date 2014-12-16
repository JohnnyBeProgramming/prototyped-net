using System;
using System.Reflection;

namespace Prototyped.Base.Commands
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class ProtoCmdArg : ProtoAttr
    {
        public string Matches { get; internal set; }
        public object HitValue { get; internal set; }
        public AttrParser ParseType { get; internal set; }

        public virtual string Hint { get; set; }
        public virtual string Prefix { get; set; }

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
            var propInfo = RuntimeTarget as PropertyInfo;
            if (propInfo != null)
            {
                // Set the calculated property
                propInfo.SetValue(cmdTarget, val);
            }

            // Check if this is a property
            var fieldInfo = RuntimeTarget as FieldInfo;
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
}
