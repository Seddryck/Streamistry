using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streamistry.Observability.Meters;
public interface IMeter
{
    void Append(object value, DateTime timestamp);
    void Trigger();
}
