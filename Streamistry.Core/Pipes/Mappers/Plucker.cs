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
        var propInfo = GetPropertyInfo(lambda);
        return (TOutput?)propInfo.GetValue(input, null);
    }

    private static PropertyInfo GetPropertyInfo(Expression<Func<TInput, TOutput?>> lambda)
    {
        if (lambda.Body is not MemberExpression member)
            throw new ArgumentException($"Expression '{lambda}' refers to a method, not a property.");

        if (member.Member is not PropertyInfo propInfo)
            throw new ArgumentException($"Expression '{lambda}' refers to a field, not a property.");

        var type = typeof(TInput);
        if (propInfo.ReflectedType != null && type != propInfo.ReflectedType && !type.IsSubclassOf(propInfo.ReflectedType))
            throw new ArgumentException($"Expression '{lambda}' refers to a property that is not a property from type {type}.");

        return propInfo;
    }
}
