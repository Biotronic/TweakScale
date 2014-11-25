using System.Runtime.Hosting;
using KSP.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TweakScale
{
    public class Hotkey
    {
        private readonly Dictionary<KeyCode, bool> _modifiers = new Dictionary<KeyCode, bool>();
        private KeyCode _trigger = KeyCode.None;
        private readonly string _name;
        private readonly PluginConfiguration _config = PluginConfiguration.CreateForType<TweakScale>();

        public Hotkey(string name, ICollection<KeyCode> defaultKey)
        {
            _name = name;
            if (defaultKey.Count == 0)
            {
                Tools.LogWf("No keys for hotkey {0}. Need at least 1 key in defaultKey parameter, got none.", _name);
            }
            else
            {
                _trigger = defaultKey.Last();
                SetModifiers(defaultKey.SkipLast().ToList());
            }
            Load();
        }

        public Hotkey(string name, string defaultKey)
        {
            _name = name;
            ParseString(defaultKey);
            Load();
        }

        public void Load()
        {
            _config.load();
            var rawNames = _config.GetValue(_name, "");
            if (!string.IsNullOrEmpty(rawNames))
            {
                ParseString(rawNames);
            }
        }

        private void ParseString(string s)
        {
            _config.SetValue(_name, s);
            _config.save();

            var names = s.Split('+');
            var keys = names.Select(Enums.Parse<KeyCode>).ToList();
            _trigger = keys.Last();

            SetModifiers(keys.SkipLast().ToList());
        }

        private void SetModifiers(ICollection<KeyCode> mods)
        {
            _modifiers[KeyCode.RightShift] = mods.Contains(KeyCode.RightShift);
            _modifiers[KeyCode.LeftShift] = mods.Contains(KeyCode.LeftShift);
            _modifiers[KeyCode.RightControl] = mods.Contains(KeyCode.RightControl);
            _modifiers[KeyCode.LeftControl] = mods.Contains(KeyCode.LeftControl);
            _modifiers[KeyCode.RightAlt] = mods.Contains(KeyCode.RightAlt);
            _modifiers[KeyCode.LeftAlt] = mods.Contains(KeyCode.LeftAlt);
            _modifiers[KeyCode.RightApple] = mods.Contains(KeyCode.RightApple);
            _modifiers[KeyCode.RightCommand] = mods.Contains(KeyCode.RightCommand);
            _modifiers[KeyCode.LeftApple] = mods.Contains(KeyCode.LeftApple);
            _modifiers[KeyCode.LeftCommand] = mods.Contains(KeyCode.LeftCommand);
            _modifiers[KeyCode.LeftWindows] = mods.Contains(KeyCode.LeftWindows);
            _modifiers[KeyCode.RightWindows] = mods.Contains(KeyCode.RightWindows);
        }

        public bool IsTriggered
        {
            get
            {
                return _modifiers.All(a => Input.GetKey(a.Key) == a.Value) && Input.GetKeyDown(_trigger);
            }
        }
    }
}
