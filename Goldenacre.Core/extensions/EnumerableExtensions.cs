using System;
using System.Collections.Generic;
using System.Linq;

namespace Goldenacre.Core.Extensions
{
    public static class EnumerableExtensions
    {
        public static bool IsLastElement<T>(this IEnumerable<T> items, T element)
            where T : class
        {
            var last = items.LastOrDefault();

            return last != null && last.Equals(element);
        }

        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector)
        {
            var seenKeys = new HashSet<TKey>();
            return source.Where(element => seenKeys.Add(keySelector(element)));
        }

        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (action == null) throw new ArgumentNullException("action");

            foreach (var element in source)
            {
                action(element);
            }
        }

        public static void ForEach<T>(this IEnumerable<T> source, Action<int, T> action)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (action == null) throw new ArgumentNullException("action");

            var index = 0;
            foreach (var element in source)
            {
                action(index, element);

                index++;
            }
        }

        public static T[] ForEach<T>(this T[] source, Func<int, T, T> func)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (func == null) throw new ArgumentNullException("func");

            for (var i = 0; i < source.Length; i++)
            {
                source[i] = func(i, source[i]);
            }

            return source;
        }

        public static T[] ForEach<T>(this T[] source, Func<T, T> func)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (func == null) throw new ArgumentNullException("func");

            for (var i = 0; i < source.Length; i++)
            {
                source[i] = func(source[i]);
            }

            return source;
        }
    }
}