using UnityEngine;
using UnityEngine.UI;
using Wheeled.Core.Data;

namespace Wheeled.Menu
{
    public sealed class HeadPreferenceTabBehaviour : MonoBehaviour, ListBehaviour.IListItem
    {
        #region Public Properties

        int ListBehaviour.IListItem.Index
        {
            get => m_headIndex;
            set
            {
                m_headIndex = value;
                image.texture = Scripts.PlayerPreferences.heads[value].icon;
            }
        }

        #endregion Public Properties

        #region Public Fields

        public RawImage image;

        #endregion Public Fields

        #region Private Fields

        private int m_headIndex;

        #endregion Private Fields
    }
}