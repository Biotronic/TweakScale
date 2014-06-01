using System.Linq;

namespace TweakScale
{
    class ScaleConfig
    {
        private static ScaleConfig GetScaleConfig(string name)
        {
            var config = GameDatabase.Instance.GetConfigs("SCALETYPE").FirstOrDefault(a => a.name == name);
            return (object)config == null ? defaultConfig : new ScaleConfig(config.config);
        }

        private static ScaleConfig defaultConfig = new ScaleConfig();

        public bool isFreeScale = false;
        public float[] scaleFactors = { 0.625f, 1.25f, 2.5f, 3.75f, 5f };
        public string[] scaleNames = { "62.5cm", "1.25m", "2.5m", "3.75m", "5m" };
        public float[] massFactors = { 0.0f, 0.0f, 1.0f };
        public float minValue = 0.625f;
        public float maxValue = 5.0f;
        public float defaultScale = 1.25f;
        public string suffix = "m";

        private ScaleConfig()
        {
        }

        public ScaleConfig(ConfigNode config)
        {
            if ((object)config != null && Tools.ConfigValue(config, "name", "default") != "default")
            {
                var type = Tools.ConfigValue(config, "type", "default");
                var source = GetScaleConfig(type);

                isFreeScale = Tools.ConfigValue(config, "freeScale", defaultValue: source.isFreeScale);
                scaleFactors = Tools.ConfigValue(config, "scaleFactors", defaultValue: source.scaleFactors);
                massFactors = Tools.ConfigValue(config, "massFactors", defaultValue: source.massFactors);
                minValue = Tools.ConfigValue(config, "minScale", defaultValue: source.minValue);
                maxValue = Tools.ConfigValue(config, "maxScale", defaultValue: source.maxValue);
                scaleNames = Tools.ConfigValue(config, "scaleNames", defaultValue: source.scaleNames);
                suffix = Tools.ConfigValue(config, "suffix", defaultValue: source.suffix);

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
            result += "	massFactors = " + massFactors.ToString() + "\n";
            result += "	minValue = " + minValue.ToString() + "\n";
            result += "	maxValue = " + maxValue.ToString() + "\n";
            return result + "}";
        }
    }
}
