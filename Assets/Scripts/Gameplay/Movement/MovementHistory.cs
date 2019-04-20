using System;
using System.Collections.Generic;
using UnityEngine;
using Wheeled.Core.Utils;

namespace Wheeled.Gameplay.Movement
{
    internal interface IReadOnlyMovementHistory
    {
        #region Public Methods

        void GetSight(double _time, out Sight? _outSight);

        void GetSimulation(double _time, out CharacterController? _outSimulation, IReadOnlyInputHistory _inputHistory);

        #endregion Public Methods
    }

    internal static class MovementHistoryHelper
    {
        #region Public Methods

        public static Snapshot GetSnapshot(this IReadOnlyMovementHistory _movementHistory, double _time, IReadOnlyInputHistory _inputHistory)
        {
            Snapshot snapshot = new Snapshot();
            _movementHistory.GetSimulation(_time, out CharacterController? simulation, _inputHistory);
            if (simulation != null)
            {
                snapshot.simulation = simulation.Value;
            }
            _movementHistory.GetSight(_time, out Sight? sight);
            if (sight != null)
            {
                snapshot.sight = sight.Value;
            }
            return snapshot;
        }

        #endregion Public Methods
    }

    internal sealed class MovementHistory : IReadOnlyMovementHistory
    {
        #region Public Properties

        public double MaxPrevisionTime { get => m_maxPrevisionTime; set { Debug.Assert(value >= 0.0); m_maxPrevisionTime = value; } }

        #endregion Public Properties

        #region Private Fields

        private readonly LinkedListHistory<int, Sight> m_sightHistory;
        private readonly LinkedListHistory<int, CharacterController> m_simulationHistory;
        private double m_maxPrevisionTime;

        #endregion Private Fields

        #region Public Constructors

        public MovementHistory()
        {
            m_sightHistory = new LinkedListHistory<int, Sight>();
            m_simulationHistory = new LinkedListHistory<int, CharacterController>();
        }

        #endregion Public Constructors

        #region Public Methods

        public void Trim(int _oldest)
        {
            m_sightHistory.ForgetOlder(_oldest, true);
            m_simulationHistory.ForgetOlder(_oldest, true);
        }

        public void GetSight(double _time, out Sight? _outSight)
        {
            m_sightHistory.Around(_time.SimulationSteps(), out HistoryNode<int, Sight>? prev, out HistoryNode<int, Sight>? next);
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

        public void GetSimulation(double _time, out CharacterController? _outSimulation, IReadOnlyInputHistory _inputHistory)
        {
            m_simulationHistory.Around(_time.SimulationSteps(), out HistoryNode<int, CharacterController>? prev, out HistoryNode<int, CharacterController>? next);
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

        #endregion Public Methods

        #region Private Methods

        private void PartialSimulate(IReadOnlyInputHistory _inputHistory, ref CharacterController _refSimulationStep, int _step, ref double _refDeltaTime, bool _canPredict)
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
                using (IEnumerator<HistoryNode<int, InputStep>> enumerator = _inputHistory.History.Since(_step, true, false).GetEnumerator())
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
                foreach (HistoryNode<int, InputStep> node in _inputHistory.History.Since(_step, false, false))
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

        #endregion Private Methods
    }
}