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
            double _linear;

            public double squareRoot
            {
                get
                {
                    return Math.Sqrt(_linear);
                }
            }
            public double linear
            {
                get
                {
                    return _linear;
                }
            }
            public double quadratic
            {
                get
                {
                    return _linear * _linear;
                }
            }
            public double cubic
            {
                get
                {
                    return _linear * _linear * _linear;
                }
            }

            public FactorSet(double factor)
            {
                _linear = factor;
            }
        }

        FactorSet _absolute;
        FactorSet _relative;

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

        public ScalingFactor(double abs, double rel)
        {
            _absolute = new FactorSet(abs);
            _relative = new FactorSet(rel);
        }
    }
}
