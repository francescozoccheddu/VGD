﻿using UnityEngine;

namespace Wheeled.Core.Data
{
    [CreateAssetMenu]
    public sealed class ManagerScript : ScriptableObject
    {
        #region Public Fields

        public CollisionScript collisions;

        public ActorScript actors;

        public PlayerPreferencesScript playerPreferences;

        public SceneScript scenes;

        #endregion Public Fields
    }
}