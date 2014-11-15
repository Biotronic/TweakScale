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
        UI_FloatRange _floatRange = null;

        public static MemberUpdater Create(object obj, string name)
        {
            if (obj == null)
            {
                return null;
            }
            var objectType = obj.GetType();
            var field = objectType.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var property = objectType.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            UI_FloatRange floatRange = null;
            if (obj is PartModule)
            {
                var fieldData = (obj as PartModule).Fields[name];
                if ((object)fieldData != null)
                {
                    var ctrl = fieldData.uiControlEditor;
                    if (ctrl is UI_FloatRange)
                    {
                        floatRange = ctrl as UI_FloatRange;
                    }
                }
            }

            if (property != null && property.GetIndexParameters().Length > 0)
            {
                Tools.LogWf("Property {0} on {1} requires indices, which TweakScale currently does not support.", name, objectType.Name);
                return null;
            }
            if (field == null && property == null)
            {
                Tools.LogWf("No valid member found for {0} in {1}", name, objectType.Name);
                return null;
            }

            return new MemberUpdater(obj, field, property, floatRange);
        }

        private MemberUpdater(object obj, FieldInfo field, PropertyInfo property, UI_FloatRange floatRange)
        {
            _object = obj;
            _field = field;
            _property = property;
            _floatRange = floatRange;
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

        public Type MemberType
        {
            get
            {
                if (_field != null)
                {
                    return _field.FieldType;
                }
                else if (_property != null)
                {
                    return _property.PropertyType;
                }
                return null;
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

        public void Scale(double scale, MemberUpdater source)
        {
            if (_field == null && _property == null)
            {
                return;
            }

            object newValue = Value;
            if (MemberType == typeof(float))
            {
                Set((float)newValue * (float)scale);
                RescaleFloatRange((float)scale);
            }
            else if (MemberType == typeof(double))
            {
                Set((double)newValue * scale);
                RescaleFloatRange((float)scale);
            }
            else if (MemberType == typeof(Vector3))
            {
                Set((Vector3)newValue * (float)scale);
            }
        }

        private void RescaleFloatRange(float factor)
        {
            if ((object)_floatRange == null)
            {
                return;
            }
            _floatRange.maxValue *= factor;
            _floatRange.minValue *= factor;
            _floatRange.stepIncrement *= factor;
        }
    }
}
