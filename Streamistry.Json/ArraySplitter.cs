using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Streamistry.Json;
internal class ArraySplitter : Splitter<JsonArray, JsonObject>
{
    public ArraySplitter(IChainablePipe<JsonArray> upstream)
        : base(upstream, x => [..Split(x)])
    { }

    private static IEnumerable<JsonObject?>? Split(JsonArray? array)
    {
        if (array is null)
            yield return null;
        else
            foreach (var item in array!)
                yield return (JsonObject?)item;
    }
}
