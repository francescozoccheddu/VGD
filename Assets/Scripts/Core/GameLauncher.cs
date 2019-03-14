using UnityEngine;
using Wheeled.Gameplay;
using Wheeled.Networking;

namespace Wheeled.Core
{

    public sealed partial class GameLauncher : MonoBehaviour
    {

        private static GameLauncher s_instance;

        public static GameLauncher Instance
        {
            get
            {
                if (s_instance == null)
                {
                    GameObject gameObject = new GameObject
                    {
                        name = "GameManager"
                    };
                    s_instance = gameObject.AddComponent<GameLauncher>();
                }
                return s_instance;
            }
        }

        private GameLauncher()
        {
        }

        public void OnEnable()
        {
            DontDestroyOnLoad(gameObject);
        }

        public void Update()
        {
            RoomTime.Manager.Update();
            NetworkManager.instance.Update();
            Debugging.Printer.Debug("Time", RoomTime.Now);
        }

        private void OnDestroy()
        {
            NetworkManager.instance.Stop();
        }

    }

}
