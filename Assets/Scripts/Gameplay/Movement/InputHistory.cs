using System.Collections.Generic;

using Wheeled.Core.Utils;

namespace Wheeled.Gameplay.Movement
{
    public interface IReadOnlyInputHistory
    {
        IReadOnlyHistory<int, InputStep> History { get; }

        IEnumerable<InputStep> GetReversedInputSequence(int _step, int _maxLength);

        CharacterController SimulateFrom(int _step, CharacterController _simulation);
    }

    public sealed class InputHistory : IReadOnlyInputHistory
    {
        public IReadOnlyHistory<int, InputStep> History => m_history;

        private readonly LinkedListHistory<int, InputStep> m_history;

        public InputHistory()
        {
            m_history = new LinkedListHistory<int, InputStep>();
        }

        public IEnumerable<InputStep> GetReversedInputSequence(int _step, int _maxLength)
        {
            int count = 0;
            foreach (HistoryNode<int, InputStep> node in m_history.UntilBackwards(_step, false, false))
            {
                if (node.time != _step - count || count >= _maxLength)
                {
                    break;
                }
                yield return node.value;
                count++;
            }
        }

        public void Put(int _step, InputStep _inputStep)
        {
            m_history.Set(_step, _inputStep);
        }

        public CharacterController SimulateFrom(int _step, CharacterController _simulation)
        {
            InputStep? input = null;
            foreach (HistoryNode<int, InputStep> node in m_history.Since(_step))
            {
                input = input?.Predicted ?? node.value;
                while (_step <= node.time)
                {
                    if (_step == node.time)
                    {
                        input = node.value;
                    }
                    _simulation = _simulation.Simulate(input.Value, (float) TimeConstants.c_simulationStep);
                    _step++;
                }
            }
            return _simulation;
        }

        public void Trim(int _oldest)
        {
            m_history.ForgetOlder(_oldest, true);
        }
    }
}