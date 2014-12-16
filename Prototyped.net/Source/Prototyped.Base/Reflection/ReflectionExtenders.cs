using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Prototyped.Base
{
    public static class ReflectionExtenders
    {
        public static IEnumerable<KeyValuePair<TAttr, Type>> GetAttributedTypes<TAttr>(this Assembly assembly) where TAttr : Attribute
        {
            var list = new List<KeyValuePair<TAttr, Type>>();
            foreach (var type in assembly.GetTypes())
            {
                var attrs = type.GetAttributedTypes<TAttr>();
                if (attrs.Any())
                {
                    list.AddRange(attrs);
                }
            }
            return list;
        }

        public static IEnumerable<KeyValuePair<TAttr, Type>> GetAttributedTypes<TAttr>(this Type type) where TAttr : Attribute
        {
            var list = new List<KeyValuePair<TAttr, Type>>();
            var classAttrs = type.GetCustomAttributes(typeof(TAttr), true);
            foreach (var customAttr in classAttrs.Cast<TAttr>())
            {
                list.Add(new KeyValuePair<TAttr, Type>(customAttr, type));
            }
            return list;
        }

        public static IEnumerable<KeyValuePair<TAttr, FieldInfo>> GetAttributedFields<TAttr>(this Type type) where TAttr : Attribute
        {
            var list = new List<KeyValuePair<TAttr, FieldInfo>>();
            var fieldAttrs = type.GetFields();
            foreach (var field in fieldAttrs)
            {
                foreach (var customAttr in field.GetCustomAttributes<TAttr>())
                {
                    list.Add(new KeyValuePair<TAttr, FieldInfo>(customAttr, field));
                }
            }
            return list;
        }

        public static IEnumerable<KeyValuePair<TAttr, PropertyInfo>> GetAttributedProperties<TAttr>(this Type type) where TAttr : Attribute
        {
            var list = new List<KeyValuePair<TAttr, PropertyInfo>>();
            var propAttrs = type.GetProperties();
            foreach (var prop in propAttrs)
            {
                foreach (var customAttr in prop.GetCustomAttributes<TAttr>())
                {
                    list.Add(new KeyValuePair<TAttr, PropertyInfo>(customAttr, prop));
                }
            }
            return list;
        }

        public static IEnumerable<KeyValuePair<TAttr, MemberInfo>> GetAttributedMethods<TAttr>(this Type type) where TAttr : Attribute
        {
            var list = new List<KeyValuePair<TAttr, MemberInfo>>();
            var methodAttrs = type.GetMembers();
            foreach (var method in methodAttrs)
            {
                foreach (var customAttr in method.GetCustomAttributes<TAttr>())
                {
                    list.Add(new KeyValuePair<TAttr, MemberInfo>(customAttr, method));
                }
            }
            return list;
        }

        public static IEnumerable<KeyValuePair<TAttr, EventInfo>> GetAttributedEvents<TAttr>(this Type type) where TAttr : Attribute
        {
            var list = new List<KeyValuePair<TAttr, EventInfo>>();
            var eventAttrs = type.GetEvents();
            foreach (var evt in eventAttrs.Where(f => f.CustomAttributes.Any(a => a is TAttr)))
            {
                var customAttr = evt.GetCustomAttribute<TAttr>();
                list.Add(new KeyValuePair<TAttr, EventInfo>(customAttr, evt));
            }
            return list;
        }
    }
}
