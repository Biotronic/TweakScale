using System.Collections.Generic;
using KSP.IO;
using TweakScale.Annotations;
using UnityEngine;

namespace TweakScale
{
    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    internal class HotkeyManager : SingletonBehavior<HotkeyManager>
    {
        private readonly OSD _osd = new OSD();
        private readonly Dictionary<string, Hotkeyable> _hotkeys = new Dictionary<string, Hotkeyable>();
        private readonly PluginConfiguration _config = PluginConfiguration.CreateForType<TweakScale>();

        public PluginConfiguration Config {
            get { return _config; }
        }

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

        public Hotkeyable AddHotkey(string hotkeyName, ICollection<KeyCode> tempDisableDefault, ICollection<KeyCode> toggleDefault, bool state)
        {
            if (_hotkeys.ContainsKey(hotkeyName))
                return _hotkeys[hotkeyName];
            return _hotkeys[hotkeyName] = new Hotkeyable(_osd, hotkeyName, tempDisableDefault, toggleDefault, state);
        }
    }
}
