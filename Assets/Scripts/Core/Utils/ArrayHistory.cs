using System;
using System.Collections.Generic;
using UnityEngine;

namespace Wheeled.Core.Utils
{
    public sealed class ArrayHistory<TValue> : IHistory<int, TValue>
    {
        private struct Node
        {
            public TValue value;
            public bool isSet;
        }

        public int Duration
        {
            get => m_array.Length; set
            {
                Debug.Assert(value > 0);
                Node[] array = new Node[value];
                int min = Math.Max(m_oldest, m_Newest - value + 1);
                int max = Math.Min(m_Newest, min + value - 1);
                for (int i = min; i <= max; i++)
                {
                    array[i % value] = this[i];
                }
                m_array = array;
            }
        }

        private int m_Newest => m_oldest + Duration - 1;

        private int m_oldest;

        private Node[] m_array;

        public ArrayHistory(int _duration = 64)
        {
            m_array = new Node[_duration];
        }

        private ref Node this[int _index] => ref m_array[_index % Duration];

        public void ExtendToFit(int _time)
        {
            int duration;
            if (_time > m_Newest)
            {
                duration = _time - m_oldest + 1;
            }
            else if (_time < m_oldest)
            {
                duration = m_oldest - _time + Duration;
            }
            else
            {
                return;
            }
            Node[] array = new Node[duration];
            for (int i = m_oldest; i <= m_Newest; i++)
            {
                array[i % duration] = this[i];
            }
            m_array = array;
        }

        public void Set(int _time, TValue _value)
        {
            if (_time > m_Newest)
            {
                int oldest = _time - Duration + 1;
                for (int i = m_oldest; i < oldest; i++)
                {
                    this[i].isSet = false;
                }
                m_oldest = oldest;
            }
            else if (_time < m_oldest)
            {
                int oldest = _time;
                for (int i = oldest + Duration; i <= m_Newest; i++)
                {
                    this[i].isSet = false;
                }
                m_oldest = oldest;
            }
            m_array[_time % Duration] = new Node
            {
                isSet = true,
                value = _value
            };
        }

        void ISimpleHistory<HistoryNode<int, TValue>, int>.Set(int _time, HistoryNode<int, TValue> _value)
        {
            if (_time != _value.time)
            {
                throw new System.ArgumentException();
            }
            Set(_time, _value.value);
        }

        public IEnumerable<HistoryNode<int, TValue>> EndBackwards()
        {
            for (int i = m_Newest; i >= m_oldest; i--)
            {
                Node node = this[i];
                if (node.isSet)
                {
                    yield return new HistoryNode<int, TValue> { time = i, value = node.value };
                }
            }
        }

        public IEnumerable<HistoryNode<int, TValue>> Begin()
        {
            int newest = m_Newest;
            for (int i = m_oldest; i <= newest; i++)
            {
                Node node = this[i];
                if (node.isSet)
                {
                    yield return new HistoryNode<int, TValue> { time = i, value = node.value };
                }
            }
        }

        public IEnumerable<HistoryNode<int, TValue>> UntilBackwards(int _time, bool _allowAfter = true, bool _allowBefore = false)
        {
            if (!Contains(_time) || !this[_time].isSet)
            {
                if (_allowAfter)
                {
                    HistoryNode<int, TValue>? next = this.Next(_time);
                    if (next != null)
                    {
                        yield return next.Value;
                    }
                    else if (!_allowBefore)
                    {
                        yield break;
                    }
                }
                else if (!_allowBefore)
                {
                    yield break;
                }
            }
            for (int i = _time; i >= m_oldest; i--)
            {
                Node node = this[i];
                if (node.isSet)
                {
                    yield return new HistoryNode<int, TValue> { time = i, value = node.value };
                }
            }
        }

        public IEnumerable<HistoryNode<int, TValue>> Since(int _time, bool _allowBefore = true, bool _allowAfter = false)
        {
            if (!Contains(_time) || !this[_time].isSet)
            {
                if (_allowBefore)
                {
                    HistoryNode<int, TValue>? last = this.Last(_time);
                    if (last != null)
                    {
                        yield return last.Value;
                    }
                    else if (!_allowAfter)
                    {
                        yield break;
                    }
                }
                else if (!_allowAfter)
                {
                    yield break;
                }
            }
            int newest = m_Newest;
            for (int i = _time; i <= newest; i++)
            {
                Node node = this[i];
                if (node.isSet)
                {
                    yield return new HistoryNode<int, TValue> { time = i, value = node.value };
                }
            }
        }

        public IEnumerable<HistoryNode<int, TValue>> UntilBackwardsSequenced(int _time)
        {
            if (_time <= m_Newest)
            {
                for (int i = _time; i >= m_oldest; i--)
                {
                    Node node = this[i];
                    if (!node.isSet)
                    {
                        yield break;
                    }
                    yield return new HistoryNode<int, TValue> { time = i, value = node.value };
                }
            }
        }

        public IEnumerable<HistoryNode<int, TValue>> SinceSequenced(int _time)
        {
            if (_time >= m_oldest)
            {
                int newest = m_Newest;
                for (int i = _time; i <= newest; i++)
                {
                    Node node = this[i];
                    if (!node.isSet)
                    {
                        yield break;
                    }
                    yield return new HistoryNode<int, TValue> { time = i, value = node.value };
                }
            }
        }

        private bool Contains(int _time)
        {
            return _time >= m_oldest && _time <= m_Newest;
        }
    }
}