using LiteNetLib.Utils;
using UnityEngine;

namespace Wheeled.Networking
{

    internal sealed class ClientGameManager : Client.IGameManager
    {

        public ClientGameManager()
        {
            Debug.Log("ClientGameManager constructed");
        }

        public void LatencyUpdated(int _latency)
        {
        }

        public void Received(NetDataReader _reader)
        {
        }

        public void Stopped()
        {
        }

    }

}
