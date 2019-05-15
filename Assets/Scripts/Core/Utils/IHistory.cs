using System;
using System.Collections.Generic;
using System.Linq;

namespace Wheeled.Core.Utils
{
    public interface IReadOnlyHistory<TTime, TValue> : IReadOnlySimpleHistory<HistoryNode<TTime, TValue>, TTime> where TTime : struct, IComparable<TTime>
    {
    }

    public interface IHistory<TTime, TValue> : ISimpleHistory<HistoryNode<TTime, TValue>, TTime>, IReadOnlyHistory<TTime, TValue> where TTime : struct, IComparable<TTime>
    {
        void Set(TTime _time, TValue _value);
    }

    public interface IReadOnlySimpleHistory<TNode, TComparer> where TNode : struct, IComparable<TComparer> where TComparer : struct
    {
        IEnumerable<TNode> EndBackwards();

        IEnumerable<TNode> Begin();

        IEnumerable<TNode> UntilBackwards(TComparer _time, bool _allowAfter = true, bool _allowBefore = false);

        IEnumerable<TNode> Since(TComparer _time, bool _allowBefore = true, bool _allowAfter = false);
    }

    public interface ISimpleHistory<TNode, TComparer> : IReadOnlySimpleHistory<TNode, TComparer> where TNode : struct, IComparable<TComparer> where TComparer : struct
    {
        void Set(TComparer _time, TNode _value);
    }

    public static class HistoryHelper
    {
        public static bool IsEqualTo<T>(this IComparable<T> _item, T _other) where T : struct
        {
            return _item.CompareTo(_other) == 0;
        }

        public static bool IsGreaterThan<T>(this IComparable<T> _item, T _other) where T : struct
        {
            return _item.CompareTo(_other) > 0;
        }

        public static bool IsLessThan<T>(this IComparable<T> _item, T _other) where T : struct
        {
            return _item.CompareTo(_other) < 0;
        }

        public static void Around<TNode, TComparer>(this IReadOnlySimpleHistory<TNode, TComparer> _history, TComparer _time, out TNode? _outA, out TNode? _outB)
            where TNode : struct, IComparable<TComparer> where TComparer : struct
        {
            _outA = null;
            _outB = null;
            foreach (TNode node in _history.Since(_time, true, true))
            {
                if (node.IsGreaterThan(_time))
                {
                    _outB = node;
                    break;
                }
                else
                {
                    _outA = node;
                }
            }
        }

        public static TNode? Last<TNode, TComparer>(this IReadOnlySimpleHistory<TNode, TComparer> _history, TComparer _time)
            where TNode : struct, IComparable<TComparer> where TComparer : struct

        {
            return _history.UntilBackwards(_time, false, true).Cast<TNode?>().FirstOrDefault();
        }

        public static TNode? Last<TNode, TComparer>(this IReadOnlySimpleHistory<TNode, TComparer> _history, TComparer _time, Func<TNode, bool> _where)
            where TNode : struct, IComparable<TComparer> where TComparer : struct

        {
            return _history.UntilBackwards(_time, false, true).Where(_where).Cast<TNode?>().FirstOrDefault();
        }

        public static TNode? Next<TNode, TComparer>(this IReadOnlySimpleHistory<TNode, TComparer> _history, TComparer _time)
            where TNode : struct, IComparable<TComparer> where TComparer : struct

        {
            return _history.Since(_time, false, true).Cast<TNode?>().FirstOrDefault();
        }

        public static TNode? Next<TNode, TComparer>(this IReadOnlySimpleHistory<TNode, TComparer> _history, TComparer _time, Func<TNode, bool> _where)
            where TNode : struct, IComparable<TComparer> where TComparer : struct

        {
            return _history.Since(_time, false, true).Where(_where).Cast<TNode?>().FirstOrDefault();
        }

        public static IEnumerable<TNode> Between<TNode, TComparer>(this IReadOnlySimpleHistory<TNode, TComparer> _history, TComparer _since, TComparer _until)
            where TNode : struct, IComparable<TComparer> where TComparer : struct
        {
            return _history.Since(_since, false, true).TakeWhile(_n => !_n.IsGreaterThan(_until));
        }

        public static IEnumerable<TNode> Until<TNode, TComparer>(this IReadOnlySimpleHistory<TNode, TComparer> _history, TComparer _until)
            where TNode : struct, IComparable<TComparer> where TComparer : struct
        {
            return _history.Begin().TakeWhile(_n => !_n.IsGreaterThan(_until));
        }

        public static IEnumerable<TNode> Sequenced<TComparer, TNode>(this IEnumerable<TNode> _enumerable, TComparer _start, Func<TComparer, TComparer> _step)
            where TNode : struct, IComparable<TComparer> where TComparer : struct
        {
            TComparer sequenceTime = _start;
            foreach (TNode node in _enumerable)
            {
                if (!node.IsEqualTo(sequenceTime))
                {
                    break;
                }
                sequenceTime = _step(sequenceTime);
                yield return node;
            }
        }

        public static IEnumerable<TNode> Sequenced<TNode>(this IEnumerable<TNode> _enumerable, int _start, int _step)
            where TNode : struct, IComparable<int>
        {
            int sequenceTime = _start;
            foreach (TNode node in _enumerable)
            {
                if (!node.IsEqualTo(sequenceTime))
                {
                    break;
                }
                sequenceTime += _step;
                yield return node;
            }
        }
    }

    public struct HistoryNode<TTime, TValue> : IComparable<TTime> where TTime : struct, IComparable<TTime>
    {
        public TTime time;
        public TValue value;

        public int CompareTo(TTime _time)
        {
            return time.CompareTo(_time);
        }

        public void Deconstruct(out TTime _outTime, out TValue _outValue)
        {
            _outTime = time;
            _outValue = value;
        }
    }
}