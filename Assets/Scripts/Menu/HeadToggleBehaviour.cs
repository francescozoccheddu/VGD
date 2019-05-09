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
    public sealed class HeadToggleBehaviour : ListBehaviour.ItemPresenterBehaviour
    {
        public RawImage image;

        protected override void Present(object _item) => image.texture = ((HeadScript) _item).icon;
    }
}
