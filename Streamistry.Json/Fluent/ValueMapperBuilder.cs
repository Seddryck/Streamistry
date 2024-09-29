using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Streamistry.Fluent;

namespace Streamistry.Json.Fluent;
public class ValueMapperBuilder<TInput> : MapperBuilder<TInput, JsonValue>
{
    public ValueMapperBuilder(IPipeBuilder<TInput> upstream, Func<TInput, string>? toString)
        : base(upstream, value => toString is null ? JsonValue.Create(value)! : JsonValue.Create(toString.Invoke(value)))
    { }
}
