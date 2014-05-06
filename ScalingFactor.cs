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
            double _quadratic;
            double _cubic;

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
                    return _quadratic;
                }
            }
            public double cubic
            {
                get
                {
                    return _cubic;
                }
            }

            public FactorSet(double factor)
            {
                _linear = factor;
                _quadratic = factor * factor;
                _cubic = factor * factor * factor;
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
