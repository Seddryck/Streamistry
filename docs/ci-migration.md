# CI, build reuse, and NuGet publishing

The GitHub Actions workflow replaces AppVeyor. It builds the solution once,
tests the resulting binaries, packages those same binaries, and publishes only
on an explicit main-branch release dispatch.

## Decisions and reference workflows

| Dimension | Evidence and decision |
|---|---|
| Frameworks/compiler | Build net8.0/net9.0/net10.0 with SDK 10.0.400+; retain the netstandard2.0 generator |
| Unit tests | Existing console assertions use CRLF: run all core/JSON tests on Windows for each framework |
| Linux tests | Run RabbitMQ 4.4.0 tests on all frameworks and a .NET 10 filesystem smoke test using the Windows-built portable test assemblies |
| Distribution | Three portable managed libraries: Streamistry, Streamistry.Json, Streamistry.RabbitMQ; no RID, architecture, AOT, or libc package matrix |
| Build reuse | Packata keeps build/tests/pack together; DubUrl archives bin/obj for later pack jobs; PocketCsvReader separates test/package artifacts |
| Selected design | One complete solution build, same-run bin/obj archive, Linux execution of portable test DLLs, later Windows pack with no compilation |
| Cache | Cache only NuGet dependencies by OS and project/tool fingerprints; no cross-run compiler-output cache |
| Version | Preserve Packata's exact GitVersion.yml and local GitVersion 6.8.2 manifest; calculate once with full history |
| Packages | One nupkg and snupkg per identity, all three TFMs, common version; validate assemblies, symbols, icon, readme, and tested-byte provenance |
| Release | Explicit workflow_dispatch publish=true on main; protected release environment and two activation acknowledgements |
| Tags/release pages | Reserve every published stable version so patch progression advances; GitHub release pages remain limited to patch zero |
| Authentication | NuGet trusted publishing and Codecov OIDC; no long-lived publishing key in repository secrets |
| Dependencies | Checkout 7.0.1, upload-artifact 7.0.2, download-artifact 8.0.2, github-script 9.0.0, cache 6.1.0; action commits pinned |

The action updates include those proposed in #311, #312, and #313. setup-dotnet
6.0.0 remains current; NuGet/login 1.2.0 and codecov-action 7.1.1 are also pinned.

## Artifact flow

1. Calculate and validate GitVersion; run version-policy and release-guard tests.
2. Restore once and build the entire Release solution once on Windows.
3. Record SHA-256 hashes of the build outputs. Run the 142 core and 15 JSON tests
   for each framework with no build/restore and the existing 10% line threshold.
   Verify coverage instrumentation restored the original DLL/PDB bytes.
4. Archive bin/obj and the commit/version/hash manifest. Upload this immutable
   same-run artifact named with the tested commit SHA.
5. Three Linux jobs download that artifact and verify its hashes. Run RabbitMQ
   tests via VSTest directly against the compiled DLLs; no compilation or NuGet
   restore occurs. The .NET 10 job also runs the filesystem smoke test.
6. After all tests pass, a Windows job downloads the same archive. Refresh
   machine-specific restore assets, then pack with --no-build --no-restore.
   Packing must preserve every compiled output hash.
7. Verify every packaged DLL/PDB equals the recorded tested build asset. Retain
   the six packages and a provenance manifest binding package/asset hashes,
   version, and commit.
8. Codecov separately downloads the retained OpenCover reports and authenticates
   with OIDC. Fork PRs retain reports but skip authenticated upload.
9. CI result requires all validation jobs, including coverage where applicable.
   The publish job downloads only the validated package artifact from this run,
   rechecks its provenance, and never builds or packs.

Artifacts provide exact run provenance. NuGet cache hits only avoid dependency
downloads; a cache miss cannot change which binaries are tested or published.
Original test suites are still executed on every run, including cache hits.

## Publication gate and first activation

Publishing is off until configured. PRs, pushes (including main), and ordinary
manual validation dispatches do not publish. An explicit publish dispatch on a
feature branch also cannot publish.

Before the first release, the maintainer must complete these concrete settings:

1. Disable the Streamistry project/deployment in AppVeyor and cancel queued old
   publishing builds. Removing appveyor.yml is not evidence that external UI
   deployment settings and already-queued builds are disabled. Set repository
   variable APPVEYOR_PUBLISHING_DISABLED=true only after checking this.
2. Configure a NuGet trusted-publishing policy for profile Seddryck, repository
   owner Seddryck, repository Streamistry, workflow file ci.yml, environment
   release. Scope it to all three package identities. The workflow requests its
   short-lived key only after every validation and release reservation succeeds.
   See the official [NuGet trusted publishing guide](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing).
3. Configure the GitHub release environment for main only, with any desired
   reviewer policy. Set NUGET_PUBLISHING_ENABLED=true after NuGet trust and the
   external AppVeyor shutdown are verified.
4. Establish the release baseline. The exact Packata config can calculate 0.39.1
   from old untagged Streamistry history, below existing published patches.
   Identify/tag the verified commit for the last actual published version, or
   establish a maintainer-reviewed new release tag/version above the feed.
   Do not invent a last-release tag on an unrelated commit. CI does not rewrite
   history, add next-version overrides, or silently compensate for this.
5. Require CI result and preserve GitGuardian Security Checks in branch
   protection. The legacy AppVeyor required check was already absent when this
   implementation started.
6. After merging the reviewed PR and a green main run, dispatch CI on main with
   publish=true. Do not run this step just to test configuration.

The workflow refuses publication unless both activation variables are exactly
true, appveyor.yml is absent, the checkout is the tested current main commit,
and GitVersion still calculates the built version after refreshing tags.
Every package's NuGet index is checked for a stable version regression.
Existing versions can only be retried when their reserved tag points at the
same commit. Network/authentication failures stop the release.

Reserve the stable release tag only after package validation, before publishing.
A conflicting tag is never moved or overwritten. Reruns can reuse the same
commit's tag. All stable versions receive tags; only patch-zero versions receive
GitHub release pages with generated notes and the Announcements discussion.
Conventional PR titles continue to populate release-note labels through gh CLI.

## Local validation

Use the installed .NET 10 SDK and .NET 8/9 runtimes. A local broker is required
only for Integration. The scripts do not provision or start Docker.

Commands in order:

- dotnet tool restore
- ./eng/Test-Versioning.ps1
- ./eng/Test-ReleaseGuards.ps1
- ./eng/Invoke-CI.ps1 -Task Build
- ./eng/Invoke-CI.ps1 -Task LinuxSmoke -Framework net10.0
- ./eng/Invoke-CI.ps1 -Task Unit -Framework net8.0 (repeat for net9.0/net10.0)
- ./eng/Build-Artifact.ps1 -Mode Save -Version 0.0.0-ci.local -Commit (git rev-parse HEAD)
- ./eng/Build-Artifact.ps1 -Mode Restore -Version 0.0.0-ci.local -Commit (git rev-parse HEAD)
- ./eng/Invoke-CI.ps1 -Task Package
- ./eng/Test-ArtifactGuards.ps1
- actionlint .github/workflows/ci.yml .github/workflows/combine-prs.yml

Do not rerun Build between tests and Package. Package accepts only a matching
build manifest and does not compile missing assets. Use an empty package output
directory when changing versions. Artifact guard tests corrupt temporary copies
or restore original bytes in finally blocks and do not publish or reserve tags.

## Rollback and remaining operational checks

To stop publication, set NUGET_PUBLISHING_ENABLED=false. Keep CI enabled.
If a release is partially published, keep its reserved tag, inspect the package
set, and retry only the same commit/version. Never delete/repoint a published
tag or overwrite a NuGet version. A later rollback to AppVeyor requires disabling
the Actions publisher first; never run two publishers simultaneously.

NuGet trust, external AppVeyor project settings, the release baseline, and the
first real release require maintainer/account access. They are documented
activation steps, not actions performed by a pull-request CI run. A first release
must verify all three packages/symbols in NuGet and the patch-zero release rule.
No actual NuGet publication is performed as part of implementation testing.

The copied Packata configuration intentionally retains inherited GitHubFlow
behavior: untagged patches can share a version; the inherited release branch
currently produces a beta label despite Packata's additional rc entry.
eng/Test-Versioning.ps1 characterizes this behavior without changing the config.
