using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Streamistry.Observability.Measurements;

namespace Streamistry.Observability;
public class ConsolePublisher
{
    public void Publish(IMeasurement measurement)
    {
        Console.WriteLine($"[{DateTime.UtcNow}] '{measurement}'");
    }
}
