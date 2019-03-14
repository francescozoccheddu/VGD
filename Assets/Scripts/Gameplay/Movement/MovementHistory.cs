#define ENABLE_PARTIAL_SIMULATION

using System.Collections.Generic;

namespace Wheeled.Gameplay.Movement
{

#if ENABLE_PARTIAL_SIMULATION
    using SimulationType = SimulationStepInfo;
#else
    using SimulationType = SimulationStep;
#endif

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

            private LinkedListNode<Node> GetNodeOrPrevious(int _step)
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

        private readonly History<Sight> m_sightHistory = new History<Sight>();
        private readonly History<SimulationType> m_simulationHistory = new History<SimulationType>();

        public void Put(int _step, in SimulationType _simulation)
        {
            m_simulationHistory.Put(_step, _simulation);
        }

        public void Put(int _step, in Sight _sight)
        {
            m_sightHistory.Put(_step, _sight);
        }

        private SimulationStep PartialSimulate(in InputStep _input, in SimulationStep _simulationStep, TimeStep _deltaTime)
        {
            SimulationStep simulation = _simulationStep;
            InputStep input = _input;
            while (_deltaTime.Step > 1)
            {
                simulation = simulation.Simulate(input, TimeStep.c_simulationStep);
                input = input.Predicted;
                _deltaTime.Step--;
            }
            simulation = simulation.Simulate(_input, _deltaTime.Remainder);
            return simulation;
        }

        public void GetSimulation(TimeStep _time, out SimulationStep? _outSimulation)
        {
            m_simulationHistory.Query(_time.Step, out History<SimulationType>.Node? prev, out History<SimulationType>.Node? next);
            if (prev != null)
            {
                if (next != null)
                {
                    // Prev & next
                    if (next.Value.step - prev.Value.step > 1)
                    {
                        // Consecutive prev & next
#if ENABLE_PARTIAL_SIMULATION
                        _outSimulation = PartialSimulate(prev.Value.value.input, prev.Value.value.simulation, new TimeStep(0, _time.Remainder));
#else
                            _outSimulation = SimulationStep.Lerp(prev.Value.value, next.Value.value, _time.Remainder);
#endif
                    }
                    else
                    {
                        // History holes
                        SimulationStep a, b;
#if ENABLE_PARTIAL_SIMULATION
                        a = prev.Value.value.simulation;
                        b = next.Value.value.simulation;
#else
                            a = prev.Value.value;
                            b = next.Value.value;
#endif
                        float period = (next.Value.step - prev.Value.step) * TimeStep.c_simulationStep;
                        float elapsed = (_time - new TimeStep(prev.Value.step, 0.0f)).Seconds;
                        _outSimulation = SimulationStep.Lerp(a, b, elapsed / period);
                    }
                }
                else
                {
                    // Prev only
                    History<SimulationType>.Node node = prev.Value;
#if ENABLE_PARTIAL_SIMULATION
                    _outSimulation = PartialSimulate(node.value.input, node.value.simulation, _time - new TimeStep(node.step, 0.0f));
#else
                        _outSimulation = node.value;
#endif
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
        }

        public void TrimNewer(int _newest, bool _keepNewest)
        {
            m_sightHistory.TrimOlder(_newest, _keepNewest);
            m_simulationHistory.TrimOlder(_newest, _keepNewest);
        }

    }

}
