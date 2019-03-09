using UnityEngine;

namespace Wheeled.Core
{

    internal static class ScriptManager
    {

        private static ActorScript s_actors;
        private static SceneScript s_scenes;

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

    }

}
