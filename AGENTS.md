## Issues

Issue titles MUST be descriptive natural-language titles.

Do NOT use Conventional Commit syntax for issue titles.

Prefer:

```text
Map should preserve null values
Add pairwise function
Reduce allocations when mapping arrays
```

Avoid:

```text
fix: preserve null values when mapping arrays
feat: add pairwise function
perf: reduce allocations when mapping arrays
```

Every issue MUST have exactly one change-type label:

* `bug` for a defect
* `new-feature` for new functionality
* `enhancement` for an improvement or refactoring of existing functionality

The label is determined by the nature of the issue.

## Branches and worktrees

Every coding task MUST be performed in its own dedicated worktree and task branch.

For a new task:

1. Fetch the latest remote state.
2. Create the task branch from the latest `origin/main`.
3. Create or use a dedicated worktree for that branch.

Branch names MUST describe the nature of the change:

* `fix/<name>` for bug fixes and issues that correct defective behavior (i.e. when issue is labelled `bug`)
* `feat/<name>` for new functionality (i.e. when issue is labelled `new-feature` or `enhancement`)
* `refactor/<name>` for internal restructuring without changing intended behavior
* `perf/<name>` for performance improvements
* `docs/<name>` for documentation-only changes (i.e. when issue is labelled `docs`)
* `test/<name>` for test-only changes
* `chore/<name>` for maintenance work that does not fit another category

When asked to **fix a bug, defect, regression, or issue describing incorrect behavior**, the branch MUST use the `fix/` prefix.

For example:

```text
fix/predicate-parameter-parsing
fix/coalesce-record-input
feat/max-by
refactor/function-builders
```

Do NOT use tooling-specific prefixes such as:

```text
codex/
chatgpt/
```

The fact that Codex, ChatGPT, or another agent performs the work MUST NOT affect the branch name. Branch names describe the change, not the tool performing it.

The name of the branch should not reference the id of the issue but should be in plain text based on the name of the issue (i.e `fix/predicate-parameter-parsing`).

## Skills

Repository-specific workflows are defined under `.github/skills/`.

When a task matches an existing skill, read and follow that skill before making changes.

## Pull requests

For every completed implementation:

1. Push the task branch.
2. Create a GitHub pull request targeting `main`.
3. Use a Conventional Commit-style PR title.
4. Include a concise description of the change.
5. Include the relevant tests or validation performed.
6. Link the pull request to the corresponding issue when one exists (use wording `close`).

Do NOT use `bug`, `new-feature`, or `enhancement` labels on the pull request unless explicitly requested.

## Completion criteria

A coding task is complete only when:

* implementation was performed in the task's dedicated worktree;
* for a new task, the branch was created from the latest `origin/main`;
* the branch name follows the repository branch naming rules;
* the solution build successfully;
* the relevant tests have been run;
* all intended changes are committed;
* commit messages follow Conventional Commits;
* the branch has been pushed;
* a pull request targeting `main` has been created;
* the PR title follows Conventional Commits;
* the corresponding issue has the appropriate `bug`, `new-feature`, or `enhancement` label;
* the pull request is linked to the issue when one exists;
* the worktree is clean.
