# Pipeline execution and optional integrations

Streamistry owns delivery, ports, synchronization, lifecycle, bounded queues,
failure routing, observability and connector resource ownership. Expressif owns
supported value calculations. Optional connectors adapt external readers into the
same ordered dictionary/array/scalar boundary. Delegate customization remains a
first-class API and Core has no reference to any optional integration.

| Package | References | Responsibility |
|---|---|---|
| Streamistry (Core project) | BCL, observability, globbing | Execution and delegate APIs |
| Streamistry.Expressif | Core, Expressif 2.93.1 | Expressions, predicates, structured values, running accumulators |
| Streamistry.Json | Core, JsonPath.Net, System.Text.Json | JsonNode/JSONPath representation and compatibility |
| Streamistry.Json.Expressif | JSON, Expressif adapter | JSON/record conversion and expression calculations |
| Streamistry.PocketCsvReader | Core, PocketCsvReader 2.53.0 | Direct delimited readers |
| Streamistry.Packata | Core, Packata.ResourceReaders 0.63.4 | Canonical endpoint readers; upstream format/storage resolution |
| Streamistry.DubUrl | Core, DubUrl 0.29.34 | Direct parameterized database reads/writes |
| Streamistry.RabbitMQ | Core, RabbitMQ.Client | Existing messaging connector |

The optional source packages do not require the Expressif adapter. Install both
when combining inputs and expressions. Packata already depends on CSV and DubUrl;
its adapter does not depend on the direct Streamistry CSV/database packages.

## Migration decisions

Use UsingExpressif() to opt into fluent Map/Filter/Constant/Cast/Date/DateTime,
Pluck/Split, and Count/Sum/Min/Max. Expressions bind once at construction. Strict
CLR output adaptation never implements a second coercion engine. Safe mapping
routes failure while preserving a successful nullable result.

Invariant culture, exact integer conversion, decimal aggregate output, count-null
inclusion, date offset normalization and Expressif null-sentinel behavior are
explicit opt-in changes. Existing CLR converters, IFormatProvider parsers, numeric
aggregates and CLR-null filters remain available for their established policies.
See [scalar compatibility](scalar-migration.md) and
[running aggregates](accumulator-migration.md) before switching.

Rows are stable shallow snapshots in column order. DBNull becomes null; duplicate
dictionary names reject. Caller readers/streams are borrowed unless ownership is
explicitly transferred. Factories own partial setup cleanup until returning a
lease. See [reader ownership](reader-contract.md), [CSV](csv-source.md),
[Packata](packata-source.md) and [databases](database-connectors.md).

Await StartAsync for asynchronous resource factories. Plain IDataReader and sink
delegates remain synchronous. Cancellation aborts and cleans up; Stop completes a
partial stream. Concurrent runs reject; sequential factory/enumerable restarts
open fresh resources. Pipeline execution is sequential and does not add scheduling
or worker threads. See [async lifecycle](async-lifecycle.md).

Buffer flush is distinct from completion. Union/zip complete after all inputs;
zip drops unmatched tails and preserves nulls. Default zip queues hold at most
1024 items per input with explicit throw/drop policies. There is no blocking
overflow policy. Existing completion-triggered dependency cycles must be replaced
with predecessor-source dependencies. See [completion](completion-contract.md).

JSON bridges produce fresh nodes and use decimal numeric representation. Existing
arbitrary JSONPath and mutation APIs remain until equivalent behavior is verified.
See the [JSON compatibility matrix](json-migration.md).

## Audit of retained calculations

| Existing calculation/API | Decision |
|---|---|
| Mapper, SafeMapper, Filter, Splitter delegates | Retain user-defined customization and Streamistry routing/emission |
| Constant | Deprecate redundant convenience implementation; optional expression-variable replacement available |
| Plucker | Deprecate in favor of Expressif field expressions or CLR Map; compatibility implementation uses compiled CLR property delegates |
| Caster/SafeCaster | Retain CLR interoperability: operator dispatch, arbitrary CLR targets and historical numeric/culture policies |
| DateParser/DateTimeParser | Retain explicit IFormatProvider interoperability; default Expressif date conversions available |
| FilterNull/FilterNotNull | Retain strict CLR-null/type interoperability; opt-in Expressif predicates own language null semantics |
| Translator | Retain generic CLR keys/delegate fallback; string-key expression lookup delegates supported field/fallback calculations |
| Generated combinators | Retain pairing/queues; expression-result overloads cover arities 2–6 |
| Core Count/Sum/Min/Max | Retain generic CLR numeric compatibility/callbacks; optional running accumulators delegate to Expressif |
| Core Average/Median | Retain pending [Expressif #1340](https://github.com/Seddryck/Expressif/issues/1340); no full-stream fold substitution |
| ObjectParser/ArrayParser, JSON serialization | Retain representation/root-shape adapters |
| PathPlucker/ValueMapper/ArraySplitter/ObjectPropertyAppender | Retain JSONPath/mutation interoperability pending [Expressif #1341](https://github.com/Seddryck/Expressif/issues/1341); bridge delegates supported calculations |
| GlobbingSource/RestResponder | Retain compatibility composition; prefer separate discovery, text I/O and parsing stages |
| Sources, sinks, buffers, union, ports and observability | Streamistry runtime/connector responsibilities, not value-language calculations |

## Deprecation policy

Constant and Plucker receive non-error Obsolete notices. Plucker validates and
compiles its CLR property expression at construction, so invalid shapes now fail
during construction rather than on the first row. Their APIs and behavior
remain available throughout the current major release line. Removal requires a
future major release, release notes, and at least one full minor release containing
the deprecation notice and replacement guidance. Do not automatically redirect
existing APIs where semantics differ. Other retained interoperability APIs and
delegates are supported; no blanket deprecation is intended. Missing upstream
capabilities remain explicitly deferred, with compatibility paths preserved.

## Examples

The [direct CSV example](csv-source.md) binds an expression once and streams into
MemorySink.State. The [contract-driven example](packata-source.md) loads and maps
metadata before constructing the pipeline. The corresponding integration tests
execute both paths against published dependencies, without DataTable materialization.
