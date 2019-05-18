using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace Wheeled.Scene
{
    public sealed class PostProcessingBehaviour : MonoBehaviour
    {

        public PostProcessVolume volume;

        public int minQuality;

        private void Set()
        {
            volume.enabled = QualitySettings.GetQualityLevel() >= minQuality;
        }

        private void OnEnable()
        {
            Set();
        }

        private void OnValidate()
        {
            Set();
        }

    }
}
