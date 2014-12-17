using System.Collections.Generic;
using UnityEngine;
using KSP.IO;

namespace TweakScale
{
    class Hotkeyable
    {
        private readonly OSD _osd;
        private readonly string _name;
        private readonly Hotkey _tempDisable;
        private readonly Hotkey _toggle;
        private bool _state;
        private readonly PluginConfiguration _config;

        public bool State {
            get { return _state && !_tempDisable.IsTriggered; }
        }

        public Hotkeyable(OSD osd, string name, ICollection<KeyCode> tempDisableDefault, ICollection<KeyCode> toggleDefault, bool state)
        {
            _config = HotkeyManager.Instance.Config;
            _osd = osd;
            _name = name;
            _tempDisable = new Hotkey("Disable " + name, tempDisableDefault);
            _toggle = new Hotkey("Toggle " + name, toggleDefault);
            _state = state;
            Load();
        }

        public Hotkeyable(OSD osd, string name, string tempDisableDefault, string toggleDefault, bool state)
        {
            _config = HotkeyManager.Instance.Config;
            _osd = osd;
            _name = name;
            _tempDisable = new Hotkey("Disable " + name, tempDisableDefault);
            _toggle = new Hotkey("Toggle " + name, toggleDefault);
            _state = state;
            Load();
        }

        private void Load()
        {
            Debug.Log("Getting value. Currently: " + _state);
            _state = _config.GetValue(_name, _state);
            Debug.Log("New value: " + _state);

            _config.SetValue(_name, _state);
            _config.save();
        }

        public void Update()
        {
            if (!_toggle.IsTriggered)
                return;
            _state = !_state;
            _osd.Info(_name + (_state ? " enabled." : " disabled."));
            _config.SetValue(_name, _state);
            _config.save();
        }

        public static bool operator true(Hotkeyable a)
        {
            return a.State;
        }

        public static bool operator false(Hotkeyable a)
        {
            return !a.State;
        }
    }
}
