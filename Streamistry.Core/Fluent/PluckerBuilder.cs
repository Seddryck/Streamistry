using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Streamistry.Pipes.Mappers;

namespace Streamistry.Fluent;
internal class PluckerBuilder<TInput, TOutput> : PipeElementBuilder<TInput, TOutput>
{
    protected Expression<Func<TInput, TOutput?>> Expr { get; set; }

    public PluckerBuilder(IPipeBuilder<TInput> upstream, Expression<Func<TInput, TOutput?>> expr)
        : base(upstream)
        => (Expr) = (expr);

    public override IChainablePort<TOutput> OnBuildPort()
        => new Plucker<TInput, TOutput>(
                Upstream.BuildPort()
                , Expr ?? throw new InvalidOperationException()
            );
}
