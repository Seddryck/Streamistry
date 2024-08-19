using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace Streamistry.Pipes.Aggregators;
public class Count<T> : Aggregator<T, int, int>
{
    public Count(IChainablePipe<T> upstream)
        : base(upstream
            , (x, y) => y is null ? x : ++x, (x) => x)
    { }
}

public class Count<T, U> : Aggregator<T, U, U>
    where U : INumber<U>
{
    public Count(IChainablePipe<T> upstream)
        : base(upstream
            , (x, y) => y is null ? x : (x is null ? U.One : ++x), (x) => x)
    { }
}
