#define ENABLE_PARTIAL_SIMULATION

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

        public readonly bool isPartialSimulationEnabled;

        public MovementHistory(bool _isPartialSimulationEnabled)
        {
            isPartialSimulationEnabled = _isPartialSimulationEnabled;
            m_sightHistory = new History<Sight>();
            m_simulationHistory = new History<SimulationStep>();
            m_inputHistory = isPartialSimulationEnabled ? new History<InputStep>() : null;
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
            if (isPartialSimulationEnabled)
            {
                m_inputHistory.Put(_step, _inputStep);
            }
        }

        private void PartialSimulate(ref SimulationStep _refSimulationStep, ref int _refStep, ref TimeStep _refDeltaTime, bool _canPredict)
        {
            LinkedListNode<History<InputStep>.Node> inputNode = m_inputHistory.GetNodeOrPrevious(_refStep) ?? m_inputHistory.First;
            InputStep input = new InputStep();
            while (_refDeltaTime > TimeStep.zero)
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
                if (_refDeltaTime.Step > 0)
                {
                    _refSimulationStep = _refSimulationStep.Simulate(input, TimeStep.c_simulationStep);
                    _refDeltaTime.Step--;
                }
                else if (_refDeltaTime.HasRemainder)
                {
                    _refSimulationStep = _refSimulationStep.Simulate(input, _refDeltaTime.Remainder);
                    _refDeltaTime = TimeStep.zero;
                }
            }
        }

        public void GetSimulation(TimeStep _time, out SimulationStep? _outSimulation)
        {
            m_simulationHistory.Query(_time.Step, out History<SimulationStep>.Node? prev, out History<SimulationStep>.Node? next);
            if (prev != null)
            {
                if (next != null)
                {
                    // Prev & next
                    if (next.Value.step - prev.Value.step > 1)
                    {
                        // Consecutive prev & next
                        if (isPartialSimulationEnabled)
                        {
                            SimulationStep simulation = prev.Value.value;
                            int step = prev.Value.step;
                            TimeStep delta = _time.RemainderTime;
                            PartialSimulate(ref simulation, ref step, ref delta, false);
                            float lerpAlpha = delta.Remainder / (TimeStep.c_simulationStep - _time.Remainder + delta.Remainder);
                            _outSimulation = SimulationStep.Lerp(simulation, next.Value.value, lerpAlpha);
                        }
                        else
                        {
                            _outSimulation = SimulationStep.Lerp(prev.Value.value, next.Value.value, _time.Remainder / TimeStep.c_simulationStep);
                        }
                    }
                    else
                    {
                        // History holes
                        SimulationStep a = prev.Value.value;
                        SimulationStep b = next.Value.value;
                        float period = (next.Value.step - prev.Value.step) * TimeStep.c_simulationStep;
                        if (isPartialSimulationEnabled)
                        {
                            SimulationStep simulation = prev.Value.value;
                            int step = prev.Value.step;
                            TimeStep targetDelta = _time - TimeStep.FromSteps(step);
                            TimeStep delta = targetDelta;
                            PartialSimulate(ref simulation, ref step, ref delta, false);
                            float lerpAlpha = delta.Remainder / (period - targetDelta.Seconds + delta.Seconds);
                            _outSimulation = SimulationStep.Lerp(simulation, next.Value.value, lerpAlpha);
                        }
                        else
                        {
                            float elapsed = (_time - new TimeStep(prev.Value.step, 0.0f)).Seconds;
                            _outSimulation = SimulationStep.Lerp(a, b, elapsed / period);
                        }
                    }
                }
                else
                {
                    // Prev only
                    History<SimulationStep>.Node node = prev.Value;
                    if (isPartialSimulationEnabled)
                    {
                        SimulationStep simulation = node.value;
                        int step = node.step;
                        TimeStep delta = TimeStep.Min(_time - TimeStep.FromSteps(step), TimeStep.FromSeconds(1.0f));
                        PartialSimulate(ref simulation, ref step, ref delta, true);
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

        public void GetSight(TimeStep _time, out Sight? _outSight)
        {
            m_sightHistory.Query(_time.Step, out History<Sight>.Node? prev, out History<Sight>.Node? next);
            if (prev != null)
            {
                if (next != null)
                {
                    // Prev & next
                    float progress = (next.Value.step - prev.Value.step) * TimeStep.c_simulationStep;
                    _outSight = Sight.Lerp(prev.Value.value, next.Value.value, progress);
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
            if (isPartialSimulationEnabled)
            {
                m_inputHistory.TrimOlder(_oldest, _keepOldest);
            }
        }

        public void TrimNewer(int _newest, bool _keepNewest)
        {
            m_sightHistory.TrimNewer(_newest, _keepNewest);
            m_simulationHistory.TrimNewer(_newest, _keepNewest);
            if (isPartialSimulationEnabled)
            {
                m_inputHistory.TrimNewer(_newest, _keepNewest);
            }
        }

    }

}
