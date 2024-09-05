using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Streamistry.Json;
public class ObjectParser : StringParser<JsonObject>
{
    public ObjectParser(IChainablePipe<string> upstream)
        : base(upstream, new ParserDelegate<string, JsonObject>(TryParse))
    { }


    private static bool TryParse(string? text, [NotNullWhen(true)] out JsonObject? value)
    {
        value = null;
        if (text == null)
            return false;
        try
        {
            value = (JsonObject)JsonNode.Parse(text)!;
            return true;
        }
        catch
        { return false; }
    }
}
