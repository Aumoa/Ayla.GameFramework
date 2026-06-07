---
name: ayla-unity-pr-review
description: Repository-local pull request and branch-diff review guidance for Ayla Unity plugin projects. Use when Codex reviews PRs, branch diffs, CI failures, Unity integration changes, asmdef changes, architecture changes, class responsibilities, duplication, tests, or merge readiness in Ayla.* repositories.
---

# Ayla Unity PR Review

## Overview

Ayla Unity plugin reviews should protect correctness, Unity integration health,
and the repository's intended architecture without turning style preference into
noise.

## Review Priorities

- Lead with actionable findings ordered by severity.
- Ground findings in concrete source locations, logs, commands, diffs, or Unity test results.
- Separate blockers from residual risks and optional follow-up ideas.
- Connect Unity-specific findings back to asmdef boundaries, Editor/Runtime separation, serialization, lifecycle behavior, package placement, or host-project validation.

## Unity Plugin Checks

- Verify runtime assemblies do not depend on `UnityEditor` or editor-only code.
- Verify C# scripts are placed under `Runtime/Script/` for runtime code or `Editor/Script/` for editor-only code.
- Check asmdef references, optional package dependencies, platform constraints, and define constraints when touched.
- Check Unity serialization behavior for renamed fields, private serialized fields, `[SerializeReference]`, asset GUID assumptions, and domain reload behavior.
- Review MonoBehaviour, ScriptableObject, async, cancellation, pooling, and disposal code for lifecycle edge cases.
- Ensure tests or validation cover the host Unity project integration when behavior depends on Unity runtime or editor behavior.

## Object-Oriented Design

- Prefer object-oriented designs with clear responsibilities, encapsulation, and extensibility unless the touched code is genuinely performance-critical.
- Accept performance-oriented departures from object-oriented design only when they do not significantly harm readability, maintainability, or local reasoning.
- Use Microsoft's recommended C# design guidelines and object-oriented patterns from major engines as references, adapted to Ayla's Unity plugin conventions.

## Class Responsibility

- Check whether each meaningful feature is owned by a clear class or collaborator.
- Flag classes that combine unrelated responsibilities when that coupling makes behavior harder to extend, test, or review.
- Prefer cohesive feature units over scattered special cases.
- Do not request extra splitting when the existing responsibility boundary is already understandable and further separation would mostly add indirection.

## Duplication

- Flag duplicated code when it represents the same feature, policy, decision, Unity integration behavior, or platform behavior and merging it would reduce bug risk or future maintenance.
- Prefer one cohesive implementation for repeated behavior that must evolve together.
- Avoid demanding abstractions for small incidental duplication when the abstraction would be noisier than the repeated code.

## Merge Readiness

- Before approving or merging into protected shared branches, check whether the change includes CI, branch trigger, permission, environment, generated file, or validation-only configuration changes that should not land.
- Treat unremoved temporary CI or validation settings as a merge blocker when they would affect protected branches.
- Clearly state whether the change is safe to merge, needs fixes first, or needs CI/runtime validation before judgment.

## Review Comments

- Write external PR review comments in English unless the user asks otherwise.
- Explain the interpretation and recommendation to the user in Korean when the surrounding conversation is Korean.
