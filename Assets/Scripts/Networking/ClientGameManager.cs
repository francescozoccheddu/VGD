using LiteNetLib.Utils;
using System.Collections.Generic;
using UnityEngine;
using Wheeled.Gameplay;

namespace Wheeled.Networking
{

    internal sealed class ClientGameManager : Client.IGameManager, InteractivePlayer.IFlushTarget
    {

        private readonly Client.IServer m_server;

        public ClientGameManager(Client.IServer _server)
        {
            m_server = _server;
            Debug.Log("ClientGameManager constructed");

            PlayerHolders.SpawnPlayerHolder().m_interactive.target = this;

        }

        #region InteractivePlayer.IFlushTarget

        void InteractivePlayer.IFlushTarget.Flush(int _firstStep, IReadOnlyList<InputStep> _inputSteps, in Snapshot _snapshot)
        {
            Debug.LogFormat("{0}, {1}", _firstStep, _inputSteps.Count);
        }

        #endregion

        #region Client.IGameManager

        void Client.IGameManager.LatencyUpdated(int _latency)
        {
        }

        void Client.IGameManager.Received(NetDataReader _reader)
        {
        }

        void Client.IGameManager.Stopped()
        {
        }

        #endregion

    }

}
