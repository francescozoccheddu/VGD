using UnityEngine;

namespace Wheeled.Core.Data
{
    public static class Scripts
    {
        public static SceneScript Scenes => s_ManagerScript.scenes;
        public static PlayerPreferencesScript PlayerPreferences => s_ManagerScript.playerPreferences;
        public static CollisionScript Collisions => s_ManagerScript.collisions;
        public static ActorScript Actors => s_ManagerScript.actors;

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

        private static ManagerScript s_managerScript;
    }
}