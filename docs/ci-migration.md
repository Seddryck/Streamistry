# GitHub Actions CI migration

The first stage moves build, test, coverage enforcement, and package validation to
GitHub Actions. AppVeyor remains the only NuGet publisher until the publishing
cutover in [#309](https://github.com/Seddryck/Streamistry/issues/309).
Implementation of this first stage is tracked in
[#308](https://github.com/Seddryck/Streamistry/issues/308).

## Build and distribution decisions

| Dimension | Repository evidence | Decision |
|---|---|---|
| Deliverables | Core, JSON, and RabbitMQ are managed libraries | Preserve `Streamistry`, `Streamistry.Json`, and `Streamistry.RabbitMQ` |
| Target frameworks | Shared props previously targeted .NET 8/9; .NET 10 requested | Build and test `net8.0`, `net9.0`, and `net10.0` |
| Compiler | Source generator references Roslyn 4.12; project uses C# 12 | Use the .NET 10 SDK for all target frameworks, with stable SDK selection in `global.json` |
| Generator | Roslyn analyzer project overrides the shared TFM | Preserve `netstandard2.0`; build it through project references |
| Unit-test OS | Existing AppVeyor runner is Windows; console assertions contain CRLF | Run core and JSON tests on Windows for each TFM |
| Integration-test OS | RabbitMQ tests require localhost AMQP; existing Docker image uses RabbitMQ 3 | Run all RabbitMQ tests on Linux for each TFM with a health-checked RabbitMQ 3.13 service |
| Linux filesystem risk | Globbing tests create/read files relative to the working directory | Run a focused Linux globbing smoke test on .NET 10 |
| RIDs and libc | No first-party native interop, RID-specific deliverables, executable, installer, or AOT publishing found | No distribution RID, architecture, or glibc/musl matrix |
| macOS and ARM | No existing native runner or distribution contract established | Do not add these runner/architecture dimensions in this migration |
| Package identity | AppVeyor explicitly packs three public projects | Produce one package and one symbol package per identity, containing all three TFMs |
| Versions | Packata uses GitVersion 6 GitHubFlow; Streamistry historically calculates untagged patches with Mainline | Pin GitVersion 6.6.0 in a local tool manifest for both providers; calculate once and pass its SemVer to every build/test/package job |
| Generated prerequisites | Scriban templates are embedded; generator is a project analyzer; icons/readme are tracked | Generate source through normal compilation; no external image-generation job |
| Publishing | AppVeyor publishes main builds and creates GitHub releases only for patch zero | Preserve that publisher during stage one; Actions has no publish/tag/release steps |

The libraries are portable managed deliverables. The Windows test choice reflects
current test assumptions, not a Windows-only package support claim. The focused
Linux tests do not establish a complete Linux unit-test guarantee. Dependencies
include managed JSON, file-globbing, and RabbitMQ clients plus build-time
Fody/Roslyn tooling; no runtime-specific package payload is introduced here.

## Workflow

`.github/workflows/ci.yml` runs on pull requests targeting main, pushes to main,
and manual dispatch. It uses read-only repository permissions, pinned action
revisions, and no publishing secrets. A new run cancels an obsolete run for the
same ref. Dependabot tracks GitHub Actions updates.

1. Version: fetch complete Git history and tags, restore the pinned local tool,
   calculate the version, verify it belongs to the checked-out commit, and test
   the version rules against isolated Git histories.
2. Windows matrix: restore the solution, build core/JSON test projects for the
   selected TFM, then run each without rebuilding or restoring. Keep the existing
   10% line-coverage threshold for each tested library. Store TRX and OpenCover
   reports in a uniquely named artifact for each framework/OS.
3. Linux matrix: wait for RabbitMQ server and port health, build its test project,
   and run the complete RabbitMQ test category on every TFM. Run the filesystem
   smoke test in the .NET 10 job. Service setup is unconditional; an unavailable
   broker fails CI instead of silently skipping integration tests.
4. After every test job succeeds, build the complete Release solution, pack the
   three public projects once with `--no-build --no-restore`, validate them, and
   retain the packages. The packaging build includes all TFMs and the generator.
5. `CI result` succeeds only if the version, unit, integration, and package jobs all
   succeeded. It fails on failed, cancelled, or skipped prerequisites.

`eng/Test-Packages.ps1` requires exactly three `.nupkg` files and three `.snupkg`
files, verifies their identities and common version, checks every TFM assembly
and PDB, and verifies the packaged readme and icon. CI artifacts are for inspection
and are never uploaded to a package feed. Codecov delivery remains on AppVeyor
until stage two; Actions retains coverage without requiring a Codecov credential.

There is no dependency lock file today, so restore uses the existing NuGet
resolution behavior. This migration does not claim locked restores. A dependency
cache is deliberately omitted until its key and lock-file policy are established.

## Versioning policy

`GitVersion.yml` uses Packata's GitVersion 6/GitHubFlow syntax, `v` tags,
conventional-commit major/minor matching, explicit `+semver` overrides, feature
`beta.<branch>` labels, and release `rc.<branch>` labels. The old `tag` and
`is-mainline` properties become `label` and `is-main-branch`. Like Packata and
PocketCsvReader, commit-message incrementing is enabled instead of restricted
to merge messages, and the catch-all no-bump expression is removed.

Streamistry retains the `Mainline` strategy because it publishes patch builds
without tagging each one. Copying Packata's strategy defaults verbatim calculates
`0.39.1` from Streamistry's current history, below AppVeyor's `0.39.52` baseline.
The adapted config stays above that baseline; enabling all commit messages can
advance the patch number compared with the old config. No tags or published
versions are rewritten.

The default is a patch for a nonempty commit message. Feature messages select a
minor bump, breaking messages select a major bump, and explicit `+semver: none`
or `skip` prevents a bump. Main's fallback is `None` and the patch-message regex
supplies the default patch: this avoids GitVersion 6's branch fallback overriding
an explicit no-bump commit. Maintenance branches (`fix`, `refactor`, `perf`,
`docs`, `test`, and `chore`) branch from main, matching AGENTS.md, so they do not
accidentally inherit a feature branch's minor increment.

`eng/Test-Versioning.ps1` verifies tagged releases, consecutive untagged patches,
feature/breaking increments, explicit no-bump/minor overrides, and feature/fix/
release-candidate prereleases in disposable fixture repositories under ignored
`artifacts/`. Each commit has a distinct timestamp so traversal does not depend
on same-second ordering. The fixtures never change real branches or tags.

## Local validation

Install the .NET 10 SDK and the .NET 8/9 runtimes to exercise all target frameworks.
The workflow installs all three SDKs to ensure those runtimes are available;
`global.json` selects the .NET 10 compiler.

```powershell
actionlint .github/workflows/ci.yml
dotnet tool restore
./eng/Test-Versioning.ps1
./eng/Invoke-CI.ps1 -Task Unit -Framework net8.0
./eng/Invoke-CI.ps1 -Task Unit -Framework net9.0
./eng/Invoke-CI.ps1 -Task Unit -Framework net10.0
./eng/Invoke-CI.ps1 -Task Package
```

With a local RabbitMQ 3 broker listening on localhost:5672, run `Integration`
for each framework. Run `LinuxSmoke` on Linux. The scripts fail immediately when
a dotnet command fails; results and packages are written beneath ignored
`artifacts/`. Use an empty package output directory when starting a new version;
the validator rejects stale or unexpected packages.

## Publishing and required-check cutover

The existing required checks are `continuous-integration/appveyor/pr` and
`GitGuardian Security Checks`. This PR does not change repository protection or
disable the AppVeyor service. Its temporary SDK bootstrap keeps the existing
publisher able to compile the added .NET 10 target without machine-wide SDK
installation changes.

Complete [#309](https://github.com/Seddryck/Streamistry/issues/309) after hosted CI
is green:

1. Verify every target framework, RabbitMQ integration, package, and symbol
   artifact in pull-request and main runs. Confirm `CI result` appears as a stable
   status check. Require it while preserving GitGuardian and the existing check
   until cutover is ready.
2. Reuse the complete-history GitVersion calculation introduced in stage one.
   Refresh tags and verify the version/commit again before reserving a release
   identity; use the same version for the entire artifact set.
3. Configure NuGet trusted publishing or an approved scoped credential. Add
   explicit release gating, artifact download/validation, tag reservation, and
   publication of already-tested artifacts without rebuilding. Define a release
   event that preserves the desired cadence; PR and ordinary branch CI must not
   publish. Preserve the patch-zero rule for GitHub release pages unless changed
   explicitly. Migrate changelog labels and Codecov delivery as part of parity.
4. Disable AppVeyor publication before enabling the Actions publisher. Never
   permit both publishers to produce the same package version.
5. After verifying the new publisher and check, remove AppVeyor's required check,
   configuration, logger dependencies, and obsolete hooks. Preserve useful local
   broker scripts. Update this guide and any remaining AppVeyor badges.

Repository protection, external AppVeyor settings, and NuGet publishing setup are
maintainer cutover actions. No credentials are copied from AppVeyor into this
repository or workflow. If cutover fails, disable the new publisher before
restoring the old one; never overwrite or reuse an already published version.
