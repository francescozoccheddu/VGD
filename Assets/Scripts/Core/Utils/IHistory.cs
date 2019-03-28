using System;
using System.Collections.Generic;

namespace Wheeled.Core.Utils
{
    internal static class IHistoryHelpers
    {

        public static bool IsGreaterThan<T>(this IComparable<T> _item, T _other) where T : struct
        {
            return _item.CompareTo(_other) > 0;
        }

        public static bool IsLessThan<T>(this IComparable<T> _item, T _other) where T : struct
        {
            return _item.CompareTo(_other) < 0;
        }

        public static bool IsEqualTo<T>(this IComparable<T> _item, T _other) where T : struct
        {
            return _item.CompareTo(_other) == 0;
        }

    }

    internal struct HistoryNode<TTime, TValue>
    {
        public TTime time;
        public TValue entry;
    }

    internal interface IHistory<TTime, TValue> where TTime : struct, IComparable<TTime>
    {

        void Clear();

        void ForgetOlder(TTime _time, bool _keepOldest);

        void ForgetNewer(TTime _time, bool _keepNewest);

        void ForgetAndOlder(TTime _time);

        void ForgetAndNewer(TTime _time);

        void Set(TTime _time, TValue _value);

        HistoryNode<TTime, TValue>? Get(TTime _time);

        void Query(TTime _time, out HistoryNode<TTime, TValue>? _outA, out HistoryNode<TTime, TValue>? _outB);

        IEnumerable<HistoryNode<TTime, TValue>> GetFullSequence();

        IEnumerable<HistoryNode<TTime, TValue>> GetFullReversedSequence();

        IEnumerable<HistoryNode<TTime, TValue>> GetSequenceSince(TTime _time, bool _allowBefore = true, bool _allowAfter = false);

        IEnumerable<HistoryNode<TTime, TValue>> GetReversedSequenceSince(TTime _time, bool _allowAfter = true, bool _allowBefore = false);

        TTime? OldestTime { get; }
        TTime? NewestTime { get; }

    }

}
