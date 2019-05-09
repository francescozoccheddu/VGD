using UnityEngine;
using UnityEngine.UI;
using Wheeled.Core.Data;

namespace Wheeled.Menu
{
    public sealed class HostArenaEntryBehaviour : ListBehaviour.ItemPresenterBehaviour
    {

        public Text text;

        protected override void Present(object _item) => text.text = ((ArenaScript) _item).name;

    }
}