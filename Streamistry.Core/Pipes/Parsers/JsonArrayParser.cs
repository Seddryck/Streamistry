using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Streamistry.Pipes.Parsers;
public class JsonArrayParser : StringParser<JsonArray>
{
    public JsonArrayParser(IChainablePipe<string> upstream)
        : base(upstream, TryParse)
    { }

    private static bool TryParse(string? text, out JsonArray? value)
    {
        value = null;
        if (text == null)
            return true;

        try
        {
            value = (JsonArray)JsonNode.Parse(text)!;
            return true;
        }
        catch
        { return false; }
    }
}
