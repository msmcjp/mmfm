using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Mmfm
{
    public interface ISortDescription<T>
    {
        string DisplayName { get; }

        Expression<Func<T, object>> SortExpression { get; }

        bool IsDescending { get; }
    }

    public static class ISortDescriptionExtension
    {
        public static IOrderedQueryable<T> OrderBy<T>(this ISortDescription<T> desc, IQueryable<T> source)
        {
            if (desc.IsDescending)
            {
                return source.OrderByDescending(desc.SortExpression);
            }
            else
            {
                return source.OrderBy(desc.SortExpression);
            }
        }

        public static IOrderedQueryable<T> ThenBy<T>(this ISortDescription<T> desc, IOrderedQueryable<T> source)
        {
            if (desc.IsDescending)
            {
                return source.ThenByDescending(desc.SortExpression);
            }
            else
            {
                return source.ThenBy(desc.SortExpression);
            }
        }
    }
}
