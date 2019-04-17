using Wheeled.Core.Utils;

namespace Wheeled.Gameplay.Action
{

    internal sealed class ActionHistory
    {

        public interface ITarget
        {
            void Perform(double _time, object _value);
        }

        public ITarget Target { get; set; }

        private readonly LinkedListHistory<double, object> m_history;

        public ActionHistory()
        {
            m_history = new LinkedListHistory<double, object>();
        }

        public void Put(double _time, object _value)
        {
            m_history.Add(_time, _value);
        }

        public void PerformUntil(double _time)
        {
        }

    }

}