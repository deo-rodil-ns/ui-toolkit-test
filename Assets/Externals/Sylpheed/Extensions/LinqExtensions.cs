using System;
using System.Collections.Generic;
using System.Linq;

namespace Sylpheed.Extensions
{
    public static class LinqExtensions
    {
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>
            (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }
        
        public static IEnumerable<T> Except<T>(this IEnumerable<T> source, T value)
        {
            return source.Except(new[] { value });
        }
        
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var element in source)
            {
                action(element);
            }
        }

        public static void IndexedForEach<T>(this IEnumerable<T> source, Action<int, T> action)
        {
            var index = 0;
            foreach (var element in source)
            {
                action(index, element);
                index++;
            }
        }

        public static T Random<T>(this IEnumerable<T> source)
        {
            return source.OrderBy(e => UnityEngine.Random.value).First();
        }
        
        public static T RandomOrDefault<T>(this IEnumerable<T> source)
        {
            return source.OrderBy(e => UnityEngine.Random.value).FirstOrDefault();
        }
    }
}