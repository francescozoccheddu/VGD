using Wheeled.Gameplay;
using Wheeled.Gameplay.Movement;

namespace Wheeled.Networking.Server
{

    internal sealed partial class ServerGameManager
    {

        private readonly struct NetPlayer : MovementValidator.ICorrectionTarget, MovementValidator.IValidationTarget
        {

            private readonly ServerGameManager m_manager;
            public readonly int id;
            public readonly PlayerHolders.AuthoritativePlayerHolder player;
            public readonly NetworkManager.Peer peer;

            public NetPlayer(ServerGameManager _manager, int _id, PlayerHolders.AuthoritativePlayerHolder _player, NetworkManager.Peer _peer)
            {
                m_manager = _manager;
                id = _id;
                player = _player;
                peer = _peer;
                player.movementValidator.correctionTarget = this;
            }

            void MovementValidator.ICorrectionTarget.Corrected(int _step, in SimulationStepInfo _simulation)
            {
                Serializer.WriteSimulationCorrectionMessage(_step, _simulation);
                peer.Send(Serializer.writer, LiteNetLib.DeliveryMethod.Unreliable);
            }

            void MovementValidator.ICorrectionTarget.Rejected(int _step, bool _newer)
            {
            }

            void MovementValidator.IValidationTarget.Validated(int _step, in InputStep _input, in SimulationStep _simulation)
            {
            }

        }


    }

}
