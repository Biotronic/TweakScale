using System;
using System.Reflection;

namespace TweakScale
{
    /// <summary>
    /// Wraps a FieldInfo or PropertyInfo and provides a common interface for either.
    /// </summary>
    /// <typeparam name="T">Pretend the field/property is this type.</typeparam>
    public abstract class MemberChanger<T>
    {
        /// <summary>
        /// Get or set the value of the field/property.
        /// </summary>
        public abstract T Value
        {
            get;
            set;
        }

        /// <summary>
        /// The actual type of the field/property.
        /// </summary>
        public abstract Type MemberType
        {
            get;
        }

        /// <summary>
        /// Creates a wrapper for the field or property by the specified name.
        /// </summary>
        /// <param name="obj">The object that holds the member to wrap.</param>
        /// <param name="name">The name of the member.</param>
        /// <returns>The member, wrapped.</returns>
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

    /// <summary>
    /// Wraps a FieldInfo.
    /// </summary>
    /// <typeparam name="T">Pretend the field is this type.</typeparam>
    class FieldChanger<T> : MemberChanger<T>
    {
        private FieldInfo fi;
        private object obj;

        public FieldChanger(object o, FieldInfo f)
        {
            if (f.FieldType.GetInterface("IConvertible") != null)
            {
                fi = f;
                obj = o;
            }
        }

        public override T Value
        {
            get
            {
                return ConvertEx.ChangeType<T>(fi.GetValue(obj));
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

    /// <summary>
    /// Wraps a PropertyInfo.
    /// </summary>
    /// <typeparam name="T">Pretend the property is this type.</typeparam>
    class PropertyChanger<T> : MemberChanger<T>
    {
        private PropertyInfo pi;
        private object obj;

        public PropertyChanger(object o, PropertyInfo p)
        {
            if (p.PropertyType.GetInterface("IConvertible") != null)
            {
                pi = p;
                obj = o;
            }
        }

        public override T Value
        {
            get
            {
                return ConvertEx.ChangeType<T>(pi.GetValue(obj, null));
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
