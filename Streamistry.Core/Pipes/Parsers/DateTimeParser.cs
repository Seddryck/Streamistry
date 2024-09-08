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
    public DateTimeParser(IFormatProvider? formatProvider = null)
        : this(formatProvider, null)
    { }

    public DateTimeParser(IChainablePipe<string> upstream, IFormatProvider? formatProvider = null)
        : this(formatProvider, upstream)
    { }

    protected DateTimeParser(IFormatProvider? formatProvider = null, IChainablePipe<string>? upstream = null)
        : base((string? x, out DateTime y) => TryParse(x, formatProvider, out y), upstream)
    { }

    private static bool TryParse(string? text, IFormatProvider? formatProvider, out DateTime value)
        => DateTime.TryParse(text, formatProvider ?? CultureInfo.InvariantCulture, out value);
}
