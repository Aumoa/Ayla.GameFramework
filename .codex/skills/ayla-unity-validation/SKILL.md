---
name: ayla-unity-validation
description: Repository-local validation workflow for Ayla Unity plugin projects. Use when Codex needs to build, test, validate, or diagnose Unity host-project integration, asmdef compilation, EditMode or PlayMode tests, package placement, or plugin changes in Ayla.* repositories.
---

# Ayla Unity Validation

## Core Rules

- Treat the repository as a Unity plugin that usually needs a host Unity project for real validation.
- Read `AGENTS.local.md` first when it exists; use it for the local host Unity project path and plugin placement.
- If no local host path is documented, infer it only when the plugin is clearly under `<UnityProject>/Assets/Plugins/<PluginDirectory>`. Otherwise report that Unity validation needs a host project path.
- Keep local-only files such as `AGENTS.local.md` and `.codex/skills/local-*` out of shared changes.

## Choosing Checks

- Use focused validation that matches the touched subsystem.
- For runtime C# changes, verify Unity compilation through the host project when possible.
- For editor code, custom inspectors, importers, or property drawers, prefer EditMode tests or an editor compilation check.
- For scene, lifecycle, pooling, timing, async, or MonoBehaviour behavior, prefer PlayMode tests when practical.
- For shared `AGENTS.md`, `.gitignore`, or shared `.codex/skills/*` changes, inspect sibling `Ayla.*` repositories under the same parent and keep equivalent shared instructions synchronized.

## Unity Host Workflow

1. Identify the host Unity project from `AGENTS.local.md` or from the plugin path.
2. Confirm the plugin remains under the host project hierarchy, preferably under `Assets/Plugins/`.
3. Inspect asmdef files and Editor/Runtime folder boundaries before running expensive checks.
4. Run the narrowest Unity test or compilation check that covers the change.
5. If Unity CLI, Unity Hub, licenses, packages, or the host project are unavailable, state exactly what could not be run and perform a stricter source review.

## Reporting

- Report the host project path used for validation.
- Report the exact Unity command or manual check that was run, when applicable.
- Distinguish passed checks, skipped checks, and checks that could not be run.
- Call out residual risk when validation is source-only.

## Shared-State Safety

- Treat `dev`, `master`, `main`, release branches, and any branch used directly by others as shared.
- Do not push, force-push, delete remote branches, modify hosted services, or change shared state without explicit user approval.
- Do not commit in sibling repositories unless the user explicitly allows commits for those repositories.
