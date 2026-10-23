# Direct delimited input

Streamistry.PocketCsvReader uses PocketCsvReader 2.53.0 and references only Core.
Pass a CsvProfile factory to expose upstream delimiter, header, quote/escape,
encoding/resource and parser-error configuration. Profiles are created per run.
No CSV parser, DataTable materialization or whole-stream buffering is added.

Caller-supplied streams are borrowed by default (leaveOpen=true); false transfers
ownership. File inputs and async stream factories transfer ownership. All owned
streams/readers close on completion, cancellation, early Stop and setup/read failure.
Borrowed streams retain their cursor and are not automatically rewound. Factory
inputs open fresh streams for restart. Async factories receive cancellation;
PocketCsvReader IDataReader reads remain synchronous with boundary token checks.

Rows follow the shared dictionary contract: ordered headers, stable snapshots,
DBNull-to-null and duplicate-name rejection. Headerless names, empty/null sequences,
encoding and malformed-row behavior follow the configured PocketCsvReader profile.
By default parser errors fail the run. Recovery is possible only through upstream
BadDataPolicy options that explicitly support the error; no arbitrary per-row
recovery is promised.

```csharp
using PocketCsvReader;
using Streamistry.PocketCsvReader;
using Streamistry.Expressif;
using Streamistry.Pipes.Sinks;

var source = new CsvSource("input.csv", () => new CsvProfile(',', '"', "\n", true));
var expression = Expressif.Expression.Create(".name | trim | upper");
var map = new Mapper<IReadOnlyDictionary<string, object>, string>(source,
    row => ExpressionAdapters.Adapt<string>(expression.Evaluate(StructuredValues.FromClr(row))));
var sink = new MemorySink<string>(map);
await new Pipeline(source).StartAsync(cancellationToken);
// sink.State contains transformed values; no DataTable was allocated.
```

