# Structured calculations

StructuredValues.FromClr converts object-valued string-keyed dictionaries and IDictionary (including reader
rows), ordered arrays/collections, and public readable CLR properties to Expressif
RecordValue/arrays. DBNull becomes null; names retain case and order. Scalars and
existing Expressif values pass through. Cycles reject; indexers are excluded. This
is representation conversion, not field-expression evaluation.

UsingExpressif().Pluck<T>(".customer.name") delegates nested/missing-field behavior
to Expressif. UsingExpressif().Split<T>(expression) evaluates once per input: null
and scalar results produce one element, empty collections zero, collections one
element per member. Strings and records are scalar. A null to nonnullable output
rejects; select a nullable output to preserve it. Delegate Split remains available.

StructureAdapters.ZipExpression overloads cover two through six inputs. Queues,
capacity, overflow, pairing and completion remain the generated Streamistry zip;
after pairing, Expressif evaluates a record with item1 through itemN fields. Example:
StructureAdapters.ZipExpression<string, string, string>(a, b,
    ".item1 | append(^.item2)"). This does not perform keyed zip.

ExpressionLookup<T> prepares string-keyed lookup records incrementally before
input delivery. Duplicate keys are last-wins (matching Translator); unmatched keys
evaluate the fallback expression on the original key; a matched null stays null.
Values delivered before preparation throw. Dictionary-source restart clears lookup
state. The legacy generic Translator retains arbitrary CLR-key/delegate support.
For asynchronous preparation, await the dictionary source and dependent execution
explicitly; synchronous auto-start follows the existing Translator contract.
