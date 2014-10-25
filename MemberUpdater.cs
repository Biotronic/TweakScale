using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace TweakScale
{
    public class MemberUpdater
    {
        object _object = null;
        FieldInfo _field = null;
        PropertyInfo _property = null;

        public MemberUpdater(object obj, string name)
        {
            if (obj == null)
            {
                return;
            }

            var objectType = obj.GetType();
            _object = obj;
            _field = objectType.GetField(name, BindingFlags.Instance | BindingFlags.Public);
            _property = objectType.GetProperty(name, BindingFlags.Instance | BindingFlags.Public);

            if (_property != null && _property.GetIndexParameters().Length > 0)
            {
                Tools.LogWf("Property {0} on {1} requires indices, which TweakScale currently does not support.", name, objectType.Name);
                _property = null;
            }
            if (_field == null && _property == null)
            {
                Tools.LogWf("No valid member found for {0} in {1}", name, objectType.Name);
            }
        }

        public object Value
        {
            get
            {
                if (_field != null)
                {
                    return _field.GetValue(_object);
                }
                else if (_property != null)
                {
                    return _property.GetValue(_object, null);
                }
                else
                {
                    return null;
                }
            }
        }

        public void Set(object value)
        {
            if (_field != null)
            {
                _field.SetValue(_object, value);
            }
            else if (_property != null)
            {
                _property.SetValue(_object, value, null);
            }
        }

        public Type MemberType
        {
            get
            {
                return _field == null ? _field.FieldType : _property.PropertyType;
            }
        }

        public void Scale(double scale, MemberUpdater source)
        {
            if (_field == null && _property == null)
            {
                return;
            }

            object newValue = Value;
            if (MemberType == typeof(float))
            {
                newValue = (float)newValue * (float)scale;
            }
            else if (MemberType == typeof(double))
            {
                newValue = (double)newValue * scale;
            }
            else if (MemberType == typeof(Vector3))
            {
                newValue = (Vector3)newValue * (float)scale;
            }
            Set(newValue);
        }
    }
}
