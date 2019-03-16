using System;
using System.Collections.Generic;

namespace Wheeled.Gameplay.Movement
{

    internal sealed class MovementHistory
    {

        private sealed class History<T>
        {

            public struct Node
            {
                public int step;
                public T value;
            }

            private readonly LinkedList<Node> m_nodes = new LinkedList<Node>();

            public void Put(int _step, T _value)
            {
                LinkedListNode<Node> listNode = GetNodeOrPrevious(_step);
                if (listNode == null)
                {
                    m_nodes.AddFirst(new Node { step = _step, value = _value });
                }
                else if (listNode.Value.step == _step)
                {
                    Node node = listNode.Value;
                    node.value = _value;
                    listNode.Value = node;
                }
                else
                {
                    m_nodes.AddAfter(listNode, new Node { step = _step, value = _value });
                }
            }

            public void TrimOlder(int _oldest, bool _keepOldest)
            {
                LinkedListNode<Node> node = m_nodes.First;
                if (_keepOldest)
                {
                    while (node != null && node.Next?.Value.step < _oldest)
                    {
                        m_nodes.RemoveFirst();
                        node = node.Next;
                    }
                }
                else
                {
                    while (node != null && node.Value.step < _oldest)
                    {
                        m_nodes.RemoveFirst();
                        node = node.Next;
                    }
                }
            }

            public void TrimNewer(int _newest, bool _keepNewest)
            {
                LinkedListNode<Node> node = m_nodes.Last;
                if (_keepNewest)
                {
                    while (node != null && node.Next?.Value.step > _newest)
                    {
                        m_nodes.RemoveLast();
                        node = node.Previous;
                    }
                }
                else
                {
                    while (node != null && node.Value.step > _newest)
                    {
                        m_nodes.RemoveLast();
                        node = node.Previous;
                    }
                }
            }

            public LinkedListNode<Node> First => m_nodes.First;

            public LinkedListNode<Node> GetNode(int _step)
            {
                LinkedListNode<Node> node = GetNodeOrPrevious(_step);
                if (node?.Value.step != _step)
                {
                    return null;
                }
                else
                {
                    return node;
                }
            }

            public LinkedListNode<Node> GetNodeOrPrevious(int _step)
            {
                LinkedListNode<Node> node = m_nodes.Last;
                while (node != null && node.Value.step > _step)
                {
                    node = node.Previous;
                }
                return node;
            }

            public void Query(int _step, out Node? _outStepOrPrevious, out Node? _outNext)
            {
                LinkedListNode<Node> node = m_nodes.Last;
                LinkedListNode<Node> lastNode = null;
                while (node != null && node.Value.step > _step)
                {
                    lastNode = node;
                    node = node.Previous;
                }
                _outStepOrPrevious = node?.Value;
                _outNext = lastNode?.Value;
            }

        }

        private readonly History<Sight> m_sightHistory;
        private readonly History<SimulationStep> m_simulationHistory;
        private readonly History<InputStep> m_inputHistory;

        public MovementHistory()
        {
            m_sightHistory = new History<Sight>();
            m_simulationHistory = new History<SimulationStep>();
            m_inputHistory = new History<InputStep>();
        }

        public void Put(int _step, in Sight _sight)
        {
            m_sightHistory.Put(_step, _sight);
        }

        public void Put(int _step, in SimulationStep _simulation)
        {
            m_simulationHistory.Put(_step, _simulation);
        }

        public void Put(int _step, in InputStep _inputStep)
        {
            m_inputHistory.Put(_step, _inputStep);
        }


        public void PullReverseInputBuffer(int _step, InputStep[] _dstBuffer, out int _outCount)
        {
            LinkedListNode<History<InputStep>.Node> node = m_inputHistory.GetNode(_step);
            _outCount = 0;
            while (node != null && _outCount < _dstBuffer.Length)
            {
                _dstBuffer[_outCount] = node.Value.value;
                if (node.Previous?.Value.step != node.Value.step - 1)
                {
                    node = null;
                }
                else
                {
                    node = node.Previous;
                }
            }
        }

        private void PartialSimulate(ref SimulationStep _refSimulationStep, ref int _refStep, ref double _refDeltaTime, bool _canPredict)
        {
            LinkedListNode<History<InputStep>.Node> inputNode = m_inputHistory.GetNodeOrPrevious(_refStep) ?? m_inputHistory.First;
            InputStep input = new InputStep();
            while (_refDeltaTime > 0.0)
            {
                if (inputNode?.Value.step < _refStep && _canPredict)
                {
                    input = inputNode.Value.value.Predicted;
                }
                else if (inputNode?.Value.step == _refStep)
                {
                    input = inputNode.Value.value;
                    inputNode = inputNode.Next;
                }
                else if (_canPredict)
                {
                    input = input.Predicted;
                }
                else
                {
                    return;
                }
                if (_refDeltaTime > TimeConstants.c_simulationStep)
                {
                    _refSimulationStep = _refSimulationStep.Simulate(input, TimeConstants.c_simulationStep);
                    _refDeltaTime -= TimeConstants.c_simulationStep;
                }
                else
                {
                    _refSimulationStep = _refSimulationStep.Simulate(input, _refDeltaTime);
                    _refDeltaTime = 0.0;
                }
            }
        }

        public void GetSimulation(double _time, out SimulationStep? _outSimulation, bool _isPartialSimulationEnabled)
        {
            m_simulationHistory.Query(_time.SimulationSteps(), out History<SimulationStep>.Node? prev, out History<SimulationStep>.Node? next);
            if (prev != null)
            {
                if (next != null)
                {
                    // Prev & next
                    SimulationStep a = prev.Value.value;
                    SimulationStep b = next.Value.value;
                    double period = (next.Value.step - prev.Value.step).SimulationPeriod();
                    double progress = _time - (prev.Value.step).SimulationPeriod();
                    if (_isPartialSimulationEnabled)
                    {
                        SimulationStep simulation = prev.Value.value;
                        int step = prev.Value.step;
                        double partialSimulationProgress = progress;
                        PartialSimulate(ref simulation, ref step, ref partialSimulationProgress, false);
                        float lerpAlpha = (float) (partialSimulationProgress / (period - progress + partialSimulationProgress));
                        _outSimulation = SimulationStep.Lerp(simulation, next.Value.value, lerpAlpha);
                    }
                    else
                    {
                        _outSimulation = SimulationStep.Lerp(a, b, (float) (progress / period));
                    }
                }
                else
                {
                    // Prev only
                    History<SimulationStep>.Node node = prev.Value;
                    if (_isPartialSimulationEnabled)
                    {
                        const double c_maxPrevision = 1.0f;
                        SimulationStep simulation = node.value;
                        int step = node.step;
                        double partialSimulationProgress = Math.Min(_time - step.SimulationPeriod(), c_maxPrevision);
                        PartialSimulate(ref simulation, ref step, ref partialSimulationProgress, true);
                        _outSimulation = simulation;
                    }
                    else
                    {
                        _outSimulation = node.value;
                    }
                }
            }
            else
            {
                // No prev
                _outSimulation = null;
            }
        }

        public void GetSight(double _time, out Sight? _outSight)
        {
            m_sightHistory.Query(_time.SimulationSteps(), out History<Sight>.Node? prev, out History<Sight>.Node? next);
            if (prev != null)
            {
                if (next != null)
                {
                    // Prev & next
                    double period = (next.Value.step - prev.Value.step).SimulationPeriod();
                    double elapsed = _time - prev.Value.step.SimulationPeriod();
                    _outSight = Sight.Lerp(prev.Value.value, next.Value.value, (float) (elapsed / period));
                }
                else
                {
                    // Prev only
                    _outSight = prev.Value.value;
                }
            }
            else
            {
                // No prev
                _outSight = null;
            }
        }

        public void TrimOlder(int _oldest, bool _keepOldest)
        {
            m_sightHistory.TrimOlder(_oldest, _keepOldest);
            m_simulationHistory.TrimOlder(_oldest, _keepOldest);
            m_inputHistory.TrimOlder(_oldest, _keepOldest);
        }

        public void TrimNewer(int _newest, bool _keepNewest)
        {
            m_sightHistory.TrimNewer(_newest, _keepNewest);
            m_simulationHistory.TrimNewer(_newest, _keepNewest);
            m_inputHistory.TrimNewer(_newest, _keepNewest);
        }

    }

}
