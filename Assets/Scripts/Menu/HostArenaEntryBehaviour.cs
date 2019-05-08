using UnityEngine;
using UnityEngine.UI;
using Wheeled.Core.Data;

namespace Wheeled.Menu
{
    public sealed class HostArenaEntryBehaviour : ToggleGroupBehaviour.ItemPresenterBehaviour
    {

        public Text text;

        protected override void Present(object _item) => text.text = ((ArenaScript) _item).name;

    }
}