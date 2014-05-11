using System.ComponentModel;
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
        public float[] scaleFactors = { 0.625f, 1.25f, 2.5f, 3.75f, 5.0f };
        public float[] massFactors = { 0.0f, 0.0f, 1.0f };
        public float minValue = 0.625f;
        public float maxValue = 5.0f;
        public float stepIncrement = 0.01f;
        public float defaultScale = 1.25f;

        private ScaleConfig()
        {
        }

        public ScaleConfig(ConfigNode config)
        {
            if ((object)config != null && Tools.configValue(config, "name", "default") != "default")
            {
                var type = Tools.configValue(config, "type", "default");
                var source = GetScaleConfig(type);

                isFreeScale = Tools.configValue(config, "freeScale", defaultValue: source.isFreeScale);
                scaleFactors = Tools.configValue(config, "scaleFactors", defaultValue: source.scaleFactors).OrderBy(a => a).ToArray();
                massFactors = Tools.configValue(config, "massFactors", defaultValue: source.massFactors);
                minValue = Tools.configValue(config, "minScale", defaultValue: source.minValue);
                maxValue = Tools.configValue(config, "maxScale", defaultValue: source.maxValue);
                stepIncrement = Tools.configValue(config, "stepIncrement", defaultValue: source.stepIncrement);

                var tmpScale = Tools.configValue(config, "defaultScale", defaultValue: source.defaultScale);
                tmpScale = Tools.closest(tmpScale, scaleFactors);
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
            result += "	stepIncrement = " + stepIncrement.ToString() + "\n";
            return result + "}";
        }
    }
}
