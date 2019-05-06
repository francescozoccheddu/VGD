using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Wheeled.Gameplay.PlayerView
{
    public sealed class HeadBehaviour : MonoBehaviour
    {

        private GameObject m_head;

        public Transform socket;

        internal void SetHead(GameObject _head)
        {
            if (m_head != null)
            {
                Destroy(m_head);
            }
            m_head = Instantiate(_head, socket);
            var cameraBehaviour = GetComponent<CameraBehaviour>();
            cameraBehaviour.SetLocal(cameraBehaviour.IsLocal);
        }

    }
}
