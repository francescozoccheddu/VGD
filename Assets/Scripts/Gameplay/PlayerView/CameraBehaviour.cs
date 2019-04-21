﻿using UnityEngine;

public class CameraBehaviour : MonoBehaviour
{
    #region Public Fields

    public new Camera camera;
    public MeshRenderer[] localNoSeeMeshes;

    #endregion Public Fields

    #region Internal Methods

    internal void SetLocal(bool _isLocal)
    {
        camera.enabled = _isLocal;
        foreach (MeshRenderer m in localNoSeeMeshes)
        {
            m.enabled = !_isLocal;
        }
    }

    #endregion Internal Methods

    #region Private Methods

    private void Start()
    {
    }

    #endregion Private Methods
}