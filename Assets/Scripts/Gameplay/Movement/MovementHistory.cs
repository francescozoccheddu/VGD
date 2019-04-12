using System;
using System.Collections.Generic;
using UnityEngine;
using Wheeled.Core.Utils;

namespace Wheeled.Gameplay.Movement
{
    internal sealed class MovementHistory
    {
        private readonly IHistory<int, Sight> m_sightHistory;
        private readonly IHistory<int, CharacterController> m_simulationHistory;
        private double m_maxPrevisionTime;

        public MovementHistory()
        {
            m_sightHistory = new LinkedListHistory<int, Sight>();
            m_simulationHistory = new LinkedListHistory<int, CharacterController>();
        }

        public double MaxPrevisionTime { get => m_maxPrevisionTime; set { Debug.Assert(value >= 0.0); m_maxPrevisionTime = value; } }

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

        public void GetSimulation(double _time, out CharacterController? _outSimulation, InputHistory _inputHistory)
        {
            m_simulationHistory.Query(_time.SimulationSteps(), out HistoryNode<int, CharacterController>? prev, out HistoryNode<int, CharacterController>? next);
            if (prev != null)
            {
                if (next != null)
                {
                    // Prev & next
                    CharacterController a = prev.Value.value;
                    CharacterController b = next.Value.value;
                    double period = (next.Value.time - prev.Value.time).SimulationPeriod();
                    double progress = _time - (prev.Value.time).SimulationPeriod();
                    CharacterController simulation = prev.Value.value;
                    int step = prev.Value.time;
                    double partialSimulationProgress = progress;
                    PartialSimulate(_inputHistory, ref simulation, step, ref partialSimulationProgress, false);
                    float lerpAlpha = (float) (partialSimulationProgress / (period - progress + partialSimulationProgress));
                    _outSimulation = CharacterController.Lerp(simulation, next.Value.value, lerpAlpha);
                }
                else
                {
                    // Prev only
                    HistoryNode<int, CharacterController> node = prev.Value;
                    CharacterController simulation = node.value;
                    int step = node.time;
                    double partialSimulationProgress = Math.Min(_time - step.SimulationPeriod(), MaxPrevisionTime);

                    PartialSimulate(_inputHistory, ref simulation, step, ref partialSimulationProgress, true);
                    _outSimulation = simulation;
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

        public void Put(int _step, in CharacterController _simulation)
        {
            m_simulationHistory.Set(_step, _simulation);
        }

        private void PartialSimulate(InputHistory _inputHistory, ref CharacterController _refSimulationStep, int _step, ref double _refDeltaTime, bool _canPredict)
        {
            void Simulate(ref CharacterController _refInnerSimulationStep, ref int _refInnerStep, ref double _refInnerDeltaTime, in InputStep _inputStep)
            {
                if (_refInnerDeltaTime >= TimeConstants.c_simulationStep)
                {
                    _refInnerSimulationStep = _refInnerSimulationStep.Simulate(_inputStep, (float) TimeConstants.c_simulationStep);
                    _refInnerDeltaTime -= TimeConstants.c_simulationStep;
                    _refInnerStep++;
                }
                else
                {
                    _refInnerSimulationStep = _refInnerSimulationStep.Simulate(_inputStep, (float) _refInnerDeltaTime);
                    _refInnerDeltaTime = 0.0;
                }
            }
            if (_canPredict)
            {
                using (IEnumerator<HistoryNode<int, InputStep>> enumerator = _inputHistory.GetSequenceSince(_step, true, false).GetEnumerator())
                {
                    if (enumerator.MoveNext())
                    {
                        HistoryNode<int, InputStep> node = enumerator.Current;
                        while (_refDeltaTime > 0.0)
                        {
                            if (node.time < _step && enumerator.MoveNext())
                            {
                                node = enumerator.Current;
                            }
                            Simulate(ref _refSimulationStep, ref _step, ref _refDeltaTime, node.value);
                            node.value = node.value.Predicted;
                        }
                    }
                }
            }
            else
            {
                foreach (HistoryNode<int, InputStep> node in _inputHistory.GetSequenceSince(_step, false, false))
                {
                    if (node.time == _step && _refDeltaTime > 0.0)
                    {
                        Simulate(ref _refSimulationStep, ref _step, ref _refDeltaTime, node.value);
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
    }
}