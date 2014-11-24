using System.Runtime.Hosting;
using KSP.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TweakScale
{
    public class Hotkey
    {
        private List<KeyCode> _modifiers = new List<KeyCode>();
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
                _modifiers = defaultKey.SkipLast().ToList();
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
            _modifiers = keys.SkipLast().ToList();

            var tmp = _modifiers.Aggregate(Tuple.Create(0, 0),
                (a, b) => Tuple.Create(a.Item1 + (Input.GetKey(b) ? 1 : 0), a.Item2 + (Input.GetKeyDown(b) ? 1 : 0)));
            var triggered = tmp.Item1 == _modifiers.Count && tmp.Item2 > 0;
        }

        public bool IsTriggered
        {
            get { return _modifiers.All(Input.GetKey) && Input.GetKeyDown(_trigger); }
        }
    }
}
