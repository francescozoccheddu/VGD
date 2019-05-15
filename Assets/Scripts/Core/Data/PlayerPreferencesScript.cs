using UnityEngine;

namespace Wheeled.Core.Data
{
    [CreateAssetMenu]
    public sealed class PlayerPreferencesScript : ScriptableObject
    {
        public HeadScript[] heads;
        public Color[] colors;
    }
}