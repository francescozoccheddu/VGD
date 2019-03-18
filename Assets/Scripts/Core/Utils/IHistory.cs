using System;
using System.Collections.Generic;

namespace Wheeled.Core.Utils
{
    internal static class IComparableHelpers
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

    internal interface IHistory<TTime, TValue> where TTime : struct, IComparable<TTime> where TValue : struct
    {

        void Clear();

        void ForgetOlder(TTime _time, bool _keepOldest);

        void ForgetNewer(TTime _time, bool _keepNewest);

        void ForgetAndOlder(TTime _time);

        void ForgetAndNewer(TTime _time);

        void Set(TTime _time, TValue _value);

        TValue? Get(TTime _time);

        void Query(TTime _time, out HistoryNode<TTime, TValue>? _outA, out HistoryNode<TTime, TValue>? _outB);

        IEnumerable<HistoryNode<TTime, TValue>> GetSequence(TTime _maxStartingTime);

        IEnumerable<HistoryNode<TTime, TValue>> GetReversedSequence(TTime _minStartingTime);

        TTime? OldestTime { get; }
        TTime? NewestTime { get; }
        TValue? Newest { get; }
        TValue? Oldest { get; }

    }

}
