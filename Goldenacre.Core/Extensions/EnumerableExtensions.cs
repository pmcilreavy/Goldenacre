namespace Goldenacre.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class EnumerableExtensions
    {
        public static bool IsFirstElement<T>(this IEnumerable<T> @this, T element) where T : class
        {
            var first = @this.FirstOrDefault();

            return first != null && first.Equals(element);
        }

        public static bool IsLastElement<T>(this IEnumerable<T> @this, T element) where T : class
        {
            var last = @this.LastOrDefault();

            return last != null && last.Equals(element);
        }

        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> @this,
            Func<TSource, TKey> keySelector)
        {
            var seenKeys = new HashSet<TKey>();
            return @this.Where(element => seenKeys.Add(keySelector(element)));
        }

        public static void ForEach<T>(this IEnumerable<T> @this, Action<T> action)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(null, "@this");
            }
            if (action == null)
            {
                throw new ArgumentNullException(null, "action");
            }

            foreach (var element in @this)
            {
                action(element);
            }
        }

        public static void ForEach<T>(this IEnumerable<T> source, Action<int, T> action)
        {
            if (source == null)
            {
                throw new ArgumentNullException(null, "source");
            }
            if (action == null)
            {
                throw new ArgumentNullException(null, "action");
            }

            var index = 0;
            foreach (var element in source)
            {
                action(index, element);

                index++;
            }
        }

        /// <summary>
        /// Batches the source IEnumerable with the specified batch size.
        /// </summary>
        /// <param name="source">The source Enumerable.</param>
        /// <param name="batchSize">Size of the batch.</param>
        public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> source, int batchSize)
        {
            using (var enumerator = source.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    yield return YieldBatchElements(enumerator, batchSize - 1);
                }
            }
        }

        private static IEnumerable<T> YieldBatchElements<T>(IEnumerator<T> source, int batchSize)
        {
            yield return source.Current;
            for (var i = 0; i < batchSize && source.MoveNext(); i++)
            {
                yield return source.Current;
            }
        }
    }
}