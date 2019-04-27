using UnityEngine.UI;
using Wheeled.Core.Data;

namespace Wheeled.Menu
{
    public sealed class HeadPreferenceTabBehaviour : ListBehaviour.ListItemBehaviour
    {
        #region Public Fields

        public RawImage image;

        #endregion Public Fields

        #region Protected Methods

        protected override void SetIndex(int _index)
        {
            image.texture = Scripts.PlayerPreferences.heads[_index].icon;
        }

        #endregion Protected Methods
    }
}