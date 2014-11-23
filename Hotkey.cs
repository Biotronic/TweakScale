using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TweakScale
{
    public class Hotkey
    {
        private List<KeyCode> _modifiers;
        private KeyCode _trigger;
        private readonly string _name;
        private readonly string _defaultKey;

        public Hotkey(string name, string defaultKey)
        {
            _name = name;
            _defaultKey = defaultKey;
        }

        public void Load()
        {
            var config = KSP.IO.PluginConfiguration.CreateForType<TweakScale>();
            config.load();
            var rawNames = config.GetValue(_name, _defaultKey);
            var names = rawNames.Split('+');
            config.SetValue(_name, rawNames);
            config.save(); // Recreate ScaleType in case it's deleted.

            var keys = names.Select(Enums.Parse<KeyCode>).ToList();
            _trigger = keys.Last();
            _modifiers = keys.SkipLast().ToList();

            foreach (var k in keys)
            {
                Debug.Log("Key: " + k);
            }
        }

        public bool IsTriggered
        {
            get { return _modifiers.All(Input.GetKey) && Input.GetKeyDown(_trigger); }
        }
    }
}
