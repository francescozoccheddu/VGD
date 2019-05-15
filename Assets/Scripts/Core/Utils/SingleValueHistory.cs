namespace Wheeled.Core.Utils
{

    public sealed class SingleValueHistory<T>
    {

        private double? m_time;

        public SingleValueHistory(T _initialValue = default)
        {
            m_time = null;
            Value = _initialValue;
        }

        public T Value { get; private set; }

        public void Put(double _time, T _value)
        {
            if (m_time > _time != true)
            {
                m_time = _time;
                Value = _value;
            }
        }

    }

}
