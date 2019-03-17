#define ENABLE_PARTIAL_SIMULATION


using UnityEngine;

namespace Wheeled.Gameplay.Movement
{

    internal sealed partial class MovementController
    {

        private sealed class History
        {

            private readonly InputStep?[] m_history;
            public int Length => m_history.Length;
            public int Oldest { get; private set; }
            public int Newest { get; private set; }

            public History(int _length)
            {
                m_history = new InputStep?[_length];
            }

            private int GetIndex(int _step)
            {
                return _step % m_history.Length;
            }

            public void Append(int _step, InputStep _input)
            {
                if (_step >= Oldest)
                {
                    int step = Mathf.Max(Oldest, Newest + 1, _step - Length + 1);
                    while (step < _step)
                    {
                        m_history[GetIndex(step)] = null;
                        step++;
                    }
                    Newest = _step;
                    m_history[GetIndex(Newest)] = _input;
                    Oldest = Mathf.Max(Oldest, Newest - Length + 1);
                }
            }

            public SimulationStep? Correct(int _step, SimulationStepInfo _simulation)
            {
                if (_step >= Oldest && _step <= Newest)
                {
                    m_history[GetIndex(_step)] = _simulation.input;
                    SimulationStepInfo simulation = _simulation;
                    for (int i = _step + 1; i <= Newest; i++)
                    {
                        int step = GetIndex(i);
                        if (m_history[step] != null)
                        {
                            simulation.input = m_history[step].Value;
                            simulation.simulation = simulation.simulation.Simulate(simulation.input, TimeConstants.c_simulationStep);
                        }
                        else
                        {
                            simulation.input = simulation.input.Predicted;
                            simulation.simulation = simulation.simulation.Simulate(simulation.input, TimeConstants.c_simulationStep);
                        }
                    }
                    return simulation.simulation;
                }
                else
                {
                    return null;
                }
            }

            public InputStep? Get(int _step)
            {
                return _step >= Oldest && _step <= Newest ? m_history[GetIndex(_step)] : null;
            }

            public void Cut(int _step)
            {
                Oldest = _step;
                Newest = _step - 1;
            }

        }

    }

}
