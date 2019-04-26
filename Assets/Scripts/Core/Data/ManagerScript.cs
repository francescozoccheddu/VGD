using UnityEngine;

namespace Wheeled.Core.Data
{
    [CreateAssetMenu]
    internal sealed class ManagerScript : ScriptableObject
    {
        #region Public Fields

        public CollisionScript collisions;

        public ActorScript actors;

        public PlayerPreferencesScript playerPreferences;

        public SceneScript scenes;

        public SocketScript sockets;

        #endregion Public Fields
    }
}