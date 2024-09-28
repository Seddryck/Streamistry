using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streamistry;
public interface IDualRoute<TMain, TAlternate> : IDualRoute, IChainablePort<TMain>, IProcessablePipe<TAlternate>
{
    new OutputPort<TMain> Main { get; }
    new OutputPort<TAlternate> Alternate { get; }
}

public interface IDualRoute : IChainablePipe
{
    IChainablePort Main { get; }
    IChainablePort Alternate { get; }
}
