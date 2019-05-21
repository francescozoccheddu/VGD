using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Wheeled.Core.Data;

namespace Wheeled.UI.Menu
{
    public sealed class HeadToggleBehaviour : ListBehaviour.ItemPresenterBehaviour
    {
        public RawImage image;

        protected override void Present(int _index) => image.texture = Scripts.PlayerPreferences.heads[_index].icon;
    }
}
