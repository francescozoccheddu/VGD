﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Wheeled.Sound;

namespace Wheeled.Gameplay.PlayerView
{
    public sealed class WeaponsBehaviour : MonoBehaviour
    {

        public OneShotAudioPlayerBehaviour laserSound;
        public OneShotAudioPlayerBehaviour rocketSound;
        public Animator animator;

        public void ShootLaser(float _power)
        {
            animator.SetTrigger("Shoot Rifle");
            laserSound.Play(_power);
        }

        public void ShootRocket()
        {
            animator.SetTrigger("Shoot Rocket");
            rocketSound.Play();
        }

    }

}
