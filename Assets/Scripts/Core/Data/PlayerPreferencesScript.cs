using UnityEngine;

namespace Wheeled.Core.Data
{
    [CreateAssetMenu]
    public sealed class PlayerPreferencesScript : ScriptableObject
    {
        #region Public Fields

        public HeadScript[] heads;
        public Color[] colors;

        #endregion Public Fields
    }
}