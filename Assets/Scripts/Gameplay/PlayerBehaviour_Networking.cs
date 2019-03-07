using System;
using UnityEngine;
using Wheeled.Core;

namespace Wheeled.Gameplay
{

    public sealed partial class PlayerBehaviour : IPlayerEventListener
    {

        public bool isAuthoritative;

        // Simulation and input history
        public class MoveHistory
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

            public static MoveHistory CreateHistoryByDuration(float _minDuration)
            {
                return new MoveHistory(Mathf.CeilToInt(_minDuration / c_timestep));
            }

            public MoveHistory(int _length)
            {
                m_nodes = new Node?[_length];
                Reset();
            }

            public void Set(Node? _node, int _index)
            {
                if (Contains(_index))
                {
                    m_nodes[_index % Length] = _node;
                }
                else if (_index > Newest)
                {
                    Newest = Math.Max(Newest, _index - Length + 1);
                    while (Newest < _index)
                    {
                        if (this[Oldest] != null)
                        {
                            OldestValid = Oldest;
                            m_oldestValidCache = this[Oldest];
                        }
                        Newest++;
                        m_nodes[m_QueueLast] = null;
                    }
                    m_nodes[m_QueueLast] = _node;
                }
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

                m_nodes[_index % Length] = _node;
                _node.simulation.Apply(_target);
                for (int i = _index + 1; i <= Newest; i++)
                {
                    Node badNode = m_nodes[i % Length].Value;
                    _target.Simulate(badNode.input, c_timestep);
                    badNode.simulation = SimulationState.Capture(_target);
                    m_nodes[i % Length] = badNode;
                }


                return true;
            }

            public void Reset()
            {
                OldestValid = -1;
                Newest = -1;
                m_oldestValidCache = null;
            }

        }

        private const float c_historyDuration = 5.0f;

        private readonly MoveHistory m_simulationHistory = MoveHistory.CreateHistoryByDuration(c_historyDuration);

        private int m_lastConfirmedNode;

        public void Corrected(int _node, InputState _input, SimulationState _simulation)
        {
            if (isInteractive && !isAuthoritative)
            {
                MoveHistory.Node node = new MoveHistory.Node { input = _input, simulation = _simulation };
                if (_node > m_simulationHistory.Newest)
                {
                    m_simulationHistory.Set(node, _node);
                    Debug.LogFormat("Reconciliation: node={0}", _node);
                }
                else
                {
                    if (m_simulationHistory.Reconciliate(node, _node, this))
                    {
                        if (m_accumulatedTime < c_timestep)
                        {
                            Simulate(GetAccumulatedInputForPartialSimulation(), m_accumulatedTime);
                        }
                        m_lastSimulationState = SimulationState.Capture(this);
                        Debug.LogFormat("Reconciliation: node={0}", _node);
                    }
                    else
                    {
                        Debug.LogFormat("Reconciliation failed: node={0}", _node);
                    }
                }
            }
        }

        public int lastArrivedNode;
        public int lastAcceptedNode;
        public int lastPresentationNode;
        public int lastConfirmedNode;
        public int lastCorrectedNode;

        private int m_lastReceivedNode;
        private float m_lastReceivedNodeTimestamp;

        private Time m_validationTime;

        public void DoPOA(float _ping)
        {
            float peerTimeSinceLastReceivedNode = UnityEngine.Time.realtimeSinceStartup - m_lastReceivedNodeTimestamp + _ping;
            Time peerTime = new Time(m_lastReceivedNode, peerTimeSinceLastReceivedNode);
            float offset = c_timestep * 2.0f + _ping * 1.25f;
            m_presentationTime = peerTime - offset;
            Debug.LogFormat("POA: offset={0} node={1} lastNode={2}", offset, m_presentationTime.Node, m_lastReceivedNode);
        }

        public void Moved(int _node, InputState _input, SimulationState _calculatedSimulation)
        {
            lastArrivedNode = _node;
            if (_node > m_lastReceivedNode)
            {
                m_lastReceivedNode = _node;
                m_lastReceivedNodeTimestamp = UnityEngine.Time.realtimeSinceStartup;
            }
            if (_node > m_lastConfirmedNode && (!isAuthoritative || _node < m_lastConfirmedNode + m_simulationHistory.Length))
            {
                m_simulationHistory.Set(new MoveHistory.Node { input = _input, simulation = _calculatedSimulation }, _node);
                lastAcceptedNode = _node;
            }
        }

        private void ConfirmSimulation()
        {
            if (m_lastConfirmedNode >= 0)
            {
                MoveHistory.Node? first = m_simulationHistory[m_lastConfirmedNode];
                if (!first.HasValue)
                {
                    return;
                }
                first.Value.simulation.Apply(this);
                InputState inputState = first.Value.input;
                while (m_lastConfirmedNode < m_validationTime.Node)
                {
                    m_lastConfirmedNode++;
                    MoveHistory.Node? node = m_simulationHistory[m_lastConfirmedNode];
                    inputState.dash = false;
                    inputState.jump = false;
                    if (node != null)
                    {
                        inputState = node.Value.input;
                    }
                    Simulate(inputState, c_timestep);
                    SimulationState calculatedSimulation = SimulationState.Capture(this);
                    m_simulationHistory.Set(new MoveHistory.Node { input = inputState, simulation = calculatedSimulation }, m_lastConfirmedNode);
                    host.Moved(m_lastConfirmedNode, inputState, calculatedSimulation);
                    if (node == null || !node.Value.simulation.IsNearlyEqual(calculatedSimulation))
                    {
                        host.Corrected(m_lastConfirmedNode, inputState, calculatedSimulation);
                        lastCorrectedNode = m_lastConfirmedNode;
                    }
                }
            }
            lastConfirmedNode = m_lastConfirmedNode;
            lastPresentationNode = m_presentationTime.Node;
        }

        private Time m_lastStatusTime;

        public void Spawned(Time _time, byte _spawnPoint)
        {
            if (!isAuthoritative)
            {
                if (isInteractive)
                {
                    if (_time > m_lastStatusTime)
                    {
                        Spawn(_spawnPoint);
                        m_lastStatusTime = _time;
                    }
                }
                else
                {
                    if (_time > m_validationTime)
                    {
                        m_actionHistory.Add(_time, new ActionHistory.SpawnAction { spawnPoint = _spawnPoint });
                    }
                    else
                    {
                        m_statusHistory.Add(_time, true);
                    }
                }
            }
        }

        public void Died(Time _time, Vector3 _hitDirection, Vector3 _hitPoint, bool _exploded)
        {
            if (!isAuthoritative)
            {
                if (isInteractive)
                {
                    if (_time > m_lastStatusTime)
                    {
                        Die(_hitDirection, _hitPoint, _exploded);
                        m_lastStatusTime = _time;
                    }
                }
                else
                {
                    if (_time > m_validationTime)
                    {
                        m_actionHistory.Add(_time, new ActionHistory.DieAction { hitDirection = _hitDirection, hitPoint = _hitPoint, exploded = _exploded });
                    }
                    else
                    {
                        m_statusHistory.Add(_time, false);
                    }
                }
            }
        }

    }

}