using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Wheeled.Core.Data;

namespace Wheeled.Menu
{
    public sealed class ColorToggleBehaviour : ListBehaviour.ItemPresenterBehaviour
    {
        public Graphic graphic;

        protected override void Present(int _index) => graphic.color = Scripts.PlayerPreferences.colors[_index];
    }
}
