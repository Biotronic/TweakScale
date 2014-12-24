using System;
using System.Reflection;
using UnityEngine;

namespace TweakScale
{
    public class MemberUpdater
    {
        private readonly object _object;
        private readonly FieldInfo _field;
        private readonly PropertyInfo _property;
        private readonly UI_FloatRange _floatRange;
        private const BindingFlags LookupFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        public static MemberUpdater Create(object obj, string name)
        {
            if (obj == null || name == null)
            {
                return null;
            }
            var objectType = obj.GetType();
            var field = objectType.GetField(name, LookupFlags);
            var property = objectType.GetProperty(name, LookupFlags);
            UI_FloatRange floatRange = null;
            BaseFieldList fields;
            if (obj is PartModule)
            {                
                fields = (obj as PartModule).Fields;
                if (fields == null)
                {
                    Debug.LogWarning("[TWEAKSCALE] MemberUpdater.Create(" + objectType.ToString() + ", " + name + "), PartModule.Fields call returns null!");
                    return null;
                }
                var fieldData = fields[name];
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
                if (_property != null)
                {
                    return _property.GetValue(_object, null);
                }
                return null;
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
                if (_property != null)
                {
                    return _property.PropertyType;
                }
                return null;
            }
        }

        public void Set(object value)
        {
            if (value.GetType() != MemberType && MemberType.GetInterface("IConvertible") != null &&
                value.GetType().GetInterface("IConvertible") != null)
            {
                value = Convert.ChangeType(value, MemberType);
            }

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

            var newValue = source.Value;
            if (MemberType == typeof(float))
            {
                RescaleFloatRange((float)scale);
                Set((float)newValue * (float)scale);
            }
            else if (MemberType == typeof(double))
            {
                RescaleFloatRange((float)scale);
                Set((double)newValue * scale);
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

        public string Name {
            get
            {
                if (_field != null)
                {
                    return _field.Name;
                }
                if (_property != null)
                {
                    return _property.Name;
                }
                return null;
            }
        }
    }
}
