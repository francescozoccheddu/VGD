using LiteNetLib.Utils;
using System.Collections.Generic;
using UnityEngine;
using Wheeled.Gameplay;
using Wheeled.Gameplay.Movement;

namespace Wheeled.Networking.Client
{

    internal sealed class ClientGameManager : Client.IGameManager, MovementController.IFlushTarget
    {

        private readonly Client.IServer m_server;
        private readonly PlayerHolders.InteractivePlayerHolder m_localPlayer;

        public ClientGameManager(Client.IServer _server)
        {
            m_server = _server;
            Debug.Log("ClientGameManager constructed");

            m_localPlayer = PlayerHolders.NewInteractivePlayer();
            m_localPlayer.m_movementController.target = this;

        }

        #region InteractivePlayer.IFlushTarget

        void MovementController.IFlushTarget.Flush(int _firstStep, IReadOnlyList<InputStep> _inputSteps, in Snapshot _snapshot)
        {
            Serializer.WriteMovementMessage(_firstStep, _inputSteps, _snapshot);
            m_server.Send(Serializer.writer, LiteNetLib.DeliveryMethod.Unreliable);
        }

        #endregion

        #region Client.IGameManager

        void Client.IGameManager.LatencyUpdated(float _latency)
        {
        }

        void Client.IGameManager.Received(NetDataReader _reader)
        {
            switch (_reader.ReadMessageType())
            {
                case Message.RoomUpdate:
                {
                    _reader.ReadRoomUpdateMessage(out TimeStep time);
                    Debug.LogFormat("RoomUpdate at {0} (oldTime={1}, diff={2})", time, RoomTime.Now, time - RoomTime.Now);
                    RoomTime.Manager.Set(time + m_server.Ping / 2.0f, RoomTime.IsRunning);
                    RoomTime.Manager.Start();
                    m_localPlayer.m_movementController.StartAt(RoomTime.Now, new TimeStep(10, 0.0f));
                }
                break;
                case Message.MovementCorrection:
                {
                    _reader.ReadMovementCorrectionMessage(out int step, out SimulationStepInfo _simulation);
                    Debug.LogFormat("Reconciliation {0}", step);
                    m_localPlayer.m_movementController.Correct(step, _simulation);
                }
                break;
            }
        }

        void Client.IGameManager.Stopped()
        {
            RoomTime.Manager.Stop();
        }

        #endregion

    }

}
