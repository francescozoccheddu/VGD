﻿using UnityEngine;

namespace Wheeled.Gameplay
{

    public sealed partial class PlayerBehaviour
    {

        private class History
        {

            public struct Node
            {
                public SimulationState simulation;
                public InputState input;
            }

            public int Last { get; private set; }
            public int Length => m_nodes.Length;
            public float Duration => Length * c_timestep;
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

            private bool Contains(int _index)
            {
                return Last - _index < Length && _index <= Last;
            }

            public bool Reconciliate(Node _node, int _index, PlayerBehaviour _target)
            {
                if (!Contains(_index))
                {
                    return false;
                }

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
