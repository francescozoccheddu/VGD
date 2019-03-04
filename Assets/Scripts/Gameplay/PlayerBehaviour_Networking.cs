using UnityEngine;

namespace Wheeled.Gameplay
{

    public sealed partial class PlayerBehaviour
    {

        public bool isAuthoritative;

        // Simulation and input history
        public class History
        {

            public struct Node
            {
                public SimulationState simulation;
                public InputState input;
            }

            // Index of the first appended node still stored
            public int Oldest => Newest - Length + 1;
            // Index of the last appended node
            public int Newest { get; private set; }
            // Maximum number of nodes that can be stored simultaneously
            public int Length => m_nodes.Length;
            // Maximum time span between the newer and the oldest node
            public float Duration => Length * c_timestep;
            // Get a node by index
            public Node? this[int _index]
            {
                get
                {
                    if (_index == OldestValid)
                    {
                        return m_oldestValidCache;
                    }
                    else if (Contains(_index))
                    {
                        return m_nodes[_index % Length];
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            // Index of the oldest non-null node
            public int OldestValid { get; private set; }

            private readonly Node?[] m_nodes;
            private int m_QueueLast => Newest % Length;
            private Node? m_oldestValidCache;

            public static History CreateHistoryByDuration(float _minDuration)
            {
                return new History(Mathf.CeilToInt(_minDuration / c_timestep));
            }

            public History(int _length)
            {
                m_nodes = new Node?[_length];
                Reset();
            }

            public void Append(Node? _node)
            {
                if (this[Oldest] != null)
                {
                    OldestValid = Oldest;
                    m_oldestValidCache = this[Oldest];
                }
                Newest++;
                m_nodes[m_QueueLast] = _node;
            }

            private bool Contains(int _index)
            {
                // True if the index is valid and the node has not yet been overwritten
                return Newest - _index < Length && _index <= Newest && _index >= 0;
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

            public void Reset()
            {
                OldestValid = -1;
                Newest = -1;
                m_oldestValidCache = null;
            }

        }

        private const float c_historyDuration = 2.0f;

        private readonly History m_history = History.CreateHistoryByDuration(c_historyDuration);

        public void Move(int _node, InputState _input, SimulationState _calculatedSimulation)
        {
            if (_node > m_history.Newest)
            {
                while (m_history.Newest < _node)
                {
                    m_history.Append(null);
                }
                m_history.Append(new History.Node { input = _input, simulation = _calculatedSimulation });
            }
        }

    }

}
