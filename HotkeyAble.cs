using System.Collections.Generic;
using System.Security.Policy;
using TweakScale.Annotations;
using UnityEngine;
using KSP.IO;

namespace TweakScale
{
    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    internal class HotkeyManager : SingletonBehavior<HotkeyManager>
    {
        private readonly OSD _osd = new OSD();
        private readonly Dictionary<string, Hotkeyable> _hotkeys = new Dictionary<string, Hotkeyable>();

        [UsedImplicitly]
        private void OnGUI()
        {
            _osd.Update();
        }

        [UsedImplicitly]
        private void Update()
        {
            foreach (var key in _hotkeys.Values)
            {
                key.Update();
            }
        }

        public Hotkeyable AddHotkey(string name, ICollection<KeyCode> tempDisableDefault, ICollection<KeyCode> toggleDefault, bool state)
        {
            if (_hotkeys.ContainsKey(name))
                return _hotkeys[name];
            return _hotkeys[name] = new Hotkeyable(_osd, name, tempDisableDefault, toggleDefault, state);
        }
    }

    class Hotkeyable
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

        public Hotkeyable(OSD osd, string name, ICollection<KeyCode> tempDisableDefault, ICollection<KeyCode> toggleDefault, bool state)
        {
            _osd = osd;
            _name = name;
            _tempDisable = new Hotkey("Disable" + name, tempDisableDefault);
            _toggle = new Hotkey("Toggle" + name, toggleDefault);
            _state = state;
            Load();
        }

        public Hotkeyable(OSD osd, string name, string tempDisableDefault, string toggleDefault, bool state)
        {
            _osd = osd;
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
