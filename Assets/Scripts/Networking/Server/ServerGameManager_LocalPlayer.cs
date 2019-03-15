using UnityEngine;
using Wheeled.Gameplay;
using Wheeled.Gameplay.Movement;

namespace Wheeled.Networking.Server
{

    internal sealed partial class ServerGameManager
    {

        private readonly MovementController m_movementController;
        private readonly PlayerView m_view;

        private void StartLocalPlayer()
        {
            m_movementController.StartAt(RoomTime.Now);
        }

        private void UpdateLocalPlayer()
        {
            m_movementController.UpdateUntil(RoomTime.Now);
            m_view.Move(m_movementController.ViewSnapshot);
            m_view.Update(Time.deltaTime);
        }

    }

}
