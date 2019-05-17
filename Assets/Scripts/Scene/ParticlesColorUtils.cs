using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Wheeled.Scene
{
    public static class ParticlesColorUtils
    {

        public static void SetChildrenRendererColor(GameObject _object, Color _color)
        {
            foreach (var r in _object.GetComponentsInChildren<MeshRenderer>())
            {
                r.material.color = _color;
                r.material.SetColor("_PaintColor", _color);
            }
            foreach (var r in _object.GetComponentsInChildren<ParticleSystemRenderer>())
            {
                r.material.color = _color;
            }
        }

    }
}
