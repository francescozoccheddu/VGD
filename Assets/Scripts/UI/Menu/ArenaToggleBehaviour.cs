using UnityEngine;
using UnityEngine.UI;
using Wheeled.Core.Data;

namespace Wheeled.UI.Menu
{
    public sealed class ArenaToggleBehaviour : ListBehaviour.ItemPresenterBehaviour
    {

        public RawImage image;

        protected override void Present(int _index) => image.texture = Scripts.Scenes.arenas[_index].icon;

    }
}