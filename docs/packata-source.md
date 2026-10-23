# Contract-driven input

Streamistry.Packata pins Packata.ResourceReaders 0.63.4. EndpointSource accepts its
IDataEndpointReaderFactory, canonical DataEndpoint and optional DataSchema or
DataAsset. It awaits OpenAsync with the execution token and owns the returned reader.
Packata owns partial-open cleanup and the reader's backing streams/commands/
connections. Source cleanup disposes the returned reader after any exit.

No format/provider dispatch is added: ResourceReaderFactory uses its existing
PocketCsvReader, DubUrl, ExcelDataReader and Parquet integrations. Integration tests
exercise delimited, SQLite, spreadsheet and Parquet endpoints against the published
package and compare direct CSV rows with contract-driven rows. Binary fixtures are
copied from Packata's Apache-2.0 test resources.

Load format-specific documents before pipeline construction, in the application:

```csharp
using Packata.DataPackage;
using Packata.DataPackage.Mapping;
using Packata.ResourceReaders;
using Streamistry.Packata;
using Streamistry.Expressif;
var mapped = new DataPackageFactory().LoadFromFile("datapackage.json")
    .ToCanonicalContract().ReportDiagnostics(diagnostic => Report(diagnostic));
var contract = mapped.RequireValue();
var asset = contract.Assets.Single(a => a.Name == "people");
var endpoint = contract.Endpoints.Single(e => e.Id == asset.EndpointBindings.Single().EndpointId);
var source = new EndpointSource(new ResourceReaderFactory(), endpoint, asset);
var expression = Expressif.Expression.Create(".name | trim | upper");
var map = new Mapper<IReadOnlyDictionary<string, object>, string>(source,
    row => ExpressionAdapters.Adapt<string>(expression.Evaluate(StructuredValues.FromClr(row))));
var sink = new Streamistry.Pipes.Sinks.MemorySink<string>(map);
await new Pipeline(source).StartAsync(cancellationToken);
```

Choose/rescope canonical endpoints in Packata or application configuration before
constructing the source, including physical tables or asset paths. Streamistry
does not reinterpret contract bindings. Mapping diagnostics belong to Packata and
can be reported before RequireValue; opening/reading exceptions propagate through
the awaited execution task. Metadata/schema validation does not automatically
validate required fields, constraints or every row. Schema projection/type handling
is exactly what the selected Packata reader provides. Apply explicit row checks
when needed. Core has no dependency on Packata or document-loading packages.

Packata's older DubUrl dependency brings a vulnerable Scriban minimum. This adapter
pins Scriban 7.5.1 to resolve that dependency while retaining Packata's verified API.
PocketCsvReader is pinned to 2.53.0 as in the direct adapter, so standalone Packata
installation uses the same delimited-reader version exercised by comparison tests.
