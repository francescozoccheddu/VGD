using Wheeled.Core.Utils;

namespace Wheeled.Gameplay.Movement
{
    internal sealed class InputHistory
    {

        private readonly LinkedListHistory<int, InputStep> m_history;

        public InputHistory()
        {
            m_history = new LinkedListHistory<int, InputStep>();
        }

        public void Put(int _step, InputStep _inputStep)
        {
            m_history.Set(_step, _inputStep);
        }

        public SimulationStep SimulateFrom(int _step, SimulationStep _simulation)
        {
            InputStep? input = null;
            foreach (HistoryNode<int, InputStep> node in m_history.GetSequence(_step))
            {
                input = input?.Predicted ?? node.entry;
                while (_step <= node.time)
                {
                    if (_step == node.time)
                    {
                        input = node.entry;
                    }
                    _simulation = _simulation.Simulate(input.Value, TimeConstants.c_simulationStep);
                    _step++;
                }
            }
            return _simulation;
        }

        public void PullReverseInputBuffer(int _step, InputStep[] _dstBuffer, out int _outCount)
        {
            _outCount = 0;
            foreach (HistoryNode<int, InputStep> node in m_history.GetReversedSequence(_step))
            {
                if (node.time != _step - _outCount)
                {
                    break;
                }
                _dstBuffer[_outCount++] = node.entry;
            }
        }

        public void Trim(int _oldest)
        {
            m_history.ForgetOlder(_oldest, true);
        }

        public void Cut(int _oldest)
        {
            m_history.ForgetAndOlder(_oldest);
        }

    }
}
