using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Streamistry.Pipes.Mappers;
public class Plucker<TInput, TOutput> : Mapper<TInput, TOutput>
{
    public Plucker(IChainablePipe<TInput> upstream, Expression<Func<TInput, TOutput?>> expr)
        : base(upstream, x => RetrieveProperty(x, expr))
    { }

    private static TOutput? RetrieveProperty(TInput? input, Expression<Func<TInput, TOutput?>> lambda)
    {
        if (input is null)
            return default;

        return (TOutput?)GetNestedPropertyValue(input, lambda);
    }

    public static object? GetNestedPropertyValue(object target, LambdaExpression lambda)
    {
        MemberExpression? expr;
        if (lambda.Body is not MemberExpression)
            throw new ArgumentException($"Expression '{lambda}' refers to a method, not a property.");

        if (lambda.Body is UnaryExpression unaryExpression)
            expr = unaryExpression.Operand as MemberExpression ?? throw new InvalidOperationException();
        else
            expr = lambda.Body as MemberExpression;

        var members = new Stack<MemberExpression>();

        while (expr != null)
        {
            members.Push(expr);
            expr = expr.Expression as MemberExpression;
        }

        // Now we evaluate them from root to leaf
        object? item = target;
        while (members.Count > 0)
        {
            var memberExpression = members.Pop();
            var propertyInfo = (memberExpression.Member as PropertyInfo)
                ?? throw new ArgumentException("Member is not a property");
            item = propertyInfo.GetValue(item);
        }
        return item;
    }
}
