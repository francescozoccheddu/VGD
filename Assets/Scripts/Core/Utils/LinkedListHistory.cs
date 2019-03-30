using System;
using System.Collections.Generic;

namespace Wheeled.Core.Utils
{

    internal class LinkedListSimpleHistory<TNode, TComparer> : ISimpleHistory<TNode, TComparer> where TNode : struct, IComparable<TComparer> where TComparer : struct
    {

        private readonly LinkedList<TNode> m_list = new LinkedList<TNode>();

        public TNode? Oldest => m_list.First?.Value;
        public TNode? Newest => m_list.Last?.Value;

        private LinkedListNode<TNode> GetNodeOrPrevious(TComparer _time)
        {
            LinkedListNode<TNode> node = m_list.Last;
            while (node?.Value.IsGreaterThan(_time) == true)
            {
                node = node.Previous;
            }
            return node;
        }

        private LinkedListNode<TNode> GetNodeOrNext(TComparer _time)
        {
            LinkedListNode<TNode> node = m_list.Last;
            if (node?.Value.IsLessThan(_time) != false)
            {
                return null;
            }
            while (node.Previous?.Value.IsLessThan(_time) == false)
            {
                node = node.Previous;
            }
            return node;
        }

        private LinkedListNode<TNode> GetNodeOrPreviousOrNext(TComparer _time)
        {
            LinkedListNode<TNode> node = m_list.First;
            while (node?.Next?.Value.IsGreaterThan(_time) == false)
            {
                node = node.Next;
            }
            return node;
        }

        private LinkedListNode<TNode> GetNode(TComparer _time, bool _allowBefore, bool _allowAfter)
        {
            LinkedListNode<TNode> listNode;
            if (_allowAfter && _allowBefore)
            {
                listNode = GetNodeOrPreviousOrNext(_time);
            }
            else if (_allowAfter)
            {
                listNode = GetNodeOrNext(_time);
            }
            else
            {
                listNode = GetNodeOrPrevious(_time);
                if (!_allowBefore && listNode?.Value.IsEqualTo(_time) != true)
                {
                    listNode = null;
                }
            }
            return listNode;
        }

        public void Clear()
        {
            m_list.Clear();
        }

        public void ForgetAndNewer(TComparer _time)
        {
            LinkedListNode<TNode> node = m_list.Last;
            while (node?.Value.IsLessThan(_time) == false)
            {
                m_list.RemoveLast();
                node = node.Previous;
            }
        }

        public void ForgetAndOlder(TComparer _time)
        {
            LinkedListNode<TNode> node = m_list.First;
            while (node?.Value.IsGreaterThan(_time) == false)
            {
                m_list.RemoveFirst();
                node = node.Next;
            }
        }

        public void ForgetNewer(TComparer _time, bool _keepNewest)
        {
            LinkedListNode<TNode> node = m_list.Last;
            if (_keepNewest)
            {
                while (node?.Next?.Value.IsLessThan(_time) == false)
                {
                    m_list.RemoveLast();
                    node = node.Previous;
                }
            }
            else
            {
                while (node?.Value.IsGreaterThan(_time) == true)
                {
                    m_list.RemoveLast();
                    node = node.Previous;
                }
            }
        }

        public void ForgetOlder(TComparer _time, bool _keepOldest)
        {
            LinkedListNode<TNode> node = m_list.First;
            if (_keepOldest)
            {
                while (node?.Next?.Value.IsGreaterThan(_time) == false)
                {
                    m_list.RemoveFirst();
                    node = node.Next;
                }
            }
            else
            {
                while (node?.Value.IsLessThan(_time) == true)
                {
                    m_list.RemoveFirst();
                    node = node.Next;
                }
            }
        }

        public void Query(TComparer _time, out TNode? _outA, out TNode? _outB)
        {
            LinkedListNode<TNode> node = m_list.Last;
            LinkedListNode<TNode> lastNode = null;
            while (node?.Value.IsGreaterThan(_time) == true)
            {
                lastNode = node;
                node = node.Previous;
            }
            _outA = node?.Value;
            _outB = lastNode?.Value;
        }

        public void Set(TComparer _time, TNode _value)
        {
            LinkedListNode<TNode> listNode = GetNodeOrPrevious(_time);
            if (listNode == null)
            {
                m_list.AddFirst(_value);
            }
            else if (listNode.Value.IsEqualTo(_time))
            {
                listNode.Value = _value;
            }
            else
            {
                m_list.AddAfter(listNode, _value);
            }
        }

        public void Add(TComparer _time, TNode _value)
        {
            LinkedListNode<TNode> listNode = GetNodeOrPrevious(_time);
            if (listNode == null)
            {
                m_list.AddFirst(_value);
            }
            else
            {
                m_list.AddAfter(listNode, _value);
            }
        }

        public IEnumerable<TNode> GetFullSequence()
        {
            LinkedListNode<TNode> listNode = m_list.First;
            while (listNode != null)
            {
                yield return listNode.Value;
                listNode = listNode.Next;

            }
        }

        public IEnumerable<TNode> GetFullReversedSequence()
        {
            LinkedListNode<TNode> listNode = m_list.Last;
            while (listNode != null)
            {
                yield return listNode.Value;
                listNode = listNode.Previous;

            }
        }

        public IEnumerable<TNode> GetSequenceSince(TComparer _time, bool _allowBefore = true, bool _allowAfter = false)
        {
            LinkedListNode<TNode> listNode = GetNode(_time, _allowBefore, _allowAfter);
            while (listNode != null)
            {
                yield return listNode.Value;
                listNode = listNode.Next;
            }
        }

        public IEnumerable<TNode> GetReversedSequenceSince(TComparer _time, bool _allowAfter = true, bool _allowBefore = false)
        {
            LinkedListNode<TNode> listNode = GetNode(_time, _allowBefore, _allowAfter);
            while (listNode != null)
            {
                yield return listNode.Value;
                listNode = listNode.Previous;
            }
        }

        public bool Has(TComparer _time)
        {
            LinkedListNode<TNode> node = GetNodeOrPrevious(_time);
            return node != null && node.Value.IsEqualTo(_time);
        }

        public TNode? Get(TComparer _time)
        {
            LinkedListNode<TNode> node = GetNodeOrPrevious(_time);
            return node?.Value.IsEqualTo(_time) == true ? node.Value : (TNode?) null;
        }

        public TNode? GetOrPrevious(TComparer _time)
        {
            return GetNodeOrPrevious(_time)?.Value;
        }

        public TNode? GetOrNext(TComparer _time)
        {
            return GetNodeOrNext(_time)?.Value;
        }

        public TNode? GetOrPreviousOrNext(TComparer _time)
        {
            return GetNodeOrPreviousOrNext(_time)?.Value;
        }
    }

    internal class LinkedListSimpleHistory<T> : LinkedListSimpleHistory<T, T> where T : struct, IComparable<T>
    {

        public void Set(T _node)
        {
            Set(_node, _node);
        }

        public void Add(T _node)
        {
            Add(_node, _node);
        }

    }

    internal sealed class LinkedListHistory<TTime, TValue> : LinkedListSimpleHistory<HistoryNode<TTime, TValue>, TTime>, IHistory<TTime, TValue> where TTime : struct, IComparable<TTime>
    {

        public TTime? OldestTime => Oldest?.time;
        public TTime? NewestTime => Oldest?.time;

        public void Add(TTime _time, TValue _value)
        {
            Add(_time, new HistoryNode<TTime, TValue> { time = _time, value = _value });
        }

        public void Set(TTime _time, TValue _value)
        {
            Set(_time, new HistoryNode<TTime, TValue> { time = _time, value = _value });
        }

    }

}
