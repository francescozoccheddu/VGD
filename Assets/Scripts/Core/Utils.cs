using System;
using UnityEngine;

namespace Wheeled.Core
{

    internal abstract class Updatable
    {

        private void Start()
        {
            if (!m_IsRunning)
            {
                m_gameObject = new GameObject("Updatable");
                if (m_dontDestroyOnLoad)
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
                m_updatable?.Update();
            }

            private void OnDestroy()
            {
                if (m_updatable != null && m_updatable.m_IsAutoRecreationEnabled)
                {
                    m_updatable.Start();
                    m_updatable = null;
                }
            }

        }

        private readonly bool m_dontDestroyOnLoad;
        private GameObject m_gameObject;

        protected Updatable(bool _dontDestroyOnLoad = false)
        {
            m_dontDestroyOnLoad = _dontDestroyOnLoad;
        }

        protected bool m_IsAutoRecreationEnabled { get; set; } = false;
        protected bool m_IsRunning
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

        protected abstract void Update();

    }

    internal interface IUpdatable
    {

        void Update();

    }

    internal sealed class UpdatableHolder : Updatable
    {

        public readonly IUpdatable updatable;

        public bool IsRunning { get => m_IsRunning; set => m_IsRunning = value; }
        public bool IsAutoRecreationEnabled { get => m_IsAutoRecreationEnabled; set => m_IsAutoRecreationEnabled = value; }

        public UpdatableHolder(IUpdatable _updatable, bool _dontDestroyOnLoad = false) : base(_dontDestroyOnLoad)
        {
            Debug.Assert(_updatable != null);
            updatable = _updatable;
        }

        protected override void Update()
        {
            updatable.Update();
        }

    }

}
