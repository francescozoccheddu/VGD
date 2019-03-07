using LiteNetLib.Utils;
using UnityEngine;
using Wheeled.Core;
using Wheeled.Gameplay;

namespace Wheeled.Networking
{

    internal sealed partial class Client
    {

        private sealed class LocalPlayerEventListener : IPlayerEventListener
        {

            private readonly Client m_server;

            public LocalPlayerEventListener(Client _client)
            {
                m_server = _client;
            }

            public void Corrected(int _node, PlayerBehaviour.InputState _input, PlayerBehaviour.SimulationState _simulation)
            {
            }

            public void Died(PlayerBehaviour.Time _time, Vector3 _hitDirection, Vector3 _hitPoint, bool _exploded)
            {
            }

            public void Moved(int _node, PlayerBehaviour.InputState _input, PlayerBehaviour.SimulationState _calculatedSimulation)
            {
                NetDataWriter writer = new NetDataWriter();
                writer.Put(Message.Moved);
                writer.Put(_node);
                writer.Put(_input);
                writer.Put(_calculatedSimulation);
                m_server.m_server.Send(writer, LiteNetLib.DeliveryMethod.Unreliable);
            }

            public void Spawned(PlayerBehaviour.Time _time, byte _spawnPoint)
            {
            }

        }

    }

}
