using UnityEngine;
using UnityEngine.UI;
using Wheeled.Core.Data;

namespace Wheeled.Menu
{
    public sealed class ArenaToggleBehaviour : ListBehaviour.ItemPresenterBehaviour
    {

        public Text text;

        protected override void Present(int _index) => text.text = Scripts.Scenes.arenas[_index].name;

    }
}