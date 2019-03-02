using UnityEngine;

namespace Wheeled.Gameplay
{
    public sealed partial class PlayerBehaviour : MonoBehaviour
    {

        public MeshRenderer actorRenderer;
        public Transform actorTransform;
        public Camera actorCamera;

        private int m_actorNode;
        private float m_timeSinceLastActorNode;

        private void SetActorNode(int _node, bool _clamp)
        {

        }

        private void UpdateActor()
        {
            actorTransform.position = characterController.transform.position;
            actorTransform.rotation = characterController.transform.rotation;
        }

    }
}
