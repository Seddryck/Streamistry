using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Streamistry.Json;
public class ArrayParser : StringParser<JsonArray>
{
    public ArrayParser(IChainablePipe<string> upstream)
        : base(upstream, TryParse)
    { }

    private static bool TryParse(string? text, [NotNullWhen(true)] out JsonArray? value)
    {
        value = null;
        if (text == null)
            return false;

        try
        {
            value = (JsonArray)JsonNode.Parse(text)!;
            return true;
        }
        catch
        { return false; }
    }
}
