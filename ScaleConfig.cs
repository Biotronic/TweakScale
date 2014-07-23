using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TweakScale
{
    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    public class TechUpdater : MonoBehaviour
    {
        public void Start()
        {
            Tech.Reload();
        }
    }

    public static class Tech
    {
        private static HashSet<string> unlockedTechs = new HashSet<string>();

        public static void Reload()
        {
            if (HighLogic.CurrentGame == null)
                return;
            if (HighLogic.CurrentGame.Mode != Game.Modes.CAREER)
                return;

            string persistentfile = KSPUtil.ApplicationRootPath + "saves/" + HighLogic.SaveFolder + "/persistent.sfs";
            ConfigNode config = ConfigNode.Load(persistentfile);
            ConfigNode gameconf = config.GetNode("GAME");
            ConfigNode[] scenarios = gameconf.GetNodes("SCENARIO");
            ConfigNode thisScenario = scenarios.FirstOrDefault(a => a.GetValue("name") == "ResearchAndDevelopment");
            ConfigNode[] techs = thisScenario.GetNodes("Tech");

            unlockedTechs = techs.Select(a => a.GetValue("id")).ToHashSet();
        }

        public static bool IsUnlocked(string techId)
        {
            if (HighLogic.CurrentGame == null)
                return true;
            if (HighLogic.CurrentGame.Mode != Game.Modes.CAREER)
                return true;
            if (techId == "")
                return true;
            return unlockedTechs.Contains(techId);
        }
    }

    /// <summary>
    /// Configuration values for TweakScale.
    /// </summary>
    public class ScaleConfig
    {
        /// <summary>
        /// Fetches the scale config with the specified name.
        /// </summary>
        /// <param name="name">The name of the config to fetch.</param>
        /// <returns>The specified config or the default config if none exists by that name.</returns>
        private static ScaleConfig GetScaleConfig(string name)
        {
            var config = GameDatabase.Instance.GetConfigs("SCALETYPE").FirstOrDefault(a => a.name == name);
            return (object)config == null ? defaultConfig : new ScaleConfig(config.config);
        }

        private static ScaleConfig[] configs;
        public static ScaleConfig[] AllConfigs
        {
            get
            {
                if (configs == null)
                {
                    configs = GameDatabase.Instance.GetConfigs("SCALETYPE").Select(a => new ScaleConfig(a.config)).ToArray();
                }
                return configs;
            }
        }

        private static ScaleConfig defaultConfig = new ScaleConfig();

        private float[] _scaleFactors = { 0.625f, 1.25f, 2.5f, 3.75f, 5f };
        private string[] _scaleNames = { "62.5cm", "1.25m", "2.5m", "3.75m", "5m" };
        public Dictionary<string, ScaleExponents> exponents = new Dictionary<string, ScaleExponents>(); 

        public bool isFreeScale = false;
        public string[] techRequired = { "", "", "", "", "" };
        public float minValue = 0.625f;
        public float maxValue = 5.0f;
        public float defaultScale = 1.25f;
        public string suffix = "m";
        public string name;

        public float[] scaleFactors
        {
            get
            {
                var result = _scaleFactors.ZipFilter(techRequired, Tech.IsUnlocked).ToArray();
                return result;
            }
        }

        public string[] scaleNames
        {
            get
            {
                var result = _scaleNames.ZipFilter(techRequired, Tech.IsUnlocked).ToArray();
                return result;
            }
        }

        private ScaleConfig()
        {
        }

        public ScaleConfig(ConfigNode config)
        {
            if ((object)config == null || Tools.ConfigValue(config, "name", "default") == "default")
            {
                return; // Default values.
            }

            var type = Tools.ConfigValue(config, "type", "default");
            var source = GetScaleConfig(type);

            isFreeScale   = Tools.ConfigValue(config, "freeScale",    defaultValue: source.isFreeScale);
            minValue      = Tools.ConfigValue(config, "minScale",     defaultValue: source.minValue);
            maxValue      = Tools.ConfigValue(config, "maxScale",     defaultValue: source.maxValue);
            suffix        = Tools.ConfigValue(config, "suffix",       defaultValue: source.suffix);
            _scaleFactors = Tools.ConfigValue(config, "scaleFactors", defaultValue: source._scaleFactors);
            _scaleNames   = Tools.ConfigValue(config, "scaleNames",   defaultValue: source._scaleNames).Select(a => a.Trim()).ToArray();
            techRequired  = Tools.ConfigValue(config, "techRequired", defaultValue: source.techRequired).Select(a=>a.Trim()).ToArray();
            name          = Tools.ConfigValue(config, "name",         defaultValue: "unnamed scaletype");

            if (_scaleFactors.Length != _scaleNames.Length)
            {
                Tools.Logf("Wrong number of scaleFactors compared to scaleNames: {0} vs {1}", _scaleFactors.Length, _scaleNames.Length);
            }

            if (techRequired.Length < _scaleFactors.Length)
            {
                techRequired = techRequired.Concat("".Repeat()).Take(_scaleFactors.Length).ToArray();
            }

            var tmpScale = Tools.ConfigValue(config, "defaultScale", defaultValue: source.defaultScale);
            if (!isFreeScale)
            {
                tmpScale = Tools.Closest(tmpScale, scaleFactors);
            }
            defaultScale = Tools.clamp(tmpScale, minValue, maxValue);

            exponents = ScaleExponents.CreateExponentsForModule(config, source.exponents);
        }

        public override string ToString()
        {
            string result = "ScaleConfig {\n";
            result += "	isFreeScale = " + isFreeScale.ToString() + "\n";
            result += "	scaleFactors = " + scaleFactors.ToString() + "\n";
            result += "	minValue = " + minValue.ToString() + "\n";
            result += "	maxValue = " + maxValue.ToString() + "\n";
            return result + "}";
        }
    }
}
