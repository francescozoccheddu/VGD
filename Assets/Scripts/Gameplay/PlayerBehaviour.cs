﻿using UnityEngine;
using Wheeled.Core;

namespace Wheeled.Gameplay
{
    public sealed partial class PlayerBehaviour : MonoBehaviour
    {

        internal IPlayerEventListener host;

        private Time m_presentationTime;

        private bool m_canSpawn = false;

        private void Start()
        {
            actorRenderer.enabled = false;
            m_presentationTime = new Time(3, 0);
            m_validationTime = Time.zero;
            m_lastStatusTime = Time.zero;
        }

        private void Update()
        {
            m_presentationTime += UnityEngine.Time.deltaTime;
            if (m_presentationTime > m_validationTime)
            {
                m_validationTime = m_presentationTime;
            }

            Clamp();

            UpdateStatus();
            if (isInteractive)
            {
                ProcessInput();
            }
            else if (isAuthoritative)
            {
                ConfirmSimulation();
            }
            UpdateActor();
        }

    }
}
