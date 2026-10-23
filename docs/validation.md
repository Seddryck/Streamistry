# Validation for the pipeline/integration migration

Validated locally on Windows on 2026-10-23. The repository remains pinned to SDK
10.0.400; the local .NET 10 SDK installation is incomplete. Validation used installed
SDK 11.0.100-rc.2.26504.105 with .NET 8/9/10 target/runtime packs. Earlier checks used
a temporary restored global.json override; final checks invoked that SDK's CLI
assembly directly, leaving global.json unchanged. Hosted CI uses the repository SDK.

Focused tests ran before broader tests for each implementation. The final Release
matrix passed all six affected suites on every supported framework:

| Test project | net8.0 | net9.0 | net10.0 |
|---|---:|---:|---:|
| Streamistry.Testing | 168 | 168 | 168 |
| Streamistry.Json.Testing | 22 | 22 | 22 |
| Streamistry.Expressif.Testing | 20 | 20 | 20 |
| Streamistry.PocketCsvReader.Testing | 7 | 7 | 7 |
| Streamistry.Packata.Testing | 9 | 9 | 9 |
| Streamistry.DubUrl.Testing | 9 | 9 | 9 |
| Total | 235 | 235 | 235 |

All 705 final matrix tests passed, with no skipped tests. This includes the existing
core/JSON regression suites and the new ownership, cancellation, completion,
overflow, expression, scalar, structured, aggregate, file/HTTP, CSV, canonical
contract and direct database tests. Individual implementation PRs record their
earlier focused filters and counts.

Additional validation passed:

- Solution Release build with ContinuousIntegrationBuild and version 0.0.0-ci.local.
- Updated eng/Invoke-CI.ps1 Unit -AllFrameworks task: all 705 tests, coverage thresholds,
  and restoration of instrumented bytes verified against the build manifest.
- Updated Package task: eight NuGet packages plus eight symbol packages, every
  net8.0/net9.0/net10.0 DLL/PDB, identities, version, readmes and icons verified.
- Package provenance checks compared all 48 packaged DLL/PDB assets with tested
  build hashes; Core's package dependency guard rejected optional dependency drift.
- eng/Test-ArtifactGuards.ps1: six rejection scenarios for incorrect identities,
  modified build/package bytes and packages differing from tested outputs; original
  bytes restored and verified afterward.
- eng/Test-ReleaseGuards.ps1: publication boundaries, version/tag and label rules.
- PowerShell syntax parsing for eng scripts, project inventory import and git diff --check.

Direct database tests used isolated SQLite files, including parameter literals,
nulls, commit/manual/autocommit policies, disposal and rollback on command failure,
upstream failure and cancellation. Packata tests exercised real CSV, SQLite, XLSX
and Parquet reads and real Data Package-to-canonical mapping. HTTP tests used an
in-process handler and checked response disposal.

Not run locally: RabbitMQ service tests, the LinuxSmoke task on a native Linux
runner, macOS tests, external database providers, live remote HTTP tests, NuGet
publication, and a separate rerun of version-calculation tests (version calculation
was unchanged). CI coverage instrumentation was exercised on all three frameworks; all
three framework test matrices also ran without coverage first. Existing JSON nullability
warnings and intentional compatibility Obsolete notices are non-error warnings.

Average/median migration and arbitrary JSONPath replacement remain explicitly
deferred to verified upstream Expressif capabilities, tracked by issues 1340 and
1341. Existing implementations remain supported as documented in the migration guide.
