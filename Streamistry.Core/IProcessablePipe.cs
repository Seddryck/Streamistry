using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streamistry;
public interface IProcessablePipe<T>
{
    void Emit(T? obj);
}
