using System.Linq.Expressions;
using VirtualQueueApi.Domain.Entities;

namespace VirtualQueueApi.Domain.Models.Queries
{
    public abstract class Search<T, TType> where T : EntityBase<TType>
    {
        public int Page { get; set; } = 1;
        public int Rows { get; set; } = 10;
        public bool IncludeDelete { get; set; } = false;
        public bool TrackQuery { get; set; } = true;
        public int SkipPages() => (Page - 1) * Rows;
        public Func<IQueryable<T>, IOrderedQueryable<T>>? OrderBy;
        public abstract Expression<Func<T, bool>> CreateExpression(Expression<Func<T, bool>>? expression = null);

        protected Search() { }
        protected Search(int page, int rows)
        {
            Page = page < 1 ? 1 : page;
            Rows = rows < 1 ? 10 : rows;
        }
    }
}
