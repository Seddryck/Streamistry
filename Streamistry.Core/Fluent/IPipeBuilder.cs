using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streamistry.Fluent;
public interface IPipeBuilder<T> : IBuilder<IChainablePort<T>>
{ }

public interface IBuilder<T>
{
    T BuildPipeElement();
    T OnBuildPipeElement();
}
