using System.Linq.Expressions;
using VirtualQueueApi.Models.Entities;
using VirtualQueueApi.Utils.Extensions;

namespace VirtualQueueApi.Domain.Models.Queries
{
    public class UserSearch : Search<User, int>
    {
        public int Id { get; set; } = 0;
        public Guid ExternalId { get; set; } = Guid.Empty;
        public int CompanyId { get; set; } = 0;
        public string Email { get; set; } = string.Empty;

        public UserSearch() { }
        public UserSearch(int page, int rows) : base(page, rows) { }

        public override Expression<Func<User, bool>> CreateExpression(Expression<Func<User, bool>>? expression = null)
        {
            // Se a expressão for nula, começa com uma expressão que é sempre verdadeira
            if (expression == null)
                expression = (user => true);

            if (Id > 0) 
                expression = expression.AndAlso(x => x.Id == Id);

            if (!IncludeDelete)
                expression = expression.AndAlso(x => !x.Deleted);

            if (ExternalId != Guid.Empty)
                expression = expression.AndAlso(x => x.ExternalId == ExternalId);

            if (CompanyId > 0)
                expression = expression.AndAlso(x => x.CompanyId == CompanyId);

            if (!string.IsNullOrEmpty(Email))            
                expression = expression.AndAlso(x => x.Email == Email);            

            return expression;
        }
    }
}
