using System;
using System.Collections.Generic;

namespace Wheeled.Core.Utils
{
    internal interface IHistory<TTime, TValue> : ISimpleHistory<HistoryNode<TTime, TValue>, TTime> where TTime : struct, IComparable<TTime>
    {
        TTime? NewestTime { get; }

        TTime? OldestTime { get; }

        void Set(TTime _time, TValue _value);
    }

    internal interface ISimpleHistory<TNode, TComparer> where TNode : struct, IComparable<TComparer> where TComparer : struct
    {
        TNode? Newest { get; }

        TNode? Oldest { get; }

        void Clear();

        void ForgetAndNewer(TComparer _time);

        void ForgetAndOlder(TComparer _time);

        void ForgetNewer(TComparer _time, bool _keepNewest);

        void ForgetOlder(TComparer _time, bool _keepOldest);

        TNode? Get(TComparer _time);

        IEnumerable<TNode> GetFullReversedSequence();

        IEnumerable<TNode> GetFullSequence();

        TNode? GetOrNext(TComparer _time);

        TNode? GetOrPrevious(TComparer _time);

        TNode? GetOrPreviousOrNext(TComparer _time);

        IEnumerable<TNode> GetReversedSequenceSince(TComparer _time, bool _allowAfter = true, bool _allowBefore = false);

        IEnumerable<TNode> GetSequenceSince(TComparer _time, bool _allowBefore = true, bool _allowAfter = false);

        bool Has(TComparer _time);

        void Query(TComparer _time, out TNode? _outA, out TNode? _outB);

        void Set(TComparer _time, TNode _value);
    }

    internal struct HistoryNode<TTime, TValue> : IComparable<TTime> where TTime : struct, IComparable<TTime>
    {
        public TTime time;
        public TValue value;

        public int CompareTo(TTime _time)
        {
            return time.CompareTo(_time);
        }
    }

    internal static class IHistoryHelpers
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
    }
}