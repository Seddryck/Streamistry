using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streamistry;
public interface IDualRoute<TMain, TAlternate> : IChainablePort<TMain>
{
    OutputPort<TMain> Main { get; }
    OutputPort<TAlternate> Alternate { get; }
}
