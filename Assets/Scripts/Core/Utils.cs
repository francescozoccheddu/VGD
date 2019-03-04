using System.Collections.Generic;

namespace Wheeled.Core
{
    internal static class Utils
    {

        public static IEnumerable<T> SingletonEnumerable<T>(this T item)
        {
            yield return item;
        }

    }

    internal class InnerClass<T>
    {

        protected readonly T m_parent;

        protected InnerClass(T _parent)
        {
            m_parent = _parent;
        }

    }

}
