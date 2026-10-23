using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Streamistry.Pipes.Mappers;
[Obsolete("Use Map for CLR property delegates or Streamistry.Expressif UsingExpressif().Pluck for field expressions.")]
public class Plucker<TInput, TOutput> : Mapper<TInput, TOutput>
{
    public Plucker(Expression<Func<TInput, TOutput>> expr)
        : this(expr, null)
    { }

    public Plucker(IChainablePort<TInput> upstream, Expression<Func<TInput, TOutput>> expr)
        : this(expr, upstream)
    { }

    protected Plucker(Expression<Func<TInput, TOutput>> expr, IChainablePort<TInput>? upstream = null)
        : base(CreateAccessor(expr), upstream)
    { }

    private static Func<TInput, TOutput> CreateAccessor(Expression<Func<TInput, TOutput>> expression)
    {
        if (expression.Body is not MemberExpression)
            throw new ArgumentException($"Expression '{expression}' refers to a method, not a property.");
        var accessor = expression.Compile();
        return input => input is null ? default! : accessor(input);
    }

    public static object GetNestedPropertyValue(object target, LambdaExpression lambda)
    {
        if (lambda.Body is not MemberExpression)
            throw new ArgumentException($"Expression '{lambda}' refers to a method, not a property.");
        return lambda.Compile().DynamicInvoke(target)!;
    }
}
