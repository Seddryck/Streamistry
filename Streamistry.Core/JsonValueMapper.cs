using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Streamistry;
public class JsonValueMapper<TInput> : Mapper<TInput, JsonValue>
{
    public JsonValueMapper(IChainablePort<TInput> upstream, Func<TInput?, string>? toString = null)
        : base(upstream, value => toString is null ? JsonValue.Create(value) : JsonValue.Create(toString.Invoke(value)))
    {
    }
}
