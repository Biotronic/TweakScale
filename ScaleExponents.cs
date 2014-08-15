using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TweakScale
{
    [KSPAddon(KSPAddon.Startup.MainMenu, false)]
    class ScaleExponentsLoader : RescalableRegistratorAddon
    {
        public override void OnStart()
        {
            ScaleExponents.LoadGlobalExponents();
        }
    }

    public class ScaleExponents
    {
        private string _id;
        private string _name;
        private Dictionary<string, Tuple<string, bool>> _exponents;
        private Dictionary<string, ScaleExponents> _children;

        private static Dictionary<string, ScaleExponents> globalList;
        private static bool globalListLoaded = false;

        private const string exponentConfigName = "TWEAKSCALEEXPONENTS";

        private static bool IsExponentBlock(ConfigNode node)
        {
            return node.name == exponentConfigName || node.name == "MODULE";
        }

        /// <summary>
        /// Load all TWEAKSCALEEXPONENTS that are globally defined.
        /// </summary>
        public static void LoadGlobalExponents()
        {
            if (!globalListLoaded)
            {
                var tmp = GameDatabase.Instance.GetConfigs(exponentConfigName)
                    .Select(a => new ScaleExponents(a.config));

                globalList = tmp
                    .GroupBy(a => a._id)
                    .Select(a => a.Aggregate((b, c) => Merge(b, c)))
                    .ToDictionary(a => a._id, a => a);
                globalListLoaded = true;
            }
        }

        /// <summary>
        /// Creates modules copy of the ScaleExponents.
        /// </summary>
        /// <returns>A copy of the object on which the function is called.</returns>
        private ScaleExponents Clone()
        {
            return new ScaleExponents(this);
        }

        private ScaleExponents(ScaleExponents source)
        {
            _id = source._id;
            if (source == null)
            {
                _exponents = new Dictionary<string, Tuple<string, bool>>();
                _children = new Dictionary<string, ScaleExponents>();
            }
            else
            {
                _exponents = source._exponents.Clone();
                _children = source
                    ._children
                    .Select(a => new KeyValuePair<string, ScaleExponents>(a.Key, a.Value.Clone()))
                    .ToDictionary(a => a.Key, a => a.Value);
            }
        }

        private ScaleExponents(ConfigNode node, ScaleExponents source = null)
        {
            _id = IsExponentBlock(node) ? node.GetValue("name") : node.name;
            _name = node.GetValue("name");
            if (_id == null)
            {
                _id = "";
            }

            if (IsExponentBlock(node))
            {
                if (string.IsNullOrEmpty(_id))
                {
                    _id = "Part";
                    _name = "Part";
                }
            }

            _exponents = new Dictionary<string, Tuple<string, bool>>();
            _children = new Dictionary<string, ScaleExponents>();

            foreach (var value in node.values.OfType<ConfigNode.Value>().Where(a=>a.name != "name"))
            {
                if (value.name.StartsWith("!"))
                {
                    _exponents[value.name.Substring(1)] = Tuple.Create(value.value, true);
                }
                else
                {
                    _exponents[value.name] = Tuple.Create(value.value, false);
                }
            }

            foreach (var childNode in node.nodes.OfType<ConfigNode>())
            {
                _children[childNode.name] = new ScaleExponents(childNode);
            }

            if (source != null)
            {
                Merge(this, source);
            }
        }

        /// <summary>
        /// Merge two ScaleExponents. All the values in <paramref name="source"/> that are not already present in <paramref name="destination"/> will be added to <paramref name="destination"/>
        /// </summary>
        /// <param name="destination">The ScaleExponents to update.</param>
        /// <param name="source">The ScaleExponents to add to <paramref name="destination"/></param>
        /// <returns>The updated exponentValue of <paramref name="destination"/>. Note that this exponentValue is also changed, so using the return exponentValue is not necessary.</returns>
        public static ScaleExponents Merge(ScaleExponents destination, ScaleExponents source)
        {
            if (destination._id != source._id)
            {
                Tools.LogWf("Wrong merge target! A name {0}, B name {1}", destination._id, source._id);
            }
            foreach (var value in source._exponents)
            {
                if (!destination._exponents.ContainsKey(value.Key))
                {
                    destination._exponents[value.Key] = value.Value;
                }
            }
            foreach (var child in source._children)
            {
                if (destination._children.ContainsKey(child.Key))
                {
                    Merge(destination._children[child.Key], child.Value);
                }
                else
                {
                    destination._children[child.Key] = child.Value.Clone();
                }
            }
            return destination;
        }

        /// <summary>
        /// The names of all exponents this ScaleExponents set covers.
        /// </summary>
        public IEnumerable<string> Exponents
        {
            get
            {
                return _exponents.Keys;
            }
        }

        /// <summary>
        /// Rescales destination exponentValue according to its associated exponent.
        /// </summary>
        /// <param name="currentValue">The current exponentValue.</param>
        /// <param name="baseValue">The unscaled exponentValue, gotten from the prefab.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="factor">The rescaling factor.</param>
        /// <param name="relative">Whether to use relative or absolute scaling.</param>
        /// <returns>The rescaled exponentValue.</returns>
        public double Rescale(double currentValue, double baseValue, string name, ScalingFactor factor, bool relative = false)
        {
            if (!_exponents.ContainsKey(name))
            {
                Tools.LogWf("No exponent found for {0}.{1}", this._id, name);
                return currentValue;
            }

            var exponentValue = _exponents[name].Item1;
            if (exponentValue.Contains(','))
            {
                if (factor.index == -1)
                {
                    Tools.LogWf("Value list used for freescale part exponent field {0}: {1}", name, exponentValue);
                    return currentValue;
                }
                var values = Tools.ConvertString(exponentValue, new double[] { });
                if (values.Length == 0)
                {
                    Tools.LogWf("No valid values found for {0}: {1}", name, exponentValue);
                }
                if (values.Length <= factor.index)
                {
                    Tools.LogWf("Too few values given for {0}. Expected at least {1}, got {2}: {3}", name, factor.index + 1, values.Length, exponentValue);
                }
                return values[factor.index];
            }
            else
            {
                double exponent;
                if (double.TryParse(exponentValue, out exponent))
                {
                    if (relative)
                    {
                        return currentValue * Math.Pow(factor.relative.linear, exponent);
                    }
                    return baseValue * Math.Pow(factor.absolute.linear, exponent);
                }
                return currentValue;
            }
        }

        /// <summary>
        /// Rescale the field of <paramref name="obj"/> according to the exponents of the ScaleExponents and <paramref name="factor"/>.
        /// </summary>
        /// <param name="obj">The object to rescale.</param>
        /// <param name="baseObj">The corresponding object in the prefab.</param>
        /// <param name="factor">The new scale.</param>
        public void UpdateFields(object obj, object baseObj, ScalingFactor factor)
        {
            if (obj is PartModule && obj.GetType().Name != _id)
            {
                Tools.LogWf("This ScaleExponent is intended for {0}, not {1}", _id, obj.GetType().Name);
                return;
            }

            if (obj is IEnumerable)
            {
                UpdateEnumerable((IEnumerable)obj, (IEnumerable)baseObj, factor);
                return;
            }

            foreach (var fieldName in Exponents)
            {
                var baseObjTmp = baseObj;
                if (_exponents[fieldName].Item2)
                {
                    baseObjTmp = null;
                }
                var value = FieldChanger<double>.CreateFromName(obj, fieldName);
                if (value == null)
                {
                    continue;
                }
                if (baseObjTmp == null)
                {
                    // No prefab from which to grab values. Use relative scaling.
                    value.Value = Rescale(value.Value, value.Value, fieldName, factor, relative: true);
                }
                else
                {
                    var baseValue = FieldChanger<double>.CreateFromName(baseObj, fieldName);
                    value.Value = Rescale(value.Value, baseValue.Value, fieldName, factor);
                }
            }

            foreach (var _child in _children)
            {
                string childName = _child.Key;
                var childObjField = FieldChanger<object>.CreateFromName(obj, childName);
                if (childObjField != null)
                {
                    var baseChildObjField = FieldChanger<object>.CreateFromName(baseObj, childName);
                    _child.Value.UpdateFields(childObjField.Value, baseChildObjField.Value, factor);
                }
            }
        }

        /// <summary>
        /// For IEnumerables (arrays, lists, etc), we want to update the items, not the list.
        /// </summary>
        /// <param name="obj">The list whose items we want to update.</param>
        /// <param name="baseObj">The corresponding list in the prefab.</param>
        /// <param name="factor">The scaling factor.</param>
        private void UpdateEnumerable(IEnumerable obj, IEnumerable baseObj, ScalingFactor factor)
        {
            IEnumerable other = baseObj;
            if (baseObj == null || obj.StupidCount() != baseObj.StupidCount())
            {
                other = ((object)null).Repeat().Take(obj.StupidCount());
            }

            foreach (var item in obj.Zip(other))
            {
                if (!string.IsNullOrEmpty(_name) && _name != "*") // Operate on specific elements, not all.
                {
                    var childName = item.Item1.GetType().GetField("name");
                    if (childName != null)
                    {
                        if (childName.FieldType != typeof(string) || (string)childName.GetValue(item.Item1) != _name)
                        {
                            continue;
                        }
                    }
                }
                UpdateFields(item.Item1, item.Item2, factor);
            }
        }

        public static void UpdateObject(Part part, Part basePart, Dictionary<string, ScaleExponents> exps, ScalingFactor factor)
        {
            if (exps.ContainsKey("Part"))
            {
                exps["Part"].UpdateFields(part, basePart, factor);
            }

            var modulesAndExponents = part.Modules.Cast<PartModule>().Zip(basePart.Modules.Cast<PartModule>()).Join(exps, modules => modules.Item1.moduleName, exponents => exponents.Key, (modules, exponent) => Tuple.Create(modules, exponent.Value)).ToArray();

            foreach (var modExp in modulesAndExponents)
            {
                modExp.Item2.UpdateFields(modExp.Item1.Item1, modExp.Item1.Item2, factor);
            }
        }

        public static Dictionary<string, ScaleExponents> CreateExponentsForModule(ConfigNode node, Dictionary<string, ScaleExponents> parent)
        {
            var local = node.nodes
                .OfType<ConfigNode>()
                .Where(IsExponentBlock)
                .Select(a => new ScaleExponents(a))
                .ToDictionary(a => a._id);

            foreach (var pExp in parent.Values)
            {
                if (local.ContainsKey(pExp._id))
                {
                    Merge(local[pExp._id], pExp);
                }
                else
                {
                    local[pExp._id] = pExp;
                }
            }

            foreach (var gExp in globalList.Values)
            {
                if (local.ContainsKey(gExp._id))
                {
                    Merge(local[gExp._id], gExp);
                }
                else
                {
                    local[gExp._id] = gExp;
                }
            }

            return local;
        }
    }
}
