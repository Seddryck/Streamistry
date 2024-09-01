using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Streamistry.Pipes.Parsers;
internal class DateTimeParser : StringParser<DateTime>
{
    public DateTimeParser(IChainablePipe<string> upstream, IFormatProvider? formatProvider = null)
        : base(upstream, (string? x, out DateTime y) => TryParse(x, formatProvider, out y))
    { }

    private static bool TryParse(string? text, IFormatProvider? formatProvider, out DateTime value)
        => DateTime.TryParse(text, formatProvider ?? CultureInfo.InvariantCulture, out value);
}
