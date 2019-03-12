#define ENABLE_PARTIAL_SIMULATION


using UnityEngine;

namespace Wheeled.Gameplay.Movement
{

    internal sealed partial class MovementController
    {

        private sealed class History
        {

            private readonly SimulationStepInfo?[] m_history;
            private int m_Lenght => m_history.Length;
            public int Oldest { get; private set; }
            public int Newest { get; private set; }

            public History(float _duration)
            {
                m_history = new SimulationStepInfo?[TimeStep.GetStepsInPeriod(_duration)];
            }

            private int GetIndex(int _step)
            {
                return _step % m_history.Length;
            }

            public void Append(int _step, SimulationStepInfo _simulation)
            {
                if (_step >= Oldest)
                {
                    int step = Mathf.Max(Oldest, Newest, _step - m_Lenght + 1);
                    while (step < _step)
                    {
                        m_history[GetIndex(step)] = null;
                        step++;
                    }
                    Newest = _step;
                    m_history[GetIndex(Newest)] = _simulation;
                    Oldest = Mathf.Max(Oldest, Newest - m_Lenght + 1);
                }
            }

            public SimulationStep? Correct(int _step, SimulationStepInfo _simulation)
            {
                if (_step >= Oldest && _step <= Newest)
                {
                    m_history[GetIndex(_step)] = _simulation;
                    SimulationStepInfo simulation = _simulation;
                    for (int i = _step + 1; i <= Newest; i++)
                    {
                        int step = GetIndex(i);
                        if (m_history[step] != null)
                        {
                            simulation = m_history[step].Value;
                            simulation.simulation = simulation.simulation.Simulate(simulation.input, TimeStep.c_simulationStep);
                            m_history[step] = simulation;
                        }
                        else
                        {
                            simulation.input = simulation.input.Predicted;
                            simulation.simulation = simulation.simulation.Simulate(simulation.input, TimeStep.c_simulationStep);
                        }
                    }
                    return simulation.simulation;
                }
                else
                {
                    return null;
                }
            }

            public void Cut(int _step)
            {
                Oldest = _step;
                Newest = _step - 1;
            }

        }

    }

}
