using UnityEngine.UI;
using Wheeled.Core.Data;

namespace Wheeled.Menu
{
    public sealed class HostArenaEntryBehaviour : ListBehaviour.ListItemBehaviour
    {
        #region Public Fields

        public Text text;

        #endregion Public Fields

        #region Protected Methods

        protected override void SetIndex(int _index)
        {
            text.text = Scripts.Scenes.arenas[_index].name.ToUpper();
        }

        #endregion Protected Methods
    }
}