# Upgrade plan: .NET 10 migration (Bottom-Up)

This document is the planning-only migration specification to upgrade the repository to .NET 10 (`net10.0`).
It follows a Bottom-Up (dependency-first) strategy: upgrade leaf libraries first, then projects that depend on them, and finish with applications (WPF tools).

Table of contents
- 1. Executive summary
- 2. Migration strategy
- 3. Dependency analysis & tiers
- 4. Per-tier specifications
- 5. Project-by-project stubs (details)
- 6. Package update reference & security notes
- 7. Breaking changes catalog (expected)
- 8. Testing & validation strategy
- 9. Risk management & mitigations
- 10. Source control & branch strategy
- 11. Success criteria
- 12. Next steps & outstanding information

1. Executive summary
---------------------
- Scope: All projects in the solution will be targeted for upgrade to `net10.0` (WPF projects to `net10.0-windows`) per the "attempt all" option. Projects that cannot reasonably be migrated (incompatible platform APIs or third-party constraints) will be flagged and options presented.
- Strategy: Bottom-Up dependency-first migration to limit ripple effects and enable early stabilization of shared libraries.
- Key constraints: solution contains projects currently targeting older .NET Framework versions (net4.8 / net4.0 / net3.5). These may require API replacements, additional NuGet shims, or to remain on .NET Framework.
- Major deliverable: `plan.md` updated with tiers, per-tier upgrade specification, validation criteria, and package upgrade reference. This plan is for execution by a separate execution agent or developers.

2. Migration strategy
---------------------
- Selected approach: Bottom-Up (dependency-first). Rationale: repository contains many libraries shared across tools; upgrading libraries first reduces multi-targeting and isolates breaking changes early.
- Parallelism: Within a tier, projects that are independent may be upgraded in parallel. Tiers themselves must be completed in order.
- Phases (per-tier flow): Preparation → Update project files & packages (batched per tier) → Fix compilation issues (batched) → Unit & integration testing → Stabilize → Proceed to next tier.

Bottom-Up execution gating and rules
- Strict ordering: Do not start upgrading Tier N+1 until Tier N is marked complete.
- Tier completion criteria (must be satisfied before proceeding):
  1. All projects in tier build successfully with `TargetFramework` set to `net10.0` (or `net10.0-windows` where applicable).
  2. Unit tests in the tier pass.
  3. Integration compilation: all immediate consumers (projects that reference the tier) compile against updated artifacts.
  4. No critical security vulnerabilities remain in packages updated in this tier (or documented exceptions exist).

Batch operation guidance for executors
- Project file updates: apply SDK-style conversion for all projects in a tier as a single batch change.
- Package updates: update all packages in the tier in one operation (single commit per-tier for package updates). Group upgrades by shared packages where possible.
- Compilation fixes: group similar fixes (API replacements, namespace changes) into a single follow-up commit per tier.
- Testing: run unit tests for all projects in the tier and then run integration compilation with consumers.

Gating example:
- After completing Tier 1 batch (conversion + package updates), run tier-level build and unit tests. If failures arise, address them in the tier branch. Only when all checks pass, merge Tier 1 changes and proceed to Tier 2.


3. Dependency analysis & tiers
-----------------------------
Source: topological order retrieved from solution (projects listed in build dependency order). The topological listing was used to assign dependency tiers. Exact project reference graphs should be validated before execution.

Topological order (leaf → application):
```
C:\...\projects\Gibbed.IO\Gibbed.IO.csproj
C:\...\projects\Gibbed.Frostbite3.Common\Gibbed.Frostbite3.Common.csproj
C:\...\projects\Gibbed.Frostbite3.Zstd\Gibbed.Frostbite3.Zstd.csproj
C:\...\projects\Gibbed.Frostbite3.VfsFormats\Gibbed.Frostbite3.VfsFormats.csproj
C:\...\projects\Gibbed.Frostbite3.ResourceFormats\Gibbed.Frostbite3.ResourceFormats.csproj
C:\...\projects\Gibbed.MassEffectAndromeda.GameInfo\Gibbed.MassEffectAndromeda.GameInfo.csproj
C:\...\projects\Gibbed.MassEffectAndromeda.FileFormats\Gibbed.MassEffectAndromeda.FileFormats.csproj
C:\...\projects\NDesk.Options\NDesk.Options.csproj
C:\...\projects\Gibbed.Frostbite3.Unbundling\Gibbed.Frostbite3.Unbundling.csproj
C:\...\projects\Gibbed.Frostbite3.Dynamic\Gibbed.Frostbite3.Dynamic.csproj
C:\...\projects\Gibbed.MassEffectAndromeda.SaveFormats\Gibbed.MassEffectAndromeda.SaveFormats.csproj
C:\...\projects\Gibbed.MassEffectAndromeda.Dumping\Gibbed.MassEffectAndromeda.Dumping.csproj
C:\...\projects\Gibbed.MassEffectAndromeda.SaveEdit\Gibbed.MassEffectAndromeda.SaveEdit.csproj
C:\...\projects\Gibbed.MassEffectAndromeda.DumpPartyMembers\Gibbed.MassEffectAndromeda.DumpPartyMembers.csproj
C:\...\projects\Gibbed.PortableExecutable\Gibbed.PortableExecutable.csproj
C:\...\projects\Gibbed.MassEffectAndromeda.DumpPlotFlags\Gibbed.MassEffectAndromeda.DumpPlotFlags.csproj
C:\...\projects\Gibbed.MassEffectAndromeda.DumpItemTypes\Gibbed.MassEffectAndromeda.DumpItemTypes.csproj
C:\...\projects\Gibbed.Frostbite3.GeneratePartitionMap\Gibbed.Frostbite3.GeneratePartitionMap.csproj
C:\...\projects\Gibbed.Frostbite3.ConvertDbObject\Gibbed.Frostbite3.ConvertDbObject.csproj
C:\...\projects\Gibbed.Frostbite3.UnpackPartitions\Gibbed.Frostbite3.UnpackPartitions.csproj
C:\...\projects\Gibbed.Frostbite3.UnpackInitFS\Gibbed.Frostbite3.UnpackInitFS.csproj
C:\...\projects\Gibbed.Frostbite3.UnpackResources\Gibbed.Frostbite3.UnpackResources.csproj
```

Tier determination rules applied:
- Tier 1: projects at the start of the topological order (no internal project references) — leaf nodes.
- Tier N+1: projects that reference only projects from previous tiers.

Assigned tiers (initial grouping - validate with full project dependency query before execution):
- Tier 1 (Leaf nodes - low-dependency libraries)
  - `Gibbed.IO`
  - `Gibbed.Frostbite3.Common`
  - `Gibbed.Frostbite3.Zstd`
  - `Gibbed.Frostbite3.VfsFormats`
  - `Gibbed.Frostbite3.ResourceFormats`
  - `Gibbed.MassEffectAndromeda.GameInfo`
  - `Gibbed.MassEffectAndromeda.FileFormats`
  - `NDesk.Options`

- Tier 2 (Depends only on Tier 1)
  - `Gibbed.Frostbite3.Unbundling`
  - `Gibbed.Frostbite3.Dynamic`
  - `Gibbed.MassEffectAndromeda.SaveFormats`
  - `Gibbed.MassEffectAndromeda.Dumping`

- Tier 3 (Build tools / CLI utilities)
  - `Gibbed.MassEffectAndromeda.DumpPartyMembers`
  - `Gibbed.PortableExecutable`
  - `Gibbed.MassEffectAndromeda.DumpPlotFlags`
  - `Gibbed.MassEffectAndromeda.DumpItemTypes`

- Tier 4 (Higher-level Frostbite tools)
  - `Gibbed.Frostbite3.GeneratePartitionMap`
  - `Gibbed.Frostbite3.ConvertDbObject`
  - `Gibbed.Frostbite3.UnpackPartitions`
  - `Gibbed.Frostbite3.UnpackInitFS`
  - `Gibbed.Frostbite3.UnpackResources`

- Tier 5 (Applications / GUI)
  - `Gibbed.MassEffectAndromeda.SaveEdit` (WPF)

Notes:
- This tiering uses the topological order as a proxy for dependency depth. It must be validated by calling `upgrade_get_project_dependencies` for projects that appear ambiguous (before execution).
- Test projects (if any) should be migrated after the projects they test.

4. Per-tier specifications
-------------------------
For each tier the plan below specifies metadata, upgrade details, package updates (where known), expected breaking change classes, and validation requirements.

Tier 1 — Leaf libraries
- Projects: listed above
- Dependencies: external NuGet only (assumption)
- Estimated complexity: Low → Medium (depends on older TFMs and netstandard usage)
- Upgrade details:
  - Convert to SDK-style .csproj (single batched change for all Tier 1 projects).
  - Set `<TargetFramework>net10.0</TargetFramework>` unless project must remain `netstandard` (document why).
  - Update PackageReferences to versions compatible with net10.0.
- Breaking change exposure: Low for pure algorithmic libraries; Medium if P/Invoke or platform-specific APIs used.
- Validation requirements:
  - All Tier 1 projects build without errors/warnings (batched check).
  - Unit tests for these libraries pass.
  - Downstream Tier 2 compilation with upgraded Tier 1 artifacts succeeds (integration build check).

Tier 2 — Dependent libraries
- Projects: listed above
- Dependencies: Tier 1
- Estimated complexity: Medium
- Upgrade details:
  - Update project files (set TFMs to `net10.0`).
  - Update NuGet packages used by these projects (see Section 6). Include EF/serialization updates if present.
- Breaking changes: API surface used from Tier 1 libraries may have changed. Address obsolete APIs and behavior differences.
- Validation requirements:
  - Build Tier 2 projects against upgraded Tier 1 binaries.
  - Unit tests and any integration tests that exercise Tier 1/Tier 2 interactions.

Tier 3 — CLI tools / analysis utilities
- Projects: listed above
- Complexity: Low → Medium
- Upgrades:
  - Convert to SDK-style and target `net10.0`.
  - Replace legacy command-line parsing packages if incompatible (e.g., older NDesk.Options) with updated NuGet versions or maintain shims.
- Validation:
  - Tool smoke tests (basic input → output validation).

Tier 4 — Frostbite-specific tools
- Projects: listed above
- Complexity: Medium → High (format handling, platform code)
- Upgrades:
  - Ensure native interop / binary readers still function. May require `DllImport` review and runtime identifiers.
- Validation:
  - Functional validation with representative data files.

Tier 5 — Applications / GUI
- Projects: `SaveEdit` (WPF)
- Complexity: High
- Key changes:
  - Target `net10.0-windows` and set `<UseWPF>true</UseWPF>` and `<UseWindowsForms>` if needed.
  - Replace/upgrade UI-specific packages (Caliburn.Micro, Microsoft.Xaml.Behaviors.Wpf) to compatible versions or migrate patterns (MVVM toolkit, CommunityToolkit.Mvvm).
  - Verify XAML behavior, resource lookups, binding behaviors, and assembly loading.
- Validation:
  - Full manual/automated UI validation and smoke scenarios.

5. Project-by-project stubs
--------------------------------
Each project below is a stub that must be filled with exact package versions and specific code changes during execution. The execution agent or developer must run `upgrade_analyze_projects` to populate package lists and compatibility suggestions. Below are example stubs for the highest-priority items.

- `projects\Gibbed.Frostbite3.Common\Gibbed.Frostbite3.Common.csproj`
  - Current state: classic .NET Framework or mixed; used widely by other projects.
  - Target state: SDK-style targeting `net10.0`.
  - Migration steps (plan-level): convert csproj → update packages → run compile → fix API usages.
  - ⚠️ Requires validation: confirm no P/Invoke or Framework-only APIs.

- `projects\Gibbed.IO\Gibbed.IO.csproj`
  - Current state: `net4.0` (legacy). May require larger code changes.
  - Options: attempt direct migration to `net10.0` or keep on `net48` if migration cost is prohibitive. Because user selected "attempt all" this plan attempts migration but will flag blockers.

- `projects\Gibbed.MassEffectAndromeda.SaveEdit\Gibbed.MassEffectAndromeda.SaveEdit.csproj`
  - Current state: WPF on .NET Framework.
  - Target: `net10.0-windows`, `<UseWPF>true</UseWPF>`.
  - Special actions: update XAML namespaces if needed, update app manifest, resolve startup model differences.
  - Major risk: third-party WPF packages and behaviors.

Detailed per-project stubs
--------------------------
Below are concise, action-oriented stubs for every project discovered in the solution topological order. Each stub lists current-state assumptions (requires verification), target state, migration-level steps (planning only), and a risk rating to guide execution prioritization.

1) `projects\Gibbed.IO\Gibbed.IO.csproj`
  - Current state: reported targeting `net4.0` (legacy). Requires `upgrade_analyze_projects` to confirm package list and APIs in use.
  - Target state: attempt `net10.0` (if blocked, document reasons and consider keeping `net48`).
  - Migration steps (plan): convert to SDK-style; update package references; replace legacy APIs (e.g., WebClient/FileStream behaviors) as needed.
  - Risk: High (legacy TFM, potential use of Framework-only APIs).

2) `projects\Gibbed.Frostbite3.Common\Gibbed.Frostbite3.Common.csproj`
  - Current state: shared utility library used widely across solution.
  - Target state: `net10.0`.
  - Migration steps: SDK conversion; update packages; run compilation and fix API mismatches.
  - Risk: Medium (widespread impact if breaking changes introduced).

3) `projects\Gibbed.Frostbite3.Zstd\Gibbed.Frostbite3.Zstd.csproj`
  - Current state: likely contains zstd interop or managed wrapper.
  - Target state: `net10.0`.
  - Migration steps: convert csproj; validate native library loading and runtime identifiers; update NuGet wrapper if present.
  - Risk: Medium→High (native interop surface).

4) `projects\Gibbed.Frostbite3.VfsFormats\Gibbed.Frostbite3.VfsFormats.csproj`
  - Current state: file/format parsing library.
  - Target state: `net10.0`.
  - Migration steps: convert csproj; run unit tests; validate IO semantics.
  - Risk: Medium.

5) `projects\Gibbed.Frostbite3.ResourceFormats\Gibbed.Frostbite3.ResourceFormats.csproj`
  - Current state: resource format parsers.
  - Target state: `net10.0`.
  - Migration steps: convert csproj; test with sample assets.
  - Risk: Medium.

6) `projects\Gibbed.MassEffectAndromeda.GameInfo\Gibbed.MassEffectAndromeda.GameInfo.csproj`
  - Current state: game metadata helpers.
  - Target state: `net10.0`.
  - Migration steps: convert csproj; update packages; run unit tests.
  - Risk: Low→Medium.

7) `projects\Gibbed.MassEffectAndromeda.FileFormats\Gibbed.MassEffectAndromeda.FileFormats.csproj`
  - Current state: file format types and readers.
  - Target state: `net10.0`.
  - Migration steps: convert csproj; validate binary readers and endianness behaviors.
  - Risk: Medium.

8) `projects\NDesk.Options\NDesk.Options.csproj`
  - Current state: older command-line parsing library.
  - Target state: `net10.0`.
  - Migration steps: convert csproj; consider replacing with `System.CommandLine` or upgrade NDesk.Options to a compatible fork if available.
  - Risk: Medium (package compatibility).

9) `projects\Gibbed.Frostbite3.Unbundling\Gibbed.Frostbite3.Unbundling.csproj`
  - Current state: tool for unbundling archives.
  - Target state: `net10.0`.
  - Migration steps: convert csproj; validate binary IO; update dependencies.
  - Risk: Medium.

10) `projects\Gibbed.Frostbite3.Dynamic\Gibbed.Frostbite3.Dynamic.csproj`
  - Current state: dynamic format support.
  - Target state: `net10.0`.
  - Migration steps: convert csproj; run integration tests with consumers.
  - Risk: Medium.

11) `projects\Gibbed.MassEffectAndromeda.SaveFormats\Gibbed.MassEffectAndromeda.SaveFormats.csproj`
  - Current state: save file formats.
  - Target state: `net10.0`.
  - Migration steps: convert csproj; run sample read/write tests.
  - Risk: Medium.

12) `projects\Gibbed.MassEffectAndromeda.Dumping\Gibbed.MassEffectAndromeda.Dumping.csproj`
  - Current state: dump utilities.
  - Target state: `net10.0`.
  - Migration steps: convert csproj; run smoke tests.
  - Risk: Low→Medium.

13) `projects\Gibbed.MassEffectAndromeda.SaveEdit\Gibbed.MassEffectAndromeda.SaveEdit.csproj` (WPF)
  - Current state: WPF GUI targeting full .NET Framework (likely net48).
  - Target state: `net10.0-windows` with `<UseWPF>true</UseWPF>`.
  - Migration steps: convert csproj using `Microsoft.NET.Sdk.WindowsDesktop`; update/replace UI packages (Caliburn.Micro, XAML behaviors); verify XAML namespaces and resource dictionaries.
  - Risk: High.

14) `projects\Gibbed.MassEffectAndromeda.DumpPartyMembers\Gibbed.MassEffectAndromeda.DumpPartyMembers.csproj`
  - Current state: small utility.
  - Target state: `net10.0`.
  - Migration steps: convert csproj; run tests.
  - Risk: Low.

15) `projects\Gibbed.PortableExecutable\Gibbed.PortableExecutable.csproj`
  - Current state: PE parsing library.
  - Target state: `net10.0`.
  - Migration steps: convert csproj; validate parsing with sample binaries.
  - Risk: Medium (binary-parsing edge cases).

16) `projects\Gibbed.MassEffectAndromeda.DumpPlotFlags\Gibbed.MassEffectAndromeda.DumpPlotFlags.csproj`
  - Current state: small utility.
  - Target state: `net10.0`.
  - Migration steps: convert csproj; test behaviors.
  - Risk: Low.

17) `projects\Gibbed.MassEffectAndromeda.DumpItemTypes\Gibbed.MassEffectAndromeda.DumpItemTypes.csproj`
  - Current state: small utility.
  - Target state: `net10.0`.
  - Migration steps: convert csproj; run sample runs.
  - Risk: Low.

18) `projects\Gibbed.Frostbite3.GeneratePartitionMap\Gibbed.Frostbite3.GeneratePartitionMap.csproj`
  - Current state: tool that depends on lower-level format libraries.
  - Target state: `net10.0`.
  - Migration steps: convert csproj; validate with representative data.
  - Risk: Medium.

19) `projects\Gibbed.Frostbite3.ConvertDbObject\Gibbed.Frostbite3.ConvertDbObject.csproj`
  - Current state: conversion utility.
  - Target state: `net10.0`.
  - Migration steps: convert csproj; validate DB object conversion routines.
  - Risk: Medium.

20) `projects\Gibbed.Frostbite3.UnpackPartitions\Gibbed.Frostbite3.UnpackPartitions.csproj`
  - Current state: unpacking tool.
  - Target state: `net10.0`.
  - Migration steps: convert csproj; verify file outputs.
  - Risk: Medium.

21) `projects\Gibbed.Frostbite3.UnpackInitFS\Gibbed.Frostbite3.UnpackInitFS.csproj`
  - Current state: unpacking tool.
  - Target state: `net10.0`.
  - Migration steps: convert csproj; verify outputs.
  - Risk: Medium.

22) `projects\Gibbed.Frostbite3.UnpackResources\Gibbed.Frostbite3.UnpackResources.csproj`
  - Current state: resource unpacker.
  - Target state: `net10.0`.
  - Migration steps: convert csproj; test with sample files.
  - Risk: Medium.

Notes on accuracy and required verification:
- Every stub above must be augmented with exact current TargetFramework, PackageReference list, LOC metrics, and test coverage discovered by `upgrade_analyze_projects` before execution.
- Items flagged as High risk should receive more detailed analysis iterations (file-level) before attempting automated conversion.

6. Package update reference & security notes
-------------------------------------------
- This plan requires calling `upgrade_analyze_projects` to extract the exact list of packages with suggested versions and any security vulnerabilities. The execution preparatory step MUST list all packages with suggested versions; include them here verbatim.
- Guidance until analysis: prefer package versions that explicitly support .NET 10 or .NET 8+; for Windows-specific packages ensure `net10.0-windows` support.
- Security items: any NuGet packages flagged with vulnerabilities must be upgraded as part of their tier's update step and called out in the execution report.

Package update policy (must be followed by executors)
- Include ALL packages from the assessment that have a "Suggested Version". Do not skip updates without documented justification.
- For packages with security vulnerabilities: upgrade immediately within the tier or escalate if upgrade breaks functionality.
- For EF Core / database packages: align EF Core major version with target framework support and update related providers (Npgsql, Microsoft.Data.SqlClient) together.
- For ASP.NET Core packages (notably if any web projects exist): move in lockstep with framework update.
- For netstandard libraries: may remain on `netstandard2.1` only if necessary; document cross-targeting strategy.

Testing checklist (per-tier and final validation)
- Per-tier checklist (required before marking tier complete):
  - [ ] SDK-style conversion applied for all projects in tier
  - [ ] TargetFramework updated to `net10.0` (or `net10.0-windows`) in project files
  - [ ] All PackageReferences updated as per assessment
  - [ ] `dotnet build` succeeds for all projects in tier
  - [ ] Unit tests for tier run and pass
  - [ ] Integration compilation: immediate consumers compile against upgraded tier
  - [ ] No critical security vulnerabilities remain in the updated package set

- Final solution checklist (after all tiers):
  - [ ] Full solution `dotnet build` is successful
  - [ ] All unit and integration tests pass
  - [ ] WPF application(s) manually verified for basic functionality
  - [ ] No unresolved high-severity warnings or security issues


7. Breaking changes catalog (expected classes)
---------------------------------------------
- WPF: XAML compilation/behavior changes, resource dictionaries, theme/dpi behavior, Application startup differences.
- BCL changes: some legacy APIs removed/obsoleted (BinaryFormatter, WebClient, certain cryptography APIs). Use recommended replacements.
- Serialization: update to modern System.Text.Json where feasible; verify third-party serializers.
- Native interop: runtime and marshalling differences; verify `CallingConvention` and structure layout.

8. Testing & validation strategy
--------------------------------
- Per-tier validation checklist (before marking tier complete):
  - [ ] All projects in tier compile without errors
  - [ ] No new warnings that are actionable (treat severe warnings as failures)
  - [ ] Unit tests in tier pass
  - [ ] Integration build: consumers in higher tiers compile against upgraded artifacts
  - [ ] Smoke tests for CLI tools
  - [ ] Manual/automated UI smoke tests for WPF apps (Tier 5)

- Full-solution checks performed after Tier 5: full build, all tests executed, a selection of end-to-end scenarios validated.

9. Risk management & mitigations
--------------------------------
- High-risk items:
  - `SaveEdit` (WPF): High risk — mitigate by delaying until all libraries are stable and by isolating UI package upgrades.
  - `Gibbed.IO` (net4.0): Medium→High — mitigate with temporary multi-targeting or a compatibility shim if needed.

- Rollback/contingency options:
  - If a project cannot be upgraded, keep it on the last supported .NET Framework and document interoperability strategy (NuGet packages, inter-process communication, or separate tooling).
  - Create feature branches per tier to enable safe PRs and rollbacks.

Mitigations and risk controls
- For WPF compatibility:
  - Maintain a branch that only contains library upgrades; keep UI changes isolated to a later branch.
  - Replace or wrap problematic third-party UI libraries; prefer CommunityToolkit.Mvvm / Microsoft.Extensions.Hosting for app startup modernization.
- For legacy TFMs:
  - Consider temporary multi-targeting (e.g., `net48;net10.0`) for shared libraries to ease migration. Document this as a temporary measure only.
  - If multi-targeting is infeasible, provide adapters (separate compatibility packages) or IPC boundaries.
- For native interop:
  - Validate runtime identifiers and native library loading; prefer System.Runtime.InteropServices.SafeHandle patterns and explicit struct layout.

11. Source control & branch strategy
-----------------------------------
- Branch naming: use `upgrade/net10/tier-<n>` per tier, with an integration branch `upgrade/net10/integration` that merges completed tier branches in order.
- Commit granularity:
  - One commit for SDK conversions across the tier
  - One commit for package updates across the tier
  - One or more commits for grouped code fixes discovered during compilation
- PR requirements:
  - Each tier PR must include: list of changed projects, `dotnet build` results, unit test summary, and any deferred compatibility decisions.
  - PRs should be reviewed by the team members responsible for the consuming projects.


10. Source control & branch strategy
-----------------------------------
- Branch: `upgrade-to-NET10` (already present). Create per-tier branches as needed, e.g., `upgrade/net10/tier-1`.
- Commit strategy: batched commits per tier with clear messages: `upgrade(tier-1): convert to SDK-style + target net10.0`.
- PR/merge: create PR per tier. PR must include build results and test status.

11. Success criteria
--------------------
Migration considered complete when:
- All targeted projects' `TargetFramework` set to the proposed `net10.0` (or `net10.0-windows` for WPF) as recorded in execution logs.
- All package updates from the assessment are applied or explicitly deferred with justification.
- All projects build without errors; tests pass; critical warnings resolved.
- No unresolved security vulnerabilities in package dependency graph (or documented mitigations).

12. Next steps & outstanding information
---------------------------------------
Required before execution:
- Run `upgrade_analyze_projects` (targetFramework: `net10.0`) to obtain per-project package suggestions, compatibility rules, and security vulnerability list. This data must be embedded into Section 6 of this plan prior to execution.
- Validate the tier assignments using `upgrade_get_project_dependencies` for projects that appear ambiguous.
- Decide policy for legacy TFMs (keep on net48 vs attempt migration). The user chose "attempt all" so the execution agent should attempt migration and flag blockers.

⚠️ Guardrail reminder: This document is planning-only. I will not execute or modify source code. To proceed to execution, switch to an execution agent or perform the steps described here.

Generated by Copilot App Modernization Agent.
