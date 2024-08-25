using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streamistry.Observability.Thresholds;
public interface IThreshold
{
    bool Check(int count, DateTime timestamp);
}
