using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Streamistry.Pipes.Mappers;

namespace Streamistry.Fluent;
public class PluckerBuilder<TInput, TOutput> : PipeElementBuilder<TInput, TOutput>
{
    protected Expression<Func<TInput, TOutput?>> Expr { get; set; }

    public PluckerBuilder(IPipeBuilder<TInput> upstream, Expression<Func<TInput, TOutput?>> expr)
        : base(upstream)
        => (Expr) = (expr);

    public override IChainablePort<TOutput> OnBuildPipeElement()
        => new Plucker<TInput, TOutput>(
                Upstream.BuildPipeElement()
                , Expr ?? throw new InvalidOperationException()
            );
}
