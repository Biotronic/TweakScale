using System;
using System.Net.Mail;
using Strategies.Effects;
using UnityEngine;

namespace TweakScale
{
    class Hotkeyable : MonoBehaviour
    {
        private readonly Hotkey _tempDisable;
        private readonly Hotkey _toggle;
        private bool _state;

        public bool State {
            get { return _state && !_tempDisable.IsTriggered; }
        }

        public Hotkeyable(string name, string tempDisableDefault, string toggleDefault, bool state)
        {
            _tempDisable = new Hotkey("Disable" + name, tempDisableDefault);
            _toggle = new Hotkey("Toggle" + name, toggleDefault);
            _state = state;
        }

// ReSharper disable once UnusedMember.Local
        private void Update()
        {
            if (_toggle.IsTriggered)
            {
                _state = !_state;
            }
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
