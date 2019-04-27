﻿using UnityEngine;

namespace Wheeled.Core.Data
{
    [CreateAssetMenu]
    public sealed class SceneScript : ScriptableObject
    {
        #region Public Fields

        public ArenaScript[] arenas;
        public int menuSceneBuildIndex;

        #endregion Public Fields
    }
}