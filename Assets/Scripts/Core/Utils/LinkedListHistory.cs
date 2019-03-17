using System;
using System.Collections.Generic;

namespace Wheeled.Core.Utils
{
    internal sealed class LinkedListHistory<TTime, TValue> : IHistory<TTime, TValue> where TTime : struct, IComparable<TTime> where TValue : struct
    {

        private readonly LinkedList<HistoryNode<TTime, TValue>> m_list = new LinkedList<HistoryNode<TTime, TValue>>();

        public TTime? OldestTime => m_list.First?.Value.time;

        public TTime? NewestTime => m_list.Last?.Value.time;

        private LinkedListNode<HistoryNode<TTime, TValue>> GetNodeOrPrevious(TTime _time)
        {
            LinkedListNode<HistoryNode<TTime, TValue>> node = m_list.Last;
            while (node?.Value.time.IsGreaterThan(_time) == true)
            {
                node = node.Previous;
            }
            return node;
        }

        private LinkedListNode<HistoryNode<TTime, TValue>> GetNodeOrNext(TTime _time)
        {
            LinkedListNode<HistoryNode<TTime, TValue>> node = m_list.Last;
            if (node?.Value.time.IsLessThan(_time) != false)
            {
                return null;
            }
            while (node.Previous?.Value.time.IsLessThan(_time) == false)
            {
                node = node.Previous;
            }
            return node;
        }

        public void Clear()
        {
            m_list.Clear();
        }

        public void ForgetAndNewer(TTime _time)
        {
            LinkedListNode<HistoryNode<TTime, TValue>> node = m_list.Last;
            while (node?.Value.time.IsLessThan(_time) == false)
            {
                m_list.RemoveLast();
                node = node.Previous;
            }
        }

        public void ForgetAndOlder(TTime _time)
        {
            LinkedListNode<HistoryNode<TTime, TValue>> node = m_list.First;
            while (node?.Value.time.IsGreaterThan(_time) == false)
            {
                m_list.RemoveFirst();
                node = node.Next;
            }
        }

        public void ForgetNewer(TTime _time, bool _keepNewest)
        {
            LinkedListNode<HistoryNode<TTime, TValue>> node = m_list.Last;
            if (_keepNewest)
            {
                while (node?.Next?.Value.time.IsLessThan(_time) == false)
                {
                    m_list.RemoveLast();
                    node = node.Previous;
                }
            }
            else
            {
                while (node?.Value.time.IsGreaterThan(_time) == true)
                {
                    m_list.RemoveLast();
                    node = node.Previous;
                }
            }
        }

        public void ForgetOlder(TTime _time, bool _keepOldest)
        {
            LinkedListNode<HistoryNode<TTime, TValue>> node = m_list.First;
            if (_keepOldest)
            {
                while (node?.Next?.Value.time.IsGreaterThan(_time) == false)
                {
                    m_list.RemoveFirst();
                    node = node.Next;
                }
            }
            else
            {
                while (node?.Value.time.IsLessThan(_time) == true)
                {
                    m_list.RemoveFirst();
                    node = node.Next;
                }
            }
        }

        public TValue? Get(TTime _time)
        {
            LinkedListNode<HistoryNode<TTime, TValue>> node = GetNodeOrPrevious(_time);
            if (node?.Value.time.IsEqualTo(_time) == true)
            {
                return node.Value.entry;
            }
            else
            {
                return null;
            }
        }

        public void Query(TTime _time, out HistoryNode<TTime, TValue>? _outA, out HistoryNode<TTime, TValue>? _outB)
        {
            LinkedListNode<HistoryNode<TTime, TValue>> node = m_list.Last;
            LinkedListNode<HistoryNode<TTime, TValue>> lastNode = null;
            while (node?.Value.time.IsGreaterThan(_time) == true)
            {
                lastNode = node;
                node = node.Previous;
            }
            _outA = node?.Value;
            _outB = lastNode?.Value;
        }

        public IEnumerable<HistoryNode<TTime, TValue>> GetReversedSequence(TTime _minStartingTime)
        {
            LinkedListNode<HistoryNode<TTime, TValue>> listNode = GetNodeOrNext(_minStartingTime);
            while (listNode != null)
            {
                yield return listNode.Value;
                listNode = listNode.Previous;
            }
        }

        public IEnumerable<HistoryNode<TTime, TValue>> GetSequence(TTime _maxStartingTime)
        {
            LinkedListNode<HistoryNode<TTime, TValue>> listNode = GetNodeOrPrevious(_maxStartingTime);
            while (listNode != null)
            {
                yield return listNode.Value;
                listNode = listNode.Next;
            }
        }

        public void Set(TTime _time, TValue _value)
        {
            LinkedListNode<HistoryNode<TTime, TValue>> listNode = GetNodeOrPrevious(_time);
            HistoryNode<TTime, TValue> node = new HistoryNode<TTime, TValue> { time = _time, entry = _value };
            if (listNode == null)
            {
                m_list.AddFirst(node);
            }
            else if (listNode.Value.time.IsEqualTo(_time))
            {
                listNode.Value = node;
            }
            else
            {
                m_list.AddAfter(listNode, node);
            }
        }

        public void Add(TTime _time, TValue _value)
        {
            LinkedListNode<HistoryNode<TTime, TValue>> listNode = GetNodeOrPrevious(_time);
            HistoryNode<TTime, TValue> node = new HistoryNode<TTime, TValue> { time = _time, entry = _value };
            if (listNode == null)
            {
                m_list.AddFirst(node);
            }
            else
            {
                m_list.AddAfter(listNode, node);
            }
        }

    }

}
