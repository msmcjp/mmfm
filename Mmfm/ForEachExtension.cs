using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mmfm
{
    public static class ForEachExtension
    {
        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> block)
        {
            foreach (var x in enumerable)
            {
                block(x);
            }
        }

        public static void ForEach(this ExpandoObject expando, Action<KeyValuePair<string, object>> block)
        {
            foreach(var x in expando)
            {
                block(x);
            }
        }

    }
}
