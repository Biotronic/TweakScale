using System;

namespace TweakScale
{
    public struct ScalingFactor
    {
        public struct FactorSet
        {
            private float _linear;

            /// <summary>
            /// Linear scaling, i.e. length.
            /// </summary>
            public float linear
            {
                get
                {
                    return _linear;
                }
            }

            /// <summary>
            /// Quadratic scaling, i.e. surface area.
            /// </summary>
            public float quadratic
            {
                get
                {
                    return _linear * _linear;
                }
            }

            /// <summary>
            /// Cubic scaling, i.e. volume.
            /// </summary>
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

        /// <summary>
        /// Scale factors relative to the part's default scale.
        /// </summary>
        public FactorSet absolute
        {
            get
            {
                return _absolute;
            }
        }

        /// <summary>
        /// Scale factors relative the part's current scale.
        /// </summary>
        public FactorSet relative
        {
            get
            {
                return _relative;
            }
        }

        /// <summary>
        /// The 0-based index of the currently chosen scale. -1 if freely scalable.
        /// </summary>
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
