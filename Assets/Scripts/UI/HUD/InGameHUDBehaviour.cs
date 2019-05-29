using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Wheeled.UI.HUD
{
    public sealed class InGameHUDBehaviour : MonoBehaviour
    {

        public HitMarkerBehaviour hitMarker;
        public HealthIndicatorBehaviour healthIndicator;
        public CrossHairBehaviour leftCrossHair;
        public CrossHairBehaviour rightCrossHair;

        public GameObject aliveOnlyGroup;

        public static InGameHUDBehaviour Instance { get; private set; }

        public void SetAlive(bool _alive)
        {
            aliveOnlyGroup.SetActive(_alive);
        }

        private void Awake()
        {
            Instance = this;
        }

    }
}
