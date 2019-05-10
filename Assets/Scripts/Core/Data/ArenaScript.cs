using UnityEngine;

namespace Wheeled.Core.Data
{
    [CreateAssetMenu]
    public sealed class ArenaScript : ScriptableObject
    {
        #region Public Fields

        public new string name;
        public Texture2D icon;
        public int buildIndex;

        #endregion Public Fields
    }
}