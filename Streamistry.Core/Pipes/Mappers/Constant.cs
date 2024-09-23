using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streamistry.Pipes.Mappers;
public class Constant<TInput, TOutput> : Mapper<TInput, TOutput>
{
    protected Constant(TOutput value, IChainablePort<TInput>? upstream)
    : base((x) => value, upstream)
    { }

    public Constant(IChainablePort<TInput> upstream, TOutput value)
    : this(value, upstream)
    { }

    public Constant(TOutput value)
    : this(value, null)
    { }
}
