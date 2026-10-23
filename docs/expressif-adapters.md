# Expressif adapters

Install Streamistry.Expressif to opt into Expressif 2.93.1. Core has no Expressif
reference. Delegate mappers, filters, splitters and aggregators remain supported.

```csharp
using Streamistry.Expressif;
using Streamistry.Fluent;
var pipeline = new PipelineBuilder()
    .Source(new[] { " a ", "b" })
    .UsingExpressif().Map<string>("trim | upper")
    .Sink().InMemory().Build();
pipeline.Start();
```

For existing ports, use source.MapExpression<string, string>("trim | upper") or
source.FilterExpression("is-not-null"). Fluent UsingExpressif().Filter accepts
text or IPredicate objects; Map accepts text or IExpression. Text is parsed/bound
when constructing the adapter, never per record. Use Map(...).Safe() in the fluent
API (or SafeMapper with ExpressionAdapters.Map) to route evaluation exceptions and
incompatible CLR output types to Alternate. Ordinary mapping propagates exceptions.
False filters are intentionally omitted; invalid nonboolean predicates throw.

Output adaptation only checks CLR assignability. It never coerces: ask Expressif
for the required semantic conversion or use an explicit CLR interoperability map.
Null is a successful result for nullable/reference ports; null to a nonnullable
value port is an InvalidCastException, as is decimal to int. This keeps valid null
separate from rejection/failure. Alternate preserves the original input.

Each adapter binds its own immutable EvaluationContext via WithContext. Runtime
variables are copied by Expressif; mutable objects/delegates supplied as variable
values remain caller-owned and must not be shared unsafely. No global mutable
context or per-record parser is introduced. Rows initially cross as BCL dictionaries
and arrays; structured representation adapters are documented separately.

