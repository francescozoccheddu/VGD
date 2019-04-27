using UnityEngine.UI;
using Wheeled.Core.Data;

namespace Wheeled.Menu
{
    public sealed class ColorPreferenceTabBehaviour : ListBehaviour.ListItemBehaviour
    {
        #region Public Fields

        public Graphic graphic;

        #endregion Public Fields

        #region Protected Methods

        protected override void SetIndex(int _index)
        {
            graphic.color = Scripts.PlayerPreferences.colors[_index];
        }

        #endregion Protected Methods
    }
}