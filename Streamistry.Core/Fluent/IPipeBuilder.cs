using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streamistry.Fluent;
internal interface IPipeBuilder<T> : IBuilder<IChainablePort<T>>
{ }

internal interface IBuilder<T>
{
    T BuildPort();
    T OnBuildPort();
}
