using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Wheeled.Menu
{
    public sealed class ColorToggleBehaviour : ListBehaviour.ItemPresenterBehaviour
    {
        public Graphic graphic;

        protected override void Present(object _item) => graphic.color = (Color) _item;
    }
}
