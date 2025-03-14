using System.Linq.Expressions;
using VirtualQueueApi.Domain.Entities;
using VirtualQueueApi.Utils.Extensions;

namespace VirtualQueueApi.Domain.Models.Queries;

public class SubscriptionManagementSearch : Search<SubscriptionManagement, Guid>
{
    public int CompanyId { get; set; }
    public ESubscriptionStatus Status { get; set; }
    public ESessionStatus SessionStatus { get; set; }
    public string SubscriptionId { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public string InvoiceId { get; set; } = string.Empty;

    public override Expression<Func<SubscriptionManagement, bool>> CreateExpression(Expression<Func<SubscriptionManagement, bool>>? expression = null)
    {
        // Se a expressão for nula, começa com uma expressão que é sempre verdadeira
        if (expression == null) expression = (user => true);
        
        if (!IncludeDelete) expression = expression.AndAlso(x => !x.Deleted);

        if (!string.IsNullOrEmpty(SubscriptionId)) expression = expression.AndAlso(x => x.SubscriptionId == SubscriptionId);

        if (!string.IsNullOrEmpty(SessionId)) expression = expression.AndAlso(x => x.SessionId == SessionId);
        
        if (!string.IsNullOrEmpty(InvoiceId)) expression = expression.AndAlso(x => x.InvoiceId == InvoiceId);

        if (Status > ESubscriptionStatus.None) expression = expression.AndAlso(x => x.Status == Status);

        if (SessionStatus > ESessionStatus.None) expression = expression.AndAlso(x => x.SessionStatus == SessionStatus);

        if (CompanyId > 0) expression = expression.AndAlso(x => x.CompanyId == CompanyId);

        return expression;
    }
}
