using System.Net;
using UnityEngine;
using Wheeled.Networking;

namespace Wheeled.Core
{

    public struct GameRoomInfo
    {
        public IPEndPoint remoteEndPoint;
        public string name;
        public int map;
    }

    public sealed partial class GameManager : MonoBehaviour
    {

        private static GameManager s_instance;

        public static GameManager Instance
        {
            get
            {
                if (s_instance == null)
                {
                    GameObject gameObject = new GameObject
                    {
                        name = "GameManager"
                    };
                    s_instance = gameObject.AddComponent<GameManager>();
                }
                return s_instance;
            }
        }

        private GameManager()
        {
            m_networkManager = new NetworkManager();
        }

        public void OnEnable()
        {
            DontDestroyOnLoad(gameObject);
        }

        public void Update()
        {
            m_networkManager.Update();
        }

        private void OnDestroy()
        {
            m_networkManager.listener = null;
            m_networkManager.Stop();
            s_instance = null;
        }

    }

}
