using System.Collections.Generic;
using Wheeled.Gameplay.Movement;

namespace Wheeled.Networking.Client
{
    internal sealed partial class ClientGameManager
    {
        private sealed class NetPlayer : Player
        {
            public NetPlayer(ClientGameManager _manager, byte _id) : base(_manager, _id)
            {
            }

            public override bool IsLocal => false;

            public void Move(int _step, IEnumerable<InputStep> _reversedInputSteps, Snapshot _snapshot)
            {
                int step = _step;
                foreach (InputStep inputStep in _reversedInputSteps)
                {
                    PutInput(step, inputStep);
                    step--;
                }
                PutSimulation(_step, _snapshot.simulation);
                PutSight(_step, _snapshot.sight);
            }
        }
    }
}