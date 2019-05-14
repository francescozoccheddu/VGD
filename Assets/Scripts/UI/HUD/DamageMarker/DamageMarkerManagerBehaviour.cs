using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Wheeled.UI.HUD.DamageMarker
{
    public sealed class DamageMarkerManagerBehaviour : MonoBehaviour
    {

        public static DamageMarkerManagerBehaviour Instance { get; private set; }


        public GameObject markerPrefab;

        public void Add(Vector3 _position)
        {
            Instantiate(markerPrefab, transform)
                .GetComponent<DamageMarkerBehaviour>()
                .Set(_position);
        }

        public Camera Camera { get; private set; }

        private void Update()
        {
            Camera = Camera.main;
        }

        private void Awake()
        {
            Instance = this;
        }

    }
}
