using System.Linq.Expressions;

namespace VirtualQueueApi.Utils.Extensions;

public static class ExpressionExtensions
{
    // Método auxiliar para combinar duas expressões usando AndAlso
    public static Expression<Func<T, bool>> AndAlso<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
    {
        var parameter = Expression.Parameter(typeof(T));

        var combined = new ReplaceParameterVisitor(expr1.Parameters[0], parameter).Visit(expr1.Body);
        var body = Expression.AndAlso(combined, new ReplaceParameterVisitor(expr2.Parameters[0], parameter).Visit(expr2.Body));

        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }
}

// Classe auxiliar para substituir os parâmetros nas expressões
internal class ReplaceParameterVisitor : ExpressionVisitor
{
    private readonly ParameterExpression _oldParameter;
    private readonly ParameterExpression _newParameter;

    public ReplaceParameterVisitor(ParameterExpression oldParameter, ParameterExpression newParameter)
    {
        _oldParameter = oldParameter;
        _newParameter = newParameter;
    }

    protected override Expression VisitParameter(ParameterExpression node)
    {
        return node == _oldParameter ? _newParameter : base.VisitParameter(node);
    }
}
