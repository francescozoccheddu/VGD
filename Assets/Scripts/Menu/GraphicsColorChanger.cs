using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Wheeled.Menu
{
    public sealed class GraphicsColorChanger : MonoBehaviour
    {

        public Graphic target;
        public Color color;
        public Graphic original;

        public bool Change { set => target.color = value ? color : original.color;  }

    }
}
