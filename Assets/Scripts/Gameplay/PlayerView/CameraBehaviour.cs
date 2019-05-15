using UnityEngine;
using UnityEngine.Rendering;

public class CameraBehaviour : MonoBehaviour
{
    #region Public Fields

    public new GameObject camera;
    public Transform[] localNoSeeNodes;

    #endregion Public Fields

    #region Internal Methods

    internal bool IsLocal { get; private set; }

    internal void SetLocal(bool _isLocal)
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

    #endregion Internal Methods

}