using System.Linq;

namespace TweakScale
{
    /// <summary>
    /// Configuration values for TweakScale.
    /// </summary>
    class ScaleConfig
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

        private static ScaleConfig defaultConfig = new ScaleConfig();

        private float[] _scaleFactors = { 0.625f, 1.25f, 2.5f, 3.75f, 5f };
        private string[] _scaleNames = { "62.5cm", "1.25m", "2.5m", "3.75m", "5m" };

        public bool isFreeScale = false;
        public string[] techRequired = { "", "", "", "", "" };
        public float minValue = 0.625f;
        public float maxValue = 5.0f;
        public float defaultScale = 1.25f;
        public string suffix = "m";

        private static bool techUnlocked(string techId)
        {
            if (techId == "")
                return true;
            if (HighLogic.CurrentGame == null)
                return true;
            if (HighLogic.CurrentGame.Mode != Game.Modes.CAREER)
                return true;
            string persistentfile = KSPUtil.ApplicationRootPath + "saves/" + HighLogic.SaveFolder + "/persistent.sfs";
            ConfigNode config = ConfigNode.Load(persistentfile);
            ConfigNode gameconf = config.GetNode("GAME");
            ConfigNode[] scenarios = gameconf.GetNodes("SCENARIO");
            foreach (ConfigNode scenario in scenarios)
            {
                if (scenario.GetValue("name") == "ResearchAndDevelopment")
                {
                    ConfigNode[] techs = scenario.GetNodes("Tech");
                    foreach (ConfigNode technode in techs)
                    {
                        if (technode.GetValue("id") == techId)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public float[] scaleFactors
        {
            get
            {
                var result = _scaleFactors.ZipFilter(techRequired, techUnlocked).ToArray();
                return result;
            }
        }

        public string[] scaleNames
        {
            get
            {
                var result = _scaleNames.ZipFilter(techRequired, techUnlocked).ToArray();
                return result;
            }
        }

        private ScaleConfig()
        {
        }

        public ScaleConfig(ConfigNode config)
        {
            if ((object)config != null && Tools.ConfigValue(config, "name", "default") != "default")
            {
                var type = Tools.ConfigValue(config, "type", "default");
                var source = GetScaleConfig(type);

                isFreeScale   = Tools.ConfigValue(config, "freeScale",    defaultValue: source.isFreeScale);
                minValue      = Tools.ConfigValue(config, "minScale",     defaultValue: source.minValue);
                maxValue      = Tools.ConfigValue(config, "maxScale",     defaultValue: source.maxValue);
                suffix        = Tools.ConfigValue(config, "suffix",       defaultValue: source.suffix);
                _scaleFactors = Tools.ConfigValue(config, "scaleFactors", defaultValue: source._scaleFactors);
                _scaleNames   = Tools.ConfigValue(config, "scaleNames",   defaultValue: source._scaleNames).Select(a => a.Trim()).ToArray();
                techRequired  = Tools.ConfigValue(config, "techRequired", defaultValue: source.techRequired).Select(a=>a.Trim()).ToArray();

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
            }
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
