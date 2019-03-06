using UnityEngine;

namespace Wheeled.Gameplay
{
    public sealed partial class PlayerBehaviour : MonoBehaviour
    {

        public MeshRenderer actorRenderer;
        public Transform actorTransform;
        public Camera actorCamera;

        private void Clamp()
        {
            if (m_presentationTime.Node < m_history.Oldest)
            {
                m_presentationTime.Node = m_history.Oldest;
                m_presentationTime.TimeSinceNode = 0.0f;
            }
        }

        private void UpdateActor()
        {
            if (isInteractive)
            {
                m_lastSimulationState.Apply(this);
                actorTransform.position = m_position;
            }
            else if (!isAuthoritative)
            {
                // Present actor
                int iPrevNode = m_presentationTime.Node;
                MoveHistory.Node? prevNode = null;

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

                int iNextNode = m_presentationTime.Node + 1;
                MoveHistory.Node? nextNode = null;

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
                        float elapsed = (m_presentationTime - new Time { Node = iPrevNode }).RealTime;
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
                        float elapsed = (m_presentationTime - new Time { Node = iPrevNode }).RealTime;
                        Simulate(predictedInput, elapsed);
                    }
                }
                else if (nextNode != null)
                {
                    // Next only
                    nextNode.Value.simulation.Apply(this);
                }

                actorTransform.position = m_position;

            }
            else
            {
                MoveHistory.Node? node = m_history[m_lastConfirmedNode];
                if (node != null)
                {
                    node.Value.simulation.Apply(this);
                    InputState predictedInput = node.Value.input;
                    predictedInput.dash = false;
                    predictedInput.jump = false;
                    float elapsed = (m_presentationTime - new Time { Node = m_lastConfirmedNode }).RealTime;
                    Simulate(predictedInput, elapsed);
                    actorTransform.position = m_position;
                }
            }
        }


    }
}
