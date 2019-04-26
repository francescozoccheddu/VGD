using UnityEngine;

namespace Wheeled.Core.Data
{
    [CreateAssetMenu]
    public sealed class HeadScript : ScriptableObject
    {
        #region Public Fields

        public GameObject prefab;
        public Texture2D icon;
        public string defaultName;

        #endregion Public Fields
    }
}