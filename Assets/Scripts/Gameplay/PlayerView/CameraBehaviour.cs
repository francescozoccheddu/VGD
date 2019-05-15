using UnityEngine;
using UnityEngine.Rendering;

namespace Wheeled.Gameplay.PlayerView
{

    public class CameraBehaviour : MonoBehaviour
    {
        public new GameObject camera;
        public Transform[] localNoSeeNodes;

        public bool IsLocal { get; private set; }

        public void SetLocal(bool _isLocal)
        {
            camera.gameObject.SetActive(_isLocal);
            foreach (var t in localNoSeeNodes)
            {
                foreach (MeshRenderer m in t.GetComponentsInChildren<MeshRenderer>())
                {
                    m.shadowCastingMode = _isLocal ? ShadowCastingMode.ShadowsOnly : ShadowCastingMode.On;
                }
            }
            IsLocal = _isLocal;
        }

    }

}