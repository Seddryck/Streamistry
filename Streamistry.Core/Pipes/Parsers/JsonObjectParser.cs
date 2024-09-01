using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Streamistry.Pipes.Parsers;
public class JsonObjectParser : StringParser<JsonObject>
{
    public JsonObjectParser(IChainablePipe<string> upstream)
        : base(upstream, new ParserDelegate<string, JsonObject>(TryParse))
    { }

    private static bool TryParse(string? text, out JsonObject? value)
    {
        value = null;
        if (text == null)
            return true;
        try
        {
            value = (JsonObject)JsonNode.Parse(text)!;
            return true;
        }
        catch
        { return false; }
    }
}
