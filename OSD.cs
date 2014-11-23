using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TweakScale
{
    // ReSharper disable once InconsistentNaming
    public class OSD
    {
        private class Message
        {
            public String Text;
            public Color Color;
            public float HideAt;
        }

        private readonly List<Message> _msgs = new List<Message>();

        private static GUIStyle CreateStyle(Color color)
        {
            var style = new GUIStyle
            {
                stretchWidth = true,
                alignment = TextAnchor.MiddleCenter,
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                normal = { textColor = color }
            };
            return style;
        }

        private float CalcHeight()
        {
            var style = CreateStyle(Color.white);
            return _msgs.Aggregate(.0f, (a, m) => a + style.CalcSize(new GUIContent(m.Text)).y);
        }

        public void Update()
        {
            if (_msgs.Count == 0) return;
            _msgs.RemoveAll(m => Time.time >= m.HideAt);
            var h = CalcHeight();
            GUILayout.BeginArea(new Rect(0, Screen.height * 0.1f, Screen.width, h), CreateStyle(Color.white));
            _msgs.ForEach(m => GUILayout.Label(m.Text, CreateStyle(m.Color)));
            GUILayout.EndArea();
        }

        public void Error(String text)
        {
            AddMessage(text, XKCDColors.LightRed);
        }

        public void Warn(String text)
        {
            AddMessage(text, XKCDColors.Yellow);
        }

        public void Success(String text)
        {
            AddMessage(text, XKCDColors.Cerulean);
        }

        public void Info(String text)
        {
            AddMessage(text, XKCDColors.OffWhite);
        }

        public void AddMessage(String text, Color color, float shownFor = 3)
        {
            var msg = new Message { Text = text, Color = color, HideAt = Time.time + shownFor };
            _msgs.Add(msg);
        }
    }
}
