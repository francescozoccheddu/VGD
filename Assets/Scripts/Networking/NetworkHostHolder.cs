using UnityEngine;
using UnityEngine.SceneManagement;
using Wheeled.Gameplay;

namespace Wheeled.Networking
{

    internal interface INetworkHost
    {
        NetworkHostHolder holder { set; get; }
    }

    internal sealed class NetworkHostHolder : MonoBehaviour
    {
        public GameObject clientPrefab;
        public GameObject serverPrefab;
        public GameObject playerPrefab;

        public bool isServer;

        public PlayerBehaviour InstatiatePlayer()
        {
            GameObject gameObject = Instantiate(playerPrefab);
            return gameObject.GetComponent<PlayerBehaviour>();
        }

        private void InstantiateHosts()
        {
            GameObject targetPrefab = isServer ? serverPrefab : clientPrefab;
            GameObject gameObject = Instantiate(targetPrefab);
            Component[] spawners = gameObject.GetComponents(typeof(INetworkHost));
            foreach (INetworkHost c in spawners)
            {
                c.holder = this;
            }
        }

        private void OnSceneChanged(Scene _current, Scene _next)
        {
            if (_next.name != "MenuScene")
            {
                InstantiateHosts();
            }
        }

        private void OnEnable()
        {
            DontDestroyOnLoad(gameObject);
            SceneManager.activeSceneChanged += OnSceneChanged;
        }

        private void OnDisable()
        {
            SceneManager.activeSceneChanged -= OnSceneChanged;
        }

    }
}
