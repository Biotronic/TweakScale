using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

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
        struct ScalingMode
        {
            public string Exponent { get; set; }
            public bool UseRelativeScaling { get; set; }

            public ScalingMode(string exponent, bool useRelativeScaling)
                : this()
            {
                Exponent = exponent;
                UseRelativeScaling = useRelativeScaling;
            }
        }

        private string _id;
        private string _name;
        private Dictionary<string, ScalingMode> _exponents;
        private List<string> _ignores;
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
                _exponents = new Dictionary<string, ScalingMode>();
                _children = new Dictionary<string, ScaleExponents>();
                _ignores = new List<string>();
            }
            else
            {
                _exponents = source._exponents.Clone();
                _children = source
                    ._children
                    .Select(a => new KeyValuePair<string, ScaleExponents>(a.Key, a.Value.Clone()))
                    .ToDictionary(a => a.Key, a => a.Value);
                _ignores = new List<string>(source._ignores);
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

            _exponents = new Dictionary<string, ScalingMode>();
            _children = new Dictionary<string, ScaleExponents>();
            _ignores = new List<string>();

            foreach (var value in node.values.OfType<ConfigNode.Value>().Where(a=>a.name != "name"))
            {
                if (value.name.StartsWith("!"))
                {
                    _exponents[value.name.Substring(1)] = new ScalingMode(value.value, true);
                }
                else if (value.name.Equals("-ignore"))
                {
                    _ignores.Add(value.value);
                }
                else
                {
                    _exponents[value.name] = new ScalingMode(value.value, false);
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
            foreach (string value in source._ignores)
            {
                if (!destination._ignores.Contains(value))
                {
                    destination._ignores.Add(value);
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
        /// Rescales destination exponentValue according to its associated exponent.
        /// </summary>
        /// <param name="currentValue">The current exponentValue.</param>
        /// <param name="baseValue">The unscaled exponentValue, gotten from the prefab.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="factor">The rescaling factor.</param>
        /// <param name="relative">Whether to use relative or absolute scaling.</param>
        /// <returns>The rescaled exponentValue.</returns>
        static private void Rescale(MemberUpdater current, MemberUpdater baseValue, string name, ScalingMode scalingMode, ScalingFactor factor)
        {
            var exponentValue = scalingMode.Exponent;
            double exponent = double.NaN;
            double[] values = null;
            if (exponentValue.Contains(','))
            {
                if (factor.index == -1)
                {
                    Tools.LogWf("Value list used for freescale part exponent field {0}: {1}", name, exponentValue);
                    return;
                }
                values = Tools.ConvertString(exponentValue, new double[] { });
                if (values.Length <= factor.index)
                {
                    Tools.LogWf("Too few values given for {0}. Expected at least {1}, got {2}: {3}", name, factor.index + 1, values.Length, exponentValue);
                    return;
                }
            }
            else if (!double.TryParse(exponentValue, out exponent))
            {
                Tools.LogWf("Invalid exponent {0} for field {1}", exponentValue, name);
            }


            if (current.MemberType.GetInterface("IList") != null)
            {
                var v = (IList)current.Value;
                var v2 = (IList)baseValue.Value;

                for (int i = 0; i < v.Count && i < v2.Count; ++i)
                {
                    if (values != null)
                    {
                        v[i] = values[factor.index];
                    }
                    else if (!double.IsNaN(exponent))
                    {
                        if (v[i] is float)
                        {
                            v[i] = (float)v2[i] * Math.Pow(factor.relative.linear, exponent);
                        }
                        else if (v[i] is double)
                        {
                            v[i] = (double)v2[i] * Math.Pow(factor.relative.linear, exponent);
                        }
                        else if (v[i] is Vector3)
                        {
                            v[i] = (Vector3)v2[i] * (float)Math.Pow(factor.relative.linear, exponent);
                        }
                    }
                }
            }

            if (values != null)
            {
                current.Set(values[factor.index]);
            }
            else if (!double.IsNaN(exponent))
            {

                current.Scale(Math.Pow(scalingMode.UseRelativeScaling ? factor.relative.linear : factor.absolute.linear, exponent), baseValue);
            }
        }

        /// <summary>
        /// Rescale the field of <paramref name="obj"/> according to the exponents of the ScaleExponents and <paramref name="factor"/>.
        /// </summary>
        /// <param name="obj">The object to rescale.</param>
        /// <param name="baseObj">The corresponding object in the prefab.</param>
        /// <param name="factor">The new scale.</param>
        /// <param name="part">The part the object is on.</param>
        private void UpdateFields(object obj, object baseObj, ScalingFactor factor, Part part)
        {
            if ((object)obj == null)
            {
                return;
            }

            if (obj is PartModule && obj.GetType().Name != _id)
            {
                Tools.LogWf("This ScaleExponent is intended for {0}, not {1}", _id, obj.GetType().Name);
                return;
            }

            // Skip update if ignore module exists.
            if ((object)part != null)
            {
                foreach (string v in _ignores)
                {
                    if (part.Modules.Contains(v))
                    {
                        return;
                    }
                }
            }

            if (obj is IEnumerable)
            {
                UpdateEnumerable((IEnumerable)obj, (IEnumerable)baseObj, factor, part);
                return;
            }

            foreach (var nameExponentKV in _exponents)
            {
                var value = MemberUpdater.Create(obj, nameExponentKV.Key);
                if (value == null)
                {
                    continue;
                }
                var baseObjTmp = baseObj;
                if (nameExponentKV.Value.UseRelativeScaling)
                {
                    // Forced relative scaling. Don't even dare look at the prefab!
                    baseObjTmp = null;
                }

                var baseValue = MemberUpdater.Create(baseObj, nameExponentKV.Key);
                Rescale(value, baseValue ?? value, nameExponentKV.Key, nameExponentKV.Value, factor);
            }

            foreach (var _child in _children)
            {
                string childName = _child.Key;
                var childObjField = MemberUpdater.Create(obj, childName);
                if (childObjField != null && _child.Value != null)
                {
                    var baseChildObjField = MemberUpdater.Create(baseObj, childName);
                    _child.Value.UpdateFields(childObjField.Value, (baseChildObjField ?? childObjField).Value, factor, part);
                }
            }
        }

        /// <summary>
        /// For IEnumerables (arrays, lists, etc), we want to update the items, not the list.
        /// </summary>
        /// <param name="obj">The list whose items we want to update.</param>
        /// <param name="prefabObj">The corresponding list in the prefab.</param>
        /// <param name="factor">The scaling factor.</param>
        /// <param name="part">The part the object is on.</param>
        private void UpdateEnumerable(IEnumerable obj, IEnumerable prefabObj, ScalingFactor factor, Part part = null)
        {
            IEnumerable other = prefabObj;
            if (prefabObj == null || obj.StupidCount() != prefabObj.StupidCount())
            {
                other = ((object)null).Repeat().Take(obj.StupidCount());
            }

            foreach (var item in obj.Zip(other, ModuleAndPrefab.Create))
            {
                if (!string.IsNullOrEmpty(_name) && _name != "*") // Operate on specific elements, not all.
                {
                    var childName = item.Current.GetType().GetField("name");
                    if (childName != null)
                    {
                        if (childName.FieldType != typeof(string) || (string)childName.GetValue(item.Current) != _name)
                        {
                            continue;
                        }
                    }
                }
                UpdateFields(item.Current, item.Prefab, factor, part);
            }
        }

        struct ModuleAndPrefab
        {
            public object Current { get; set; }
            public object Prefab { get; set; }

            private ModuleAndPrefab(object current, object prefab)
                : this()
            {
                Current = current;
                Prefab = prefab;
            }

            public static ModuleAndPrefab Create(object current, object prefab)
            {
                return new ModuleAndPrefab(current, prefab);
            }
        }

        struct ModulesAndExponents
        {
            public object Current { get; set; }
            public object Prefab { get; set; }
            public ScaleExponents Exponents { get; set; }

            private ModulesAndExponents(ModuleAndPrefab modules, ScaleExponents exponents)
                : this()
            {
                Current = modules.Current;
                Prefab = modules.Prefab;
                Exponents = exponents;
            }

            public static ModulesAndExponents Create(ModuleAndPrefab modules, KeyValuePair<string, ScaleExponents> exponents)
            {
                return new ModulesAndExponents(modules, exponents.Value);
            }
        }

        public static void UpdateObject(Part part, Part prefabObj, Dictionary<string, ScaleExponents> exponents, ScalingFactor factor)
        {
            if (exponents.ContainsKey("Part"))
            {
                exponents["Part"].UpdateFields(part, prefabObj, factor, part);
            }

            var modulePairs = part.Modules.Zip(prefabObj.Modules, ModuleAndPrefab.Create);
            var modulesAndExponents = modulePairs.Join(exponents,
                                        modules => ((PartModule)modules.Current).moduleName,
                                        exps => exps.Key,
                                        ModulesAndExponents.Create).ToArray();

            foreach (var modExp in modulesAndExponents)
            {
                modExp.Exponents.UpdateFields(modExp.Current, modExp.Prefab, factor, part);
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
