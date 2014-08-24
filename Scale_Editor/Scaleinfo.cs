using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TweakScale
{
    struct ScaleInfo
    {
        public string type;
        public float defaultScale;

        public static ScaleInfo DefaultValue
        {
            get
            {
                return new ScaleInfo("stack", 1.25f);
            }
        }

        public ScaleInfo(string type, float scale)
        {
            this.type = type;
            this.defaultScale = scale;
        }

        public static bool operator !=(ScaleInfo a, ScaleInfo b)
        {
            return !(a == b);
        }

        public static bool operator ==(ScaleInfo a, ScaleInfo b)
        {
            return a.defaultScale == b.defaultScale && a.type == b.type;
        }

        public override bool Equals(object obj)
        {
            if (obj is ScaleInfo)
            {
                return this == (ScaleInfo)obj;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return type.GetHashCode() ^ defaultScale.GetHashCode();
        }
    }
}
