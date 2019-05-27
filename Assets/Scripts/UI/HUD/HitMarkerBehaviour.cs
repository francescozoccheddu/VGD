using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Wheeled.Sound;

namespace Wheeled.UI.HUD
{
    public sealed class HitMarkerBehaviour : MonoBehaviour
    {

        public Animator animator;

        public void Hit()
        {
            animator.SetTrigger("Hit");
            GetComponent<AudioSource>().Play();
        }

    }
}
