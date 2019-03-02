using UnityEngine;

namespace Wheeled.Gameplay
{

    public sealed partial class PlayerBehaviour
    {

        // Simulation and input history
        public class History
        {

            public struct Node
            {
                public SimulationState simulation;
                public InputState input;
            }

            // Index of the last appended node
            public int Last { get; private set; }
            // Maximum number of nodes that can be stored simultaneously
            public int Length => m_nodes.Length;
            // Maximum time span between the newer and the oldest node
            public float Duration => Length * c_timestep;
            // Get a node by index
            public Node this[int _index] => m_nodes[_index % Length];

            private readonly Node[] m_nodes;
            private int m_QueueLast => Last % Length;

            public static History CreateHistoryByDuration(float _minDuration)
            {
                return new History(Mathf.CeilToInt(_minDuration / c_timestep));
            }

            public History(int _length)
            {
                m_nodes = new Node[_length];
                Reset();
            }

            public void Append(Node _node)
            {
                Last++;
                m_nodes[m_QueueLast] = _node;
            }

            public bool Contains(int _index)
            {
                // True if the index is valid and the node has not yet been overwritten
                return Last - _index < Length && _index <= Last && _index >= 0;
            }

            // Replace a bad node and resimulate all subsequent nodes
            public bool Reconciliate(Node _node, int _index, PlayerBehaviour _target)
            {
                if (!Contains(_index))
                {
                    // The bad node has already been overwritten by newer nodes
                    return false;
                }

                // TODO Save first bad node in order to resimulate when _target will be not null

                // TODO Reconciliate

                return true;
            }

            public Node? GetOrNull(int _index)
            {
                return Contains(_index) ? this[_index] : (Node?) null;
            }

            public void Reset()
            {
                Last = -1;
            }

        }

        private const float c_historyDuration = 2.0f;

        private readonly History m_history = History.CreateHistoryByDuration(c_historyDuration);

    }

}
