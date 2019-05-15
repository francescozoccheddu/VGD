using UnityEngine;
using Wheeled.Core.Data;

namespace Wheeled.Gameplay.PlayerView
{

    public sealed class SocketsManagerBehaviour : MonoBehaviour
    {
        private static SocketsManagerBehaviour s_instance;
        public SocketBehaviour eye;
        public SocketBehaviour rifle;
        public SocketBehaviour rocket;

        public static SocketsManagerBehaviour Instance
        {
            get
            {
                if (s_instance == null)
                {
                    s_instance = Scripts.Actors.player.GetComponent<SocketsManagerBehaviour>();
                }
                return s_instance;
            }
        }

    }

}
