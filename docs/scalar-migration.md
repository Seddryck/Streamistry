# Scalar compatibility

Opt in with builder.UsingExpressif().Constant(value), .IsNull(), .IsNotNull(),
.Cast<T>(), .Date() or .DateTime(). Safe() routes failed conversions to Alternate.
Constant is an Expressif runtime-variable expression, not interpolated language text.
Supported conversion targets are int, decimal, bool, string, DateOnly, DateTime and
TimeOnly (including nullable value types); all calculation is in Expressif.Caster.

| Behavior | Existing Core APIs | Expressif adapters |
|---|---|---|
| Null/DBNull conversion | CLR cast maps null to default; DBNull may fail | Valid null for nullable/reference output, rejected for nonnullable |
| Failed conversion | Caster throws; SafeCaster/Parser reject | InvalidCastException; Safe() rejects original input |
| Integer fractional input | Convert.ChangeType may round | Exact conversion required; fractional values reject |
| Numeric overflow | CLR conversion fails | Expressif TryCast fails; never wraps |
| Numeric types | Requested CLR target | int or decimal; no implicit output coercion |
| Dates | Invariant by default; custom IFormatProvider supported | Expressif invariant DateOnly/DateTime rules; DateTime adjusts offsets to UTC |
| Null predicates | CLR null only | Expressif is-null also recognizes DBNull and its null sentinel/text |
| Constant | CLR delegate | Bound Expressif variable, same output identity |

Compatibility decision: keep existing Core APIs through the current release series.
No automatic redirect may silently change rounding, culture, sentinel treatment,
or CLR operator dispatch. Custom IFormatProvider parsing and user-defined implicit/
explicit operators remain thin CLR interoperability adapters through existing Cast,
SafeCaster and Parse APIs (or delegates). Long/short/float/double output conversions
remain CLR adapters too. These CLR-specific policies are not a second expression
language and do not need upstream language additions. No unavailable Expressif
conversion capability is assumed. Migration is explicit; removals require a future
major release and a published deprecation window.
