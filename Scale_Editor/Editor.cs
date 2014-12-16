using KSPAPIExtensions;
using System;
using System.Linq;

namespace TweakScale
{
    public class TweakScaleEditor : PartModule
    {
        [KSPField(isPersistant = false, guiActiveEditor = true, guiName = "Hide Part")]
        [UI_Toggle()]
        bool _hide = false;
        bool _oldHide = false;

        [KSPField(isPersistant = false, guiActiveEditor = true, guiName = "TweakScale this!")]
        [UI_Toggle()]
        bool _scaled = false;
        bool _oldScaled = false;


        [KSPField(isPersistant = false, guiActiveEditor = true, guiName = "Type")]
        [UI_ChooseOption(scene = UI_Scene.Editor)]
        public int _scaleTypeId = 0;
        private int _oldScaleTypeId = -1;

        [KSPField(isPersistant = false, guiActiveEditor = true, guiName = "Def. Scale", guiFormat = "S4", guiUnits = "m")]
        [UI_FloatEdit(scene = UI_Scene.Editor, minValue = 0.625f, maxValue = 5, incrementLarge = 1.25f, incrementSmall = 0.125f, incrementSlide = 0.001f)]
        public float _tweakScale = 1.25f;

        [KSPField(isPersistant = false, guiActiveEditor = false, guiName = "Def. Scale")]
        [UI_ChooseOption(scene = UI_Scene.Editor)]
        public int _tweakName = 0;

        ScaleType _cfg;
        BaseField _scale;
        BaseField _name;
        BaseField _type;
        UI_FloatEdit _scaleEdit;
        UI_ChooseOption _nameEdit;
        UI_ChooseOption _typeEdit;

        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);
            _scale = this.Fields["tweakScale"];
            _name = this.Fields["tweakName"];
            _type = this.Fields["scaleTypeId"];
            _scaleEdit = (UI_FloatEdit)_scale.uiControlEditor;
            _nameEdit = (UI_ChooseOption)_name.uiControlEditor;
            _typeEdit = (UI_ChooseOption)_type.uiControlEditor;

            _typeEdit.options = ScaleType.AllScaleTypes.Select(a => a.Name).ToArray();

            scaleInfo = ScaleDatabase.Lookup(part.partInfo.name);
        }

        ScaleInfo scaleInfo
        {
            get
            {
                return new ScaleInfo(_typeEdit.options[_scaleTypeId], _tweakScale);
            }
            set
            {
                _scaleTypeId = Array.IndexOf(_typeEdit.options, value.type);
                ChangeScaleType();
                _tweakScale = value.defaultScale;
                if (!_cfg.IsFreeScale)
                {
                    _tweakName = Tools.ClosestIndex(_tweakScale, _cfg.AllScaleFactors);
                    _tweakScale = _cfg.AllScaleFactors[_tweakName];
                }
            }
        }

        public void Update()
        {
            base.OnUpdate();

            if (_hide != _oldHide)
            {
                _oldHide = _hide;
                ScaleDatabase.Hide(part.partInfo.name, _hide);
            }
            _hide = _oldHide = ScaleDatabase.IsHidden(part.partInfo.name);

            _type.guiActiveEditor = _scaled;
            _scale.guiActiveEditor = _scaled && _cfg.IsFreeScale;
            _name.guiActiveEditor = _scaled && !_cfg.IsFreeScale;


            if (_oldScaleTypeId != _scaleTypeId)
            {
                ChangeScaleType();
            }

            if (!_cfg.IsFreeScale)
            {
                _tweakScale = _cfg.AllScaleFactors[_tweakName];
            }

            if (_scaled)
            {
                if (scaleInfo != ScaleDatabase.Lookup(part.partInfo.name) || _oldScaled != _scaled)
                {
                    ScaleDatabase.Update(part.partInfo.name, scaleInfo);
                    _oldScaled = _scaled;
                }
            }
            else
            {
                if (_oldScaled != _scaled)
                {
                    ScaleDatabase.Remove(part.partInfo.name);
                    _oldScaled = _scaled;
                }
            }
        }

        private void ChangeScaleType()
        {
            _oldScaleTypeId = _scaleTypeId;
            _cfg = ScaleType.AllScaleTypes[_scaleTypeId];
            if (_cfg.IsFreeScale)
            {
                _scaleEdit.minValue = _cfg.MinValue;
                _scaleEdit.maxValue = _cfg.MaxValue;
                _scaleEdit.incrementLarge = (float)Math.Round((_scaleEdit.maxValue - _scaleEdit.minValue) / 10, 2);
                _scaleEdit.incrementSmall = (float)Math.Round(_scaleEdit.incrementLarge / 10, 2);
                _tweakScale = _cfg.DefaultScale;
                _scale.guiUnits = _cfg.Suffix;
            }
            else
            {
                _nameEdit.options = _cfg.ScaleNames;
                _tweakName = Tools.ClosestIndex(_cfg.DefaultScale, _cfg.AllScaleFactors);
                _tweakScale = _cfg.AllScaleFactors[_tweakName];
            }
        }
    }
}
