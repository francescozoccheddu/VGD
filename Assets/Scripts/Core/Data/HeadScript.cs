using UnityEngine;

namespace Wheeled.Core.Data
{
    [CreateAssetMenu]
    public sealed class HeadScript : ScriptableObject
    {
        public GameObject prefab;
        public Texture2D icon;
    }
}