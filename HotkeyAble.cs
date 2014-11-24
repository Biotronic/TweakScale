using System.Collections.Generic;
using UnityEngine;
using KSP.IO;

namespace TweakScale
{
    class Hotkeyable : MonoBehaviour
    {
        private readonly OSD _osd;
        private readonly string _name;
        private readonly Hotkey _tempDisable;
        private readonly Hotkey _toggle;
        private bool _state;
        private readonly PluginConfiguration _config = PluginConfiguration.CreateForType<TweakScale>();

        public bool State {
            get { return _state && !_tempDisable.IsTriggered; }
        }

        public Hotkeyable(string name, ICollection<KeyCode> tempDisableDefault, ICollection<KeyCode> toggleDefault, bool state)
        {
            _osd = new OSD();
            _name = name;
            _tempDisable = new Hotkey("Disable" + name, tempDisableDefault);
            _toggle = new Hotkey("Toggle" + name, toggleDefault);
            _state = state;
            Load();
        }

        public Hotkeyable(string name, string tempDisableDefault, string toggleDefault, bool state)
        {
            _osd = new OSD();
            _name = name;
            _tempDisable = new Hotkey("Disable" + name, tempDisableDefault);
            _toggle = new Hotkey("Toggle" + name, toggleDefault);
            _state = state;
            Load();
        }

        private void Load()
        {
            _state = _config.GetValue(_name, _state);

            _config.SetValue(_name, _state);
            _config.save();
        }

// ReSharper disable once UnusedMember.Local
        private void OnGUI()
        {
            _osd.Update();
        }

// ReSharper disable once UnusedMember.Local
        private void Update()
        {
            if (!_toggle.IsTriggered) return;
            _state = !_state;
            _osd.Info(name + (_state? " enabled." : " disabled."));
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
