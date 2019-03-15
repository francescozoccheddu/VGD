using UnityEngine;
using Wheeled.Gameplay;
using Wheeled.Gameplay.Movement;

namespace Wheeled.Networking.Client
{

    internal sealed partial class ClientGameManager
    {

        private const float c_controllerOffset = 0.5f;
        private readonly MovementController m_movementController;
        private readonly PlayerView m_view;

        private void UpdateLocalPlayer()
        {
            m_movementController.UpdateUntil(RoomTime.Now + c_controllerOffset);
            m_view.Move(m_movementController.ViewSnapshot);
            m_view.Update(Time.deltaTime);
        }

    }

}
