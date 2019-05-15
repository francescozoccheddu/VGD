namespace Wheeled.Core.Utils
{
    public sealed class EventHistory<T>
    {
        public interface ITarget
        {
            void Perform(double _time, T _value);
        }

        public ITarget Target { get; set; }

        private readonly LinkedListHistory<double, T> m_history;

        public EventHistory()
        {
            m_history = new LinkedListHistory<double, T>();
        }

        public void Put(double _time, T _value)
        {
            m_history.Add(_time, _value);
        }

        public void PerformUntil(double _time)
        {
            foreach (HistoryNode<double, T> node in m_history.Until(_time))
            {
                Target?.Perform(_time, node.value);
            }
            m_history.ForgetAndOlder(_time);
        }
    }
}