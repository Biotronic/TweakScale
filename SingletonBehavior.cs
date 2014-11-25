using TweakScale.Annotations;
using UnityEngine;

namespace TweakScale
{
    class SingletonBehavior<T> : MonoBehaviour where T : SingletonBehavior<T>
    {
        public static T Instance { get; protected set; }

        [UsedImplicitly]
        private void Awake()
        {
            Instance = (T)this;
        }
    }
}
