using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Streamistry.Fluent;

namespace Streamistry.Json.Fluent;

public static class BasePipeBuilderExtension
{
    public static ValueMapperBuilder<TInput> AsJsonValue<TInput>(this BasePipeBuilder<TInput> builder, Func<TInput, string>? toString = null)
       => new(builder, toString);

    public static PathPluckerBuilder<TOutput> Pluck<TOutput>(this BasePipeBuilder<JsonObject> builder, string path)
       => new(builder, path);

    public static ArrayPathPluckerBuilder<TOutput> Pluck<TOutput>(this BasePipeBuilder<JsonArray> builder, string path)
       => new(builder, path);

    public static ArraySplitterBuilder Split(this BasePipeBuilder<JsonArray> builder)
      => new(builder);
}

public static class ParserBuilderExtension
{
    public static SpecializedParserBuilder<TInput, JsonObject> AsJsonObject<TInput>(this ParserBuilder<TInput> builder)
        => new(builder.Upstream, typeof(ObjectParser), null);

    public static SpecializedParserBuilder<TInput, JsonArray> AsJsonArray<TInput>(this ParserBuilder<TInput> builder)
        => new(builder.Upstream, typeof(ArrayParser), null);
}
