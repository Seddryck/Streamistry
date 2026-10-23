# Build and distribution decisions

The shared eng/Projects.psd1 inventory is authoritative for packages, test projects
and frameworks. Every optional package joins immutable build/package provenance.
No additional publication trigger or permission is introduced. The still-connected
AppVeyor project now has an explicit SDK-based build/test-only compatibility check;
deployment is disabled and GitHub Actions remains the sole publisher.

| Dimension | Repository evidence | Decision | Confidence |
|---|---|---|---|
| Deliverables | Eight managed library packages; generator used only at build time | One multi-target NuGet and symbol package per library | High |
| Target frameworks | Directory.Build.props supports net8.0/net9.0/net10.0 | Compile/test every framework; one common source version | High |
| Native tests | CSV/file paths and provider packages can differ by OS; SQLite native assets | Existing Windows tests cover all unit/adapter projects; extend focused Linux smoke to database/Packata fixtures | High; hosted Linux run still required |
| Distribution RIDs/architectures/libc | No application executable; optional provider NuGet dependencies resolve native assets | No RID-specific Streamistry packages or new platform promises | High |
| Package identities | Core package is Streamistry; integrations use project names | Eight explicit identities plus matching symbols | High |
| Generated prerequisites | Scriban source generator compiles into Core | Generate during normal build; no extra artifact generator/tool | High |
| Version/provenance | Existing CI calculates one version, seals tested outputs and verifies packages | Preserve flow; include every optional library/test output and derive expected counts from inventory | High |

Linux smoke exercises file behavior and SQLite/Packata binary-format reads without
restoring or recompiling the shared build. Native-provider testing does not imply
all transitive database providers or macOS are tested. No musl or installer claim
is added. Publication still uses the previously tested packages and existing
release guards; this change performs local validation only, with no NuGet release.

The Windows job runs all three unit-test frameworks in one invocation, verifying
the complete artifact manifest before and after the group. Coverage restoration
is still verified for every sealed output, while four redundant full-tree hash
scans are avoided. Its 45-minute ceiling accommodates first-run restore, native
provider dependencies and archive creation; the former 20-minute job timed out
after its second framework. AppVeyor installs the .NET 10 SDK, builds all target
frameworks and runs the same six unit/adapter projects on net10.0. The full runtime
matrix and RabbitMQ/Linux validation remain in GitHub Actions.
