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
            if (isInteractive)
            {
                m_lastSimulationState.Apply(this);
                actorTransform.position = characterController.transform.position;
            }
            else
            {
                // Update actor time
                m_timeSinceLastActorNode += Time.deltaTime;
                m_actorNode += Mathf.FloorToInt(m_timeSinceLastActorNode / c_timestep);
                m_timeSinceLastActorNode %= c_timestep;

                // Clamp to history tail
                if (m_actorNode < m_history.Oldest)
                {
                    m_actorNode = m_history.Oldest;
                    m_timeSinceLastActorNode = 0.0f;
                }

                // Present actor
                int iPrevNode = m_actorNode;
                History.Node? prevNode = null;

                while (iPrevNode >= m_history.Oldest)
                {
                    prevNode = m_history[iPrevNode];
                    if (prevNode != null)
                    {
                        break;
                    }
                    iPrevNode--;
                }
                if (prevNode == null)
                {
                    iPrevNode = m_history.OldestValid;
                    prevNode = m_history[iPrevNode];
                }

                int iNextNode = m_actorNode + 1;
                History.Node? nextNode = null;

                while (iNextNode <= m_history.Newest)
                {
                    nextNode = m_history[iNextNode];
                    if (nextNode != null)
                    {
                        break;
                    }
                    iNextNode++;
                }

                if (prevNode != null)
                {
                    if (nextNode != null && iNextNode - iPrevNode > 1)
                    {
                        // Prev & Next but missing nodes
                        // Interpolate
                        float period = (iNextNode - iPrevNode) * c_timestep;
                        float elapsed = (m_actorNode - iPrevNode) * c_timestep + m_timeSinceLastActorNode;
                        float progress = elapsed / period;
                        SimulationState.Lerp(prevNode.Value.simulation, nextNode.Value.simulation, progress).Apply(this);
                    }
                    else
                    {
                        // Prev only or consecutive Prev & Next 
                        // Partial simulation
                        prevNode.Value.simulation.Apply(this);
                        InputState predictedInput = prevNode.Value.input;
                        predictedInput.dash = false;
                        predictedInput.jump = false;
                        Simulate(predictedInput, m_timeSinceLastActorNode);
                    }
                }
                else if (nextNode != null)
                {
                    // Next only
                    nextNode.Value.simulation.Apply(this);
                }

                actorTransform.position = characterController.transform.position;

            }
        }

    }
}
