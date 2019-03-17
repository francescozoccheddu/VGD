using System;
using Wheeled.Core.Utils;

namespace Wheeled.Gameplay.Movement
{

    internal sealed class MovementHistory
    {

        private readonly IHistory<int, Sight> m_sightHistory;
        private readonly IHistory<int, SimulationStep> m_simulationHistory;
        private readonly IHistory<int, InputStep> m_inputHistory;

        public MovementHistory()
        {
            m_sightHistory = new LinkedListHistory<int, Sight>();
            m_simulationHistory = new LinkedListHistory<int, SimulationStep>();
            m_inputHistory = new LinkedListHistory<int, InputStep>();
        }

        public void Put(int _step, in Sight _sight)
        {
            m_sightHistory.Set(_step, _sight);
        }

        public void Put(int _step, in SimulationStep _simulation)
        {
            m_simulationHistory.Set(_step, _simulation);
        }

        public void Put(int _step, in InputStep _inputStep)
        {
            m_inputHistory.Set(_step, _inputStep);
        }


        public void PullReverseInputBuffer(int _step, InputStep[] _dstBuffer, out int _outCount)
        {
            _outCount = 0;
            foreach (HistoryNode<int, InputStep> node in m_inputHistory.GetReversedSequence(_step))
            {
                if (node.time != _step - _outCount)
                {
                    break;
                }
                _dstBuffer[_outCount++] = node.entry;
            }
        }

        private void PartialSimulate(ref SimulationStep _refSimulationStep, ref int _refStep, ref double _refDeltaTime, bool _canPredict)
        {
            InputStep input = new InputStep();
            foreach (HistoryNode<int, InputStep> node in m_inputHistory.GetSequence(_refStep))
            {
                if (_refDeltaTime <= 0.0)
                {
                    break;
                }
                if (node.time < _refStep && _canPredict)
                {
                    input = node.entry.Predicted;
                }
                else if (node.time == _refStep)
                {
                    input = node.entry;
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

        public void GetSimulation(double _time, out SimulationStep? _outSimulation, bool _isPartialSimulationEnabled)
        {
            m_simulationHistory.Query(_time.SimulationSteps(), out HistoryNode<int, SimulationStep>? prev, out HistoryNode<int, SimulationStep>? next);
            if (prev != null)
            {
                if (next != null)
                {
                    // Prev & next
                    SimulationStep a = prev.Value.entry;
                    SimulationStep b = next.Value.entry;
                    double period = (next.Value.time - prev.Value.time).SimulationPeriod();
                    double progress = _time - (prev.Value.time).SimulationPeriod();
                    if (_isPartialSimulationEnabled)
                    {
                        SimulationStep simulation = prev.Value.entry;
                        int step = prev.Value.time;
                        double partialSimulationProgress = progress;
                        PartialSimulate(ref simulation, ref step, ref partialSimulationProgress, false);
                        float lerpAlpha = (float) (partialSimulationProgress / (period - progress + partialSimulationProgress));
                        _outSimulation = SimulationStep.Lerp(simulation, next.Value.entry, lerpAlpha);
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
                    if (_isPartialSimulationEnabled)
                    {
                        const double c_maxPrevision = 1.0f;
                        SimulationStep simulation = node.entry;
                        int step = node.time;
                        double partialSimulationProgress = Math.Min(_time - step.SimulationPeriod(), c_maxPrevision);
                        PartialSimulate(ref simulation, ref step, ref partialSimulationProgress, true);
                        _outSimulation = simulation;
                    }
                    else
                    {
                        _outSimulation = node.entry;
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
            m_sightHistory.Query(_time.SimulationSteps(), out HistoryNode<int, Sight>? prev, out HistoryNode<int, Sight>? next);
            if (prev != null)
            {
                if (next != null)
                {
                    // Prev & next
                    double period = (next.Value.time - prev.Value.time).SimulationPeriod();
                    double elapsed = _time - prev.Value.time.SimulationPeriod();
                    _outSight = Sight.Lerp(prev.Value.entry, next.Value.entry, (float) (elapsed / period));
                }
                else
                {
                    // Prev only
                    _outSight = prev.Value.entry;
                }
            }
            else
            {
                // No prev
                _outSight = null;
            }
        }

        public void ForgetOlder(int _oldest, bool _keepOldest)
        {
            m_sightHistory.ForgetOlder(_oldest, _keepOldest);
            m_simulationHistory.ForgetOlder(_oldest, _keepOldest);
            m_inputHistory.ForgetOlder(_oldest, _keepOldest);
        }

        public void ForgetNewer(int _newest, bool _keepNewest)
        {
            m_sightHistory.ForgetNewer(_newest, _keepNewest);
            m_simulationHistory.ForgetNewer(_newest, _keepNewest);
            m_inputHistory.ForgetNewer(_newest, _keepNewest);
        }

    }

}
