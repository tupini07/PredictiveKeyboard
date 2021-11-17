using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.Extensions
{
    public static class IEnumerableExtensions
    {
        public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> source, int batchSize)
        {
            var result = new List<List<T>> { new List<T>() };

            foreach (var element in source)
            {
                if (result.Last().Count == batchSize)
                {
                    result.Add(new List<T>());
                }

                result.Last().Add(element);
            }

            return result;
        }
    }
}
