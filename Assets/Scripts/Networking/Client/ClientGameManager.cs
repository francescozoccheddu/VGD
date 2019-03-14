using LiteNetLib.Utils;
using System.Collections.Generic;
using UnityEngine;
using Wheeled.Core;
using Wheeled.Gameplay;
using Wheeled.Gameplay.Movement;

namespace Wheeled.Networking.Client
{

    internal sealed partial class ClientGameManager : Updatable.ITarget, Client.IGameManager, MovementController.IFlushTarget
    {

        private readonly Updatable m_updatable;
        private const float c_controllerOffset = 0.5f;
        private readonly MovementController m_movementController;
        private readonly PlayerView m_view;
        private readonly Client.IServer m_server;

        public ClientGameManager(Client.IServer _server)
        {
            m_updatable = new Updatable(this, false)
            {
                IsRunning = true
            };
            m_server = _server;
            Debug.Log("ClientGameManager constructed");
            m_movementController = new MovementController(3.0f)
            {
                target = this
            };
            m_view = new PlayerView();
        }

        #region InteractivePlayer.IFlushTarget

        void MovementController.IFlushTarget.FlushSimulation(int _firstStep, IReadOnlyList<InputStep> _inputSteps, in SimulationStep _simulation)
        {
            Serializer.WriteSimulationMessage(_firstStep, _inputSteps, _simulation);
            m_server.Send(Serializer.writer, LiteNetLib.DeliveryMethod.Unreliable);
        }

        void MovementController.IFlushTarget.FlushSight(int _step, in Sight _sight)
        {
            Serializer.WriteSightMessage(_step, _sight);
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
                    if (!m_movementController.IsRunning)
                    {
                        m_movementController.StartAt(RoomTime.Now, new TimeStep(20, 0.0f));
                    }
                }
                break;
                case Message.SimulationCorrection:
                {
                    _reader.ReadSimulationCorrectionMessage(out int step, out SimulationStepInfo _simulation);
                    Debug.LogFormat("Reconciliation {0}", step);
                    m_movementController.Correct(step, _simulation);
                }
                break;
            }
        }

        void Client.IGameManager.Stopped()
        {
            RoomTime.Manager.Stop();
            m_updatable.IsRunning = false;
        }

        #endregion

        #region Room Update

        void Updatable.ITarget.Update()
        {
            RoomTime.Manager.Update();
            m_movementController.Update();
            m_view.Move(m_movementController.ViewSnapshot);
            m_view.Update(Time.deltaTime);
        }

        #endregion

    }

}
