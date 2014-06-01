using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace TweakScale
{
    public abstract class MemberChanger<T>
    {
        public abstract T Value
        {
            get;
            set;
        }

        public abstract Type MemberType
        {
            get;
        }

        public static MemberChanger<T> CreateFromName(object obj, string name)
        {
            var field = obj.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Public);
            if (field != null)
            {
                return new FieldChanger<T>(obj, field);
            }

            var prop = obj.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public);
            if (prop != null)
            {
                return new PropertyChanger<T>(obj, prop);
            }
            return null;
        }
    }

    class FieldChanger<T> : MemberChanger<T>
    {
        private FieldInfo fi;
        private object obj;

        public FieldChanger(object o, FieldInfo f)
        {
            fi = f;
            obj = o;
        }

        public override T Value
        {
            get
            {
                return (T)Convert.ChangeType(fi.GetValue(obj), typeof(T));
            }
            set
            {
                fi.SetValue(obj, Convert.ChangeType(value, MemberType));
            }
        }

        public override Type MemberType
        {
            get
            {
                return fi.FieldType;
            }
        }
    }

    class PropertyChanger<T> : MemberChanger<T>
    {
        private PropertyInfo pi;
        private object obj;

        public PropertyChanger(object o, PropertyInfo p)
        {
            pi = p;
            obj = o;
        }

        public override T Value
        {
            get
            {
                return (T)Convert.ChangeType(pi.GetValue(obj, null), typeof(T));
            }
            set
            {
                pi.SetValue(obj, Convert.ChangeType(value, MemberType), null);
            }
        }

        public override Type MemberType
        {
            get
            {
                return pi.PropertyType;
            }
        }
    }
}
