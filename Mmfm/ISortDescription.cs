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
        string Name { get; }

        Expression<Func<T, object>> SortExpression { get; }

        bool? IsDescending { get; set; }
    }

    public static class ISortDescriptionExtension
    {
        public static void ToggleIsDescending<T>(this ISortDescription<T> desc)
        {
            desc.IsDescending = !desc.IsDescending ?? true;
        }

        public static IOrderedQueryable<T> OrderBy<T>(this ISortDescription<T> desc, IQueryable<T> source)
        {
            if (desc.IsDescending == true)
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
            if (desc.IsDescending == true)
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
