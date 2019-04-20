using UnityEngine;

namespace Wheeled.Core.Data
{
    internal static class ScriptManager
    {
        #region Public Properties

        public static ActorScript Actors
        {
            get
            {
                if (s_actors == null)
                {
                    s_actors = Resources.Load<ActorScript>("Actors");
                }
                return s_actors;
            }
        }
        public static SceneScript Scenes
        {
            get
            {
                if (s_scenes == null)
                {
                    s_scenes = Resources.Load<SceneScript>("Scenes");
                }
                return s_scenes;
            }
        }
        public static CollisionScript Collisions
        {
            get
            {
                if (s_collisions == null)
                {
                    s_collisions = Resources.Load<CollisionScript>("Collisions");
                }
                return s_collisions;
            }
        }

        #endregion Public Properties

        #region Private Fields

        private static ActorScript s_actors;
        private static SceneScript s_scenes;
        private static CollisionScript s_collisions;

        #endregion Private Fields
    }
}