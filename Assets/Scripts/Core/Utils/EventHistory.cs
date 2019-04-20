namespace Wheeled.Core.Utils
{
    internal sealed class EventHistory<T>
    {
        #region Public Interfaces

        public interface ITarget
        {
            #region Public Methods

            void Perform(double _time, T _value);

            #endregion Public Methods
        }

        #endregion Public Interfaces

        #region Public Properties

        public ITarget Target { get; set; }

        #endregion Public Properties

        #region Private Fields

        private readonly LinkedListHistory<double, T> m_history;

        #endregion Private Fields

        #region Public Constructors

        public EventHistory()
        {
            m_history = new LinkedListHistory<double, T>();
        }

        #endregion Public Constructors

        #region Public Methods

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

        #endregion Public Methods
    }
}