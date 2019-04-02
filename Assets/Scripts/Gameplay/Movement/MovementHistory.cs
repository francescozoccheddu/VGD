using System;

using Wheeled.Core.Utils;

namespace Wheeled.Gameplay.Movement
{
    internal sealed class MovementHistory
    {
        private readonly IHistory<int, Sight> m_sightHistory;
        private readonly IHistory<int, SimulationStep> m_simulationHistory;

        public MovementHistory()
        {
            m_sightHistory = new LinkedListHistory<int, Sight>();
            m_simulationHistory = new LinkedListHistory<int, SimulationStep>();
        }

        public void ForgetNewer(int _newest, bool _keepNewest)
        {
            m_sightHistory.ForgetNewer(_newest, _keepNewest);
            m_simulationHistory.ForgetNewer(_newest, _keepNewest);
        }

        public void ForgetOlder(int _oldest, bool _keepOldest)
        {
            m_sightHistory.ForgetOlder(_oldest, _keepOldest);
            m_simulationHistory.ForgetOlder(_oldest, _keepOldest);
        }

        public void GetSight(double _time, out Sight? _outSight)
        {
            m_sightHistory.Query(_time.SimulationSteps(), out HistoryNode<int, Sight>? prev, out HistoryNode<int, Sight>? next);
            if (prev != null)
            {
                if (next != null)
                {
                    // Prev & next
                    double period = (next.Value.time - prev.Value.time).SimulationPeriod();
                    double elapsed = _time - prev.Value.time.SimulationPeriod();
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

        public void GetSimulation(double _time, out SimulationStep? _outSimulation, InputHistory _inputHistory)
        {
            m_simulationHistory.Query(_time.SimulationSteps(), out HistoryNode<int, SimulationStep>? prev, out HistoryNode<int, SimulationStep>? next);
            if (prev != null)
            {
                if (next != null)
                {
                    // Prev & next
                    SimulationStep a = prev.Value.value;
                    SimulationStep b = next.Value.value;
                    double period = (next.Value.time - prev.Value.time).SimulationPeriod();
                    double progress = _time - (prev.Value.time).SimulationPeriod();
                    if (_inputHistory != null)
                    {
                        SimulationStep simulation = prev.Value.value;
                        int step = prev.Value.time;
                        double partialSimulationProgress = progress;
                        PartialSimulate(_inputHistory, ref simulation, ref step, ref partialSimulationProgress, false);
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
                    HistoryNode<int, SimulationStep> node = prev.Value;
                    if (_inputHistory != null)
                    {
                        const double c_maxPrevision = 1.0f;
                        SimulationStep simulation = node.value;
                        int step = node.time;
                        double partialSimulationProgress = Math.Min(_time - step.SimulationPeriod(), c_maxPrevision);
                        PartialSimulate(_inputHistory, ref simulation, ref step, ref partialSimulationProgress, true);
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

        public void Put(int _step, in Sight _sight)
        {
            m_sightHistory.Set(_step, _sight);
        }

        public void Put(int _step, in SimulationStep _simulation)
        {
            m_simulationHistory.Set(_step, _simulation);
        }

        private void PartialSimulate(InputHistory _inputHistory, ref SimulationStep _refSimulationStep, ref int _refStep, ref double _refDeltaTime, bool _canPredict)
        {
            InputStep input = new InputStep();
            foreach (HistoryNode<int, InputStep> node in _inputHistory.GetSequenceSince(_refStep, true, false))
            {
                if (_refDeltaTime <= 0.0)
                {
                    break;
                }
                if (node.time < _refStep && _canPredict)
                {
                    input = node.value.Predicted;
                }
                else if (node.time == _refStep)
                {
                    input = node.value;
                }
                else if (_canPredict)
                {
                    input = input.Predicted;
                }
                else
                {
                    break;
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
    }
}