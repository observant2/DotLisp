using System;
using System.Collections.Generic;
using System.Linq;
using DotLisp.Types;
using Newtonsoft.Json;

namespace DotLisp
{
    public static class Util
    {
        public static string ReplaceFirst(this string text, string search,
            string replace)
        {
            var pos = text.IndexOf(search, StringComparison.Ordinal);
            if (pos < 0)
            {
                return text;
            }

            return text.Substring(0, pos) + replace +
                   text.Substring(pos + search.Length);
        }

        public static LinkedList<T> ToLinkedList<T>(this IEnumerable<T> list)
        {
            return new LinkedList<T>(list);
        }

        public static DotList ToDotList(this IEnumerable<DotExpression> list)
        {
            return new DotList()
            {
                Expressions = list.ToLinkedList()
            };
        }

        public static DotBool ToDotBool(this bool val)
        {
            return val ? DotBool.True() : DotBool.False();
        }

        public static string PrettyPrint(this object o)
        {
            return JsonConvert.SerializeObject(o, Formatting.Indented,
                new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });
        }

        public static IEnumerable<TResult> Pairwise<T, TResult>(
            this IEnumerable<T> enumerable,
            Func<T, T, TResult> selector)
        {
            var list = enumerable.ToList();
            var previous = list.First();
            foreach (var item in list.Skip(1))
            {
                yield return selector(previous, item);
                previous = item;
            }
        }
    }
}