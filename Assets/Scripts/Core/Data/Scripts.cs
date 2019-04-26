using UnityEngine;

namespace Wheeled.Core.Data
{
    internal static class Scripts
    {
        #region Public Properties

        public static SocketScript Sockets => s_ManagerScript.sockets;
        public static SceneScript Scenes => s_ManagerScript.scenes;
        public static PlayerPreferencesScript PlayerPreferences => s_ManagerScript.playerPreferences;
        public static CollisionScript Collisions => s_ManagerScript.collisions;
        public static ActorScript Actors => s_ManagerScript.actors;

        #endregion Public Properties

        #region Private Properties

        private static ManagerScript s_ManagerScript
        {
            get
            {
                if (s_managerScript == null)
                {
                    s_managerScript = Resources.Load<ManagerScript>("Manager");
                }
                return s_managerScript;
            }
        }

        #endregion Private Properties

        #region Private Fields

        private static ManagerScript s_managerScript;

        #endregion Private Fields
    }
}