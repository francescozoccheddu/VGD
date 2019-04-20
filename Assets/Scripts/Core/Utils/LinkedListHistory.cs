using System;
using System.Collections.Generic;

namespace Wheeled.Core.Utils
{
    internal sealed class LinkedListHistory<TTime, TValue> : LinkedListSimpleHistory<HistoryNode<TTime, TValue>, TTime>, IHistory<TTime, TValue> where TTime : struct, IComparable<TTime>
    {
        #region Public Methods

        public void Add(TTime _time, TValue _value)
        {
            Add(_time, new HistoryNode<TTime, TValue> { time = _time, value = _value });
        }

        public void Set(TTime _time, TValue _value)
        {
            Set(_time, new HistoryNode<TTime, TValue> { time = _time, value = _value });
        }

        #endregion Public Methods
    }

    internal class LinkedListSimpleHistory<TNode, TComparer> : ISimpleHistory<TNode, TComparer> where TNode : struct, IComparable<TComparer> where TComparer : struct
    {
        #region Private Fields

        private readonly LinkedList<TNode> m_list = new LinkedList<TNode>();

        #endregion Private Fields

        #region Public Methods

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

        public void ForgetAndOlder(TComparer _time)
        {
            LinkedListNode<TNode> node = m_list.First;
            while (node?.Value.IsGreaterThan(_time) == false)
            {
                m_list.RemoveFirst();
                node = node.Next;
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

        public IEnumerable<TNode> EndBackwards()
        {
            LinkedListNode<TNode> listNode = m_list.Last;
            while (listNode != null)
            {
                yield return listNode.Value;
                listNode = listNode.Previous;
            }
        }

        public IEnumerable<TNode> Begin()
        {
            LinkedListNode<TNode> listNode = m_list.First;
            while (listNode != null)
            {
                yield return listNode.Value;
                listNode = listNode.Next;
            }
        }

        public IEnumerable<TNode> UntilBackwards(TComparer _time, bool _allowAfter = true, bool _allowBefore = false)
        {
            LinkedListNode<TNode> listNode = GetNode(_time, _allowBefore, _allowAfter);
            while (listNode != null)
            {
                yield return listNode.Value;
                listNode = listNode.Previous;
            }
        }

        public IEnumerable<TNode> Since(TComparer _time, bool _allowBefore = true, bool _allowAfter = false)
        {
            LinkedListNode<TNode> listNode = GetNode(_time, _allowBefore, _allowAfter);
            while (listNode != null)
            {
                yield return listNode.Value;
                listNode = listNode.Next;
            }
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

        #endregion Public Methods

        #region Private Methods

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

        private LinkedListNode<TNode> GetNodeOrPrevious(TComparer _time)
        {
            LinkedListNode<TNode> node = m_list.Last;
            while (node?.Value.IsGreaterThan(_time) == true)
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

        #endregion Private Methods
    }

    internal class LinkedListSimpleHistory<T> : LinkedListSimpleHistory<T, T> where T : struct, IComparable<T>
    {
        #region Public Methods

        public void Add(T _node)
        {
            Add(_node, _node);
        }

        public void Set(T _node)
        {
            Set(_node, _node);
        }

        #endregion Public Methods
    }
}