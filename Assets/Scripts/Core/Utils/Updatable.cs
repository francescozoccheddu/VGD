﻿using System;
using UnityEngine;

namespace Wheeled.Core.Utils
{

    internal sealed class Updatable
    {

        public interface ITarget
        {

            void Update();

        }

        private void Start()
        {
            if (!IsRunning)
            {
                m_gameObject = new GameObject("Updatable");
                if (dontDestroyOnLoad)
                {
                    UnityEngine.Object.DontDestroyOnLoad(m_gameObject);
                }
                m_gameObject.AddComponent<UpdatableBehaviour>().SetUpdatable(this);
            }
        }

        private sealed class UpdatableBehaviour : MonoBehaviour
        {

            private Updatable m_updatable;

            public void SetUpdatable(Updatable _updatable)
            {
                if (m_updatable != null)
                {
                    throw new InvalidOperationException("Cannot change target after it has been set");
                }
                m_updatable = _updatable;
            }

            private void Update()
            {
                m_updatable.target.Update();
            }

            private void OnDestroy()
            {
                if (m_updatable != null && m_updatable.IsAutoRecreationEnabled)
                {
                    m_updatable.Start();
                    m_updatable = null;
                }
            }

        }

        public readonly bool dontDestroyOnLoad;
        private GameObject m_gameObject;
        public readonly ITarget target;

        public Updatable(ITarget _updatable, bool _dontDestroyOnLoad = false)
        {
            dontDestroyOnLoad = _dontDestroyOnLoad;
            target = _updatable;
        }

        public bool IsAutoRecreationEnabled { get; set; } = false;
        public bool IsRunning
        {
            get => m_gameObject != null;
            set
            {
                if (value)
                {
                    Start();
                }
                else
                {
                    if (m_gameObject != null)
                    {
                        UnityEngine.Object.Destroy(m_gameObject);
                        m_gameObject = null;
                    }
                }
            }
        }

    }

}