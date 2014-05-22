using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TweakScale
{
    public struct ScalingFactor
    {
        public struct FactorSet
        {
            float _linear;

            public float squareRoot
            {
                get
                {
                    return (float)Math.Sqrt(_linear);
                }
            }
            public float linear
            {
                get
                {
                    return _linear;
                }
            }
            public float quadratic
            {
                get
                {
                    return _linear * _linear;
                }
            }
            public float cubic
            {
                get
                {
                    return _linear * _linear * _linear;
                }
            }

            public FactorSet(float factor)
            {
                _linear = factor;
            }
        }

        FactorSet _absolute;
        FactorSet _relative;
        int _index;

        public FactorSet absolute
        {
            get
            {
                return _absolute;
            }
        }
        public FactorSet relative
        {
            get
            {
                return _relative;
            }
        }
        public int index
        {
            get
            {
                return _index;
            }
        }

        public ScalingFactor(float abs, float rel, int idx)
        {
            _absolute = new FactorSet(abs);
            _relative = new FactorSet(rel);
            _index = idx;
        }
    }
}
