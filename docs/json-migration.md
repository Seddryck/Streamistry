# JSON, files and HTTP

Compose FilePathSource -> FileTextReader -> ObjectParser/ArrayParser for files.
Compose URL Mapper -> HttpTextResponder -> ObjectParser/ArrayParser for HTTP.
Expressif URL calculation can precede a CLR Uri construction map. These stages
separate discovery, text I/O, root parsing and value calculation. FileTextReader
closes each file before emission. HttpTextResponder borrows HttpClient and disposes
requests, responses and content streams on success/rejection/failure; non-2xx routes
the Uri to Alternate, transport/parse exceptions remain exceptions. Its pipe is
synchronous; ReadAsync is an awaited helper for explicit async composition.

Legacy GlobbingSource<T> and RestResponder remain compatibility conveniences.
RestResponder keeps its HTTP 200-only and root-cast behavior but now closes responses.

Streamistry.Json.Expressif is an optional bridge referencing both optional packages.
JsonExpressionMapper converts JsonNode to Expressif, calculates, then serializes a
fresh JsonNode. ObjectParser/ArrayParser keep BCL parsing, malformed-text rejection
and strict root shape; JSON null rejects as a wrong root.

| Concern | JsonNode compatibility | Expressif bridge |
|---|---|---|
| Record/object | JsonObject, ordered properties | RecordValue, ordered fields |
| Arrays | JsonArray | object[] |
| Scalars/roots | Object/ArrayParser restrict root | Bridge accepts any JsonNode root |
| Missing vs null | JSONPath match information available | Missing field and explicit null evaluate to null |
| Numeric values | JSON number representation | Decimal; out-of-range JSON numbers reject explicitly |
| Paths | Arbitrary JsonPath.Net | Expressif field expressions only |
| Mutation/identity | Existing mappers may mutate/reuse nodes | Fresh output nodes; input untouched |

Object/array parsing and serialization remain representation adapters. Supported
field selection, value mapping, array calculation and record updates use Expressif
expressions, e.g. .customer | put(name := .name | upper). Split expression results
with Streamistry's splitter. Positional pairing for appends remains Streamistry zip;
then an Expressif record expression calculates the combined output. Existing
PathPlucker, ArraySplitter, ValueMapper and ObjectPropertyAppender stay available
for arbitrary JSONPath or mutation/identity compatibility. The upstream gap is
https://github.com/Seddryck/Expressif/issues/1341; no equivalent coverage is assumed.
