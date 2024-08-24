using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streamistry.Observability;
public interface ITracer
{
    IDisposable? StartActiveSpan(string spanName);
}
