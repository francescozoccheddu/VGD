using UnityEngine;
using UnityEngine.UI;
using Wheeled.Core.Data;

namespace Wheeled.Menu
{
    internal sealed class HeadPreferenceTabBehaviour : MonoBehaviour
    {
        #region Internal Properties

        internal int HeadIndex
        {
            get => m_headIndex;
            set
            {
                m_headIndex = value;
                image.texture = Scripts.PlayerPreferences.heads[value].icon;
            }
        }

        #endregion Internal Properties

        #region Public Fields

        public RawImage image;

        #endregion Public Fields

        #region Private Fields

        private int m_headIndex;

        #endregion Private Fields
    }
}