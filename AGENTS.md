# Repository Instructions

Policy version: 2026-06-08

This repository is a plugin project intended to be attached to a host Unity
project.

## Local Instructions

If `AGENTS.local.md` exists at the repository root, read it and follow it as
private, machine-local context. Treat it as optional for other contributors and
do not assume it exists outside the current working tree.

Project-local Codex skills belong under `.codex/skills/`. Skill directories
whose names start with `local-` are private local skills and must not be
committed.

Do not copy or synchronize `AGENTS.local.md` or `.codex/skills/local-*` to other
repositories.

## Ayla Plugin Family

Ayla plugin repositories are expected to live as sibling directories whose names
start with `Ayla.` under the same parent directory, for example:

```text
<Parent>/Ayla.Core
<Parent>/Ayla.RemoteDebug
```

The parent directory itself is not fixed and may differ between machines or host
Unity projects.

When editing shared `AGENTS.md`, shared `.gitignore` rules for agent-local files,
or shared `.codex/skills/*` entries, inspect sibling `Ayla.*` repositories under
the same parent directory. If equivalent shared instructions or shared skills are
missing or outdated, apply the same update there too, as long as filesystem and
user permissions allow it.

Treat each sibling `Ayla.*` directory as a separate Git boundary. Check its Git
status before changing it. Do not revert unrelated changes. Do not commit in a
sibling repository unless the user has explicitly allowed commits for that
repository. Never push any repository without explicit user approval.

When shared instructions conflict between Ayla repositories, prefer the rule with
the newest explicit policy version. If no version makes the newer rule clear,
prefer the rule that is more valid for the current project. If that is still
ambiguous, ask the user how to resolve the conflict.

## Unity Host Project

This plugin needs a host Unity project for normal Unity-based testing,
validation, and integration work.

When adding or changing functionality, use a test Unity project that covers the
behavior being changed, and place this plugin under that project's hierarchy.
The recommended location is:

```text
<UnityProject>/Assets/Plugins/<PluginDirectory>
```
