# Migration Plan: SDK-Style Project Conversion

**Scenario**: Migrate from non-SDK-style projects to SDK-style projects in .NET  
**Repository**: C:\Users\david\source\repos\Gibbed.MassEffectAndromeda  
**Solution**: Mass Effect Andromeda.sln  
**Target Framework**: .NET 10 (net10.0-windows10.0.17763.0)  
**Working Branch**: upgrade-to-NET10  
**Date**: 2026-02-17

---

## Table of Contents

1. [Executive Summary](#executive-summary)
2. [Migration Strategy](#migration-strategy)
3. [Detailed Dependency Analysis](#detailed-dependency-analysis)
4. [Project-by-Project Plans](#project-by-project-plans)
5. [Risk Management](#risk-management)
6. [Testing & Validation Strategy](#testing--validation-strategy)
7. [Complexity & Effort Assessment](#complexity--effort-assessment)
8. [Source Control Strategy](#source-control-strategy)
9. [Success Criteria](#success-criteria)

---

## Executive Summary

### Scenario Overview

This migration plan addresses the conversion of projects from legacy (non-SDK-style) to modern SDK-style project format in the Mass Effect Andromeda solution. The assessment revealed that **all 22 projects already use SDK-style format** (`<Project Sdk="Microsoft.NET.Sdk">`), which significantly reduces the primary conversion work.

### Scope

- **Total Projects**: 22
- **Current State**: All projects already migrated to SDK-style format
- **Target Framework**: .NET 10 (net10.0-windows10.0.17763.0)
- **Working Branch**: upgrade-to-NET10

### Discovered Metrics

**Project Complexity**:
- Leaf nodes (no dependencies): 1 (Gibbed.IO)
- Mid-tier libraries: 13
- Application projects: 8 (tools and utilities)
- Maximum dependency depth: 3 levels
- Circular dependencies: None detected

**Legacy Settings Present**:
- Projects with `GenerateAssemblyInfo=false`: Most projects (requires review)
- Projects with `LangVersion=preview`: All projects
- Projects with legacy `CodeAnalysisRuleSet` references: At least 2 (Gibbed.MassEffectAndromeda.SaveEdit, Gibbed.PortableExecutable)
- Projects with legacy `Reference` elements: At least 1 (Gibbed.Frostbite3.UnpackResources)
- Projects with obsolete properties: `RestorePackages`, `SolutionDir`, hard-coded paths in CodeAnalysis properties

**Risk Indicators**:
- High-risk projects: 1 (Gibbed.MassEffectAndromeda.SaveEdit - WPF application with complex legacy settings)
- Security vulnerabilities: None identified
- Breaking changes: None expected (already SDK-style)

### Complexity Classification

**Classification: Simple-to-Medium**

**Justification**:
- ✅ All projects already SDK-style (primary conversion complete)
- ✅ No circular dependencies
- ✅ Dependency depth manageable (≤3 levels)
- ✅ 22 projects is moderate size
- ⚠️ Legacy settings scattered across projects require cleanup
- ⚠️ 1 high-complexity WPF application with extensive legacy properties

**Iteration Strategy**: **Phase-based batch approach**
- Phase 1: Leaf nodes and simple libraries (batch together)
- Phase 2: Mid-tier libraries (batch by complexity)
- Phase 3: Application projects (batch simple, separate complex)

**Expected Iterations**: 6-8 iterations total (3 discovery/strategy, 3-5 detail generation)

### Critical Issues

1. **Projects already SDK-style**: No file format conversion needed via `dotnet try-convert`
2. **Legacy properties remain**: Need systematic cleanup of obsolete SDK-style incompatible properties
3. **Tool error encountered**: `upgrade_get_feature_instructions` failed with SQLite error (may require manual scenario guidance)

### Recommended Approach

**Incremental cleanup and validation** rather than conversion:

1. **Audit Phase**: Systematically identify all legacy properties across 22 projects
2. **Cleanup Phase**: Remove obsolete properties in dependency order (Bottom-Up Strategy)
3. **Validation Phase**: Build, test, and run analyzers after each batch
4. **Commit Strategy**: One commit per phase for easy rollback

### Selected Strategy

**Bottom-Up (Dependency-First) Strategy**

**Rationale**:
- Projects already SDK-style, so focus is on cleanup and validation
- Dependency ordering ensures changes to foundational libraries validate before dependent projects
- Allows incremental testing and rollback
- Each tier can be validated independently before proceeding

**Strategy-Specific Considerations**:
- Tier 1 (Leaf node): Gibbed.IO - cleanup and validate first
- Tier 2 (Low-dependency libraries): Batch cleanup of format/common/etc.
- Tier 3 (Application projects): SaveEdit requires separate iteration due to complexity

---

## Migration Strategy

### Approach Selection

**Selected Approach: Incremental Cleanup with Bottom-Up Validation**

### Justification

This is **not a traditional SDK-style conversion** because all projects already use `<Project Sdk="Microsoft.NET.Sdk">`. Instead, this is a **legacy property cleanup and validation** operation with the following characteristics:

1. **Low conversion risk**: No file format transformation needed
2. **Cleanup focus**: Remove obsolete properties introduced during past migrations
3. **Validation emphasis**: Ensure builds, tests, and analyzers run cleanly
4. **Incremental benefit**: Each tier cleanup improves build performance and maintainability

**Why not all-at-once?**
- Different projects have different legacy properties
- WPF application (SaveEdit) requires careful handling
- Bottom-up approach allows validation at each tier before proceeding
- Easier rollback if issues discovered

### Bottom-Up Strategy Rationale

**Why Bottom-Up for this scenario:**

1. **Foundation first**: Clean Gibbed.IO (foundational library) validates that no legacy properties break dependent projects
2. **Incremental validation**: Each tier builds on clean, validated lower tiers
3. **Risk isolation**: If cleanup breaks a project, only that tier is affected (lower tiers remain stable)
4. **Learning curve**: Patterns discovered in Tier 1-2 inform cleanup of more complex projects
5. **Clear milestones**: Each tier completion is a validation checkpoint

**Strategy-Specific Ordering**:
- **Tier 1 (Foundation)**: Gibbed.IO - single project, straightforward cleanup
- **Tier 2 (Core Libraries)**: 7 projects - batch cleanup, simple libraries with minimal legacy settings
- **Tier 3 (Mid-Tier)**: 5 projects - batch cleanup, may have more legacy settings
- **Tier 4a (Simple Apps)**: 7 projects - batch cleanup, console applications
- **Tier 4b (Complex Apps)**: 2 projects - individual cleanup, WPF and complex console app

### Dependency-Based Ordering Rationale

**Bottom-Up Ordering Principles Applied**:

1. **Tier determination**: Projects grouped by dependency tier (0 internal refs = Tier 1, depends only on Tier 1 = Tier 2, etc.)
2. **No tier skipping**: Cannot clean Tier N+1 before Tier N validated
3. **Tier completion criteria**: All projects in tier build, pass tests, analyzer-clean
4. **Between-tier validation**: Verify no regressions in cleaned tiers before proceeding

**Execution Flow Per Tier**:

Each tier follows this pattern:

1. **Audit** (one task per tier)
   - Identify all legacy properties in tier's projects
   - Document removal plan

2. **Cleanup** (one task per tier)
   - Remove obsolete properties from all projects in tier
   - Update .csproj files
   - Commit changes

3. **Validation** (one task per tier)
   - Build all projects in tier
   - Run tests (if applicable)
   - Run analyzers and apply fixes
   - Verify no warnings/errors
   - Commit analyzer fixes

4. **Checkpoint** (included in validation)
   - Confirm tier complete
   - Document any issues or lessons learned
   - Proceed to next tier

### Parallel vs Sequential Execution

**Sequential by tier, parallel within tier**:

- **Between tiers**: Sequential (must complete Tier N before starting Tier N+1)
- **Within tier**: Parallel possible (projects in same tier can be cleaned simultaneously)
- **Exception**: Tier 4b projects (SaveEdit and UnpackResources) should be sequential due to complexity

### Phase Definitions

**Phase 1: Foundation Cleanup**
- **Scope**: Tier 1 (Gibbed.IO)
- **Duration**: Low complexity
- **Deliverable**: Clean, validated foundational library

**Phase 2: Core Libraries Cleanup**
- **Scope**: Tier 2 (7 projects)
- **Duration**: Low complexity (batch operation)
- **Deliverable**: Clean core libraries with no legacy properties

**Phase 3: Mid-Tier Cleanup**
- **Scope**: Tier 3 (5 projects)
- **Duration**: Medium complexity
- **Deliverable**: Clean mid-tier libraries

**Phase 4: Simple Applications Cleanup**
- **Scope**: Tier 4a (7 console applications)
- **Duration**: Medium complexity (batch operation)
- **Deliverable**: Clean, validated application projects

**Phase 5: Complex Applications Cleanup**
- **Scope**: Tier 4b (UnpackResources, SaveEdit)
- **Duration**: High complexity (individual attention)
- **Deliverable**: All projects SDK-style clean, validated

### Bottom-Up Strategy Specific Considerations

**Tier Risk Assessment**:

- **Tier 1**: Low risk (single project, simple library)
- **Tier 2**: Low risk (simple libraries, uniform legacy properties expected)
- **Tier 3**: Medium risk (may have more diverse legacy settings)
- **Tier 4a**: Medium risk (console apps, platform-specific settings)
- **Tier 4b**: High risk (WPF app with extensive legacy CodeAnalysis settings, complex console app with legacy Reference elements)

**Incremental Benefits Per Tier**:

- **After Tier 1**: Foundation clean, validates SDK behavior
- **After Tier 2**: 8 of 22 projects clean (36%)
- **After Tier 3**: 13 of 22 projects clean (59%)
- **After Tier 4a**: 20 of 22 projects clean (91%)
- **After Tier 4b**: All projects clean (100%)

**Multi-Project Tier Handling**:

- **Tiers 2, 3, 4a**: Batch operations (all projects in tier cleaned in single pass)
- **Tier 4b**: Individual operations (one project at a time)
- **Testing**: Entire tier tested together (cumulative validation)

---

## Detailed Dependency Analysis

### Dependency Graph Summary

The solution follows a clean dependency hierarchy with no circular dependencies detected. Projects are organized in a bottom-up structure from foundational libraries through mid-tier components to application executables.

### Topological Order (Bottom-Up)

Projects listed in dependency order (leaf nodes first):

```
Tier 1: Leaf Nodes (No Internal Dependencies)
  1. Gibbed.IO

Tier 2: Low-Dependency Libraries (Depend only on Tier 1)
  2. Gibbed.Frostbite3.Common → Gibbed.IO
  3. Gibbed.Frostbite3.ResourceFormats → Gibbed.IO
  4. Gibbed.Frostbite3.VfsFormats → Gibbed.IO
  5. Gibbed.Frostbite3.Zstd → Gibbed.IO
  6. Gibbed.MassEffectAndromeda.FileFormats → Gibbed.IO
  7. Gibbed.MassEffectAndromeda.GameInfo → Gibbed.IO
  8. NDesk.Options (standalone)

Tier 3: Mid-Tier Libraries (Depend on Tier 1-2)
  9. Gibbed.Frostbite3.Dynamic → Gibbed.Frostbite3.Common, Gibbed.IO
  10. Gibbed.Frostbite3.Unbundling → Gibbed.Frostbite3.VfsFormats, Gibbed.IO
  11. Gibbed.MassEffectAndromeda.SaveFormats → Gibbed.MassEffectAndromeda.FileFormats, Gibbed.IO
  12. Gibbed.MassEffectAndromeda.Dumping → Gibbed.MassEffectAndromeda.GameInfo, Gibbed.Frostbite3.ResourceFormats
  13. Gibbed.PortableExecutable → Gibbed.IO

Tier 4: Application Projects (Depend on multiple tiers)
  14. Gibbed.MassEffectAndromeda.SaveEdit (WPF) → SaveFormats, GameInfo, FileFormats
  15. Gibbed.MassEffectAndromeda.DumpPartyMembers → Dumping, Frostbite3.Dynamic
  16. Gibbed.MassEffectAndromeda.DumpPlotFlags → PortableExecutable, Frostbite3.Dynamic
  17. Gibbed.MassEffectAndromeda.DumpItemTypes → Dumping
  18. Gibbed.Frostbite3.GeneratePartitionMap → Frostbite3.Unbundling, VfsFormats
  19. Gibbed.Frostbite3.ConvertDbObject → Frostbite3.Dynamic
  20. Gibbed.Frostbite3.UnpackPartitions → Frostbite3.Unbundling, VfsFormats
  21. Gibbed.Frostbite3.UnpackInitFS → Frostbite3.VfsFormats
  22. Gibbed.Frostbite3.UnpackResources → Multiple (Complex)
```

### Migration Phase Groupings

Based on Bottom-Up Strategy and complexity, projects are grouped into the following migration phases:

**Phase 1: Foundation (Tier 1)**
- Gibbed.IO

**Phase 2: Core Libraries (Tier 2 - Simple)**
- Gibbed.Frostbite3.Common
- Gibbed.Frostbite3.ResourceFormats
- Gibbed.Frostbite3.VfsFormats
- Gibbed.Frostbite3.Zstd
- Gibbed.MassEffectAndromeda.FileFormats
- Gibbed.MassEffectAndromeda.GameInfo
- NDesk.Options

**Phase 3: Mid-Tier Libraries (Tier 3)**
- Gibbed.Frostbite3.Dynamic
- Gibbed.Frostbite3.Unbundling
- Gibbed.MassEffectAndromeda.SaveFormats
- Gibbed.MassEffectAndromeda.Dumping
- Gibbed.PortableExecutable

**Phase 4: Simple Applications (Tier 4 - Low Complexity)**
- Gibbed.MassEffectAndromeda.DumpPartyMembers
- Gibbed.MassEffectAndromeda.DumpPlotFlags
- Gibbed.MassEffectAndromeda.DumpItemTypes
- Gibbed.Frostbite3.GeneratePartitionMap
- Gibbed.Frostbite3.ConvertDbObject
- Gibbed.Frostbite3.UnpackPartitions
- Gibbed.Frostbite3.UnpackInitFS

**Phase 5: Complex Applications (Tier 4 - High Complexity)**
- Gibbed.Frostbite3.UnpackResources (legacy Reference elements)
- Gibbed.MassEffectAndromeda.SaveEdit (WPF with extensive legacy settings)

### Critical Path Identification

**Primary Critical Path**:
```
Gibbed.IO → Gibbed.MassEffectAndromeda.FileFormats → 
Gibbed.MassEffectAndromeda.SaveFormats → Gibbed.MassEffectAndromeda.SaveEdit
```

This path represents the most complex chain leading to the high-risk SaveEdit WPF application.

**Secondary Critical Paths**:
- Gibbed.IO → Gibbed.Frostbite3.VfsFormats → Gibbed.Frostbite3.Unbundling → UnpackResources
- Gibbed.IO → Gibbed.Frostbite3.Common → Gibbed.Frostbite3.Dynamic → Multiple tools

### Circular Dependency Details

**No circular dependencies detected** - clean dependency hierarchy allows straightforward bottom-up migration.

### Ordering Rationale (Bottom-Up Strategy Alignment)

1. **Tier 1 first**: Gibbed.IO is the foundation - all other projects depend on it directly or transitively
2. **Tier 2 batched**: Low-dependency libraries can be processed together as they only depend on Gibbed.IO
3. **Tier 3 batched**: Mid-tier libraries build on validated Tiers 1-2
4. **Tier 4 split by complexity**: Simple tools batched together, complex applications handled separately
5. **SaveEdit last**: Highest complexity, most legacy settings, benefits from all lower tiers validated first

---

## Project-by-Project Plans

### Tier 1: Foundation

#### Gibbed.IO

**Current State**:
- **Format**: SDK-style (`<Project Sdk="Microsoft.NET.Sdk">`)
- **Target Framework**: net10.0-windows10.0.17763.0
- **Output Type**: Library
- **Dependencies**: None (leaf node)
- **Dependants**: 18 projects depend on this (directly or transitively)
- **Legacy Settings**: 
  - `GenerateAssemblyInfo=false`
  - `LangVersion=preview`
  - `Configurations=Debug;Release;SaveEdit Packaging` (custom configuration)

**Target State**:
- SDK-style project with minimal properties
- Remove or justify all legacy properties
- Validate SDK defaults work correctly
- Ensure builds cleanly with analyzers

**Migration Steps**:

1. **Prerequisites**
   - Verify .NET 10 SDK installed
   - Ensure working branch `upgrade-to-NET10` is current
   - Backup current .csproj file

2. **Project File Audit**
   - Review current .csproj for all properties
   - Identify SDK defaults vs explicit overrides
   - Document reason for each legacy property

3. **Legacy Property Analysis**

   | Property | Current Value | SDK Default | Action | Reason |
   |----------|--------------|-------------|--------|--------|
   | `GenerateAssemblyInfo` | false | true | **Investigate then decide** | May be disabled to avoid duplicates with existing AssemblyInfo.cs; check if AssemblyInfo.cs exists |
   | `LangVersion` | preview | Default for TFM | **Change to explicit version** | `preview` is unstable; use `latest` or specific version (e.g., `12.0` for C# 12) |
   | `Configurations` | Custom | Debug;Release | **Keep** | Solution uses "SaveEdit Packaging" configuration; needed for solution builds |
   | `TargetFramework` | net10.0-windows10.0.17763.0 | N/A | **Keep** | Required for Windows-specific APIs |
   | `OutputType` | Library | Library (for SDK) | **Remove** | SDK default for class libraries is Library |

4. **Code Modifications**

   **Check for AssemblyInfo.cs**:
   - If `Properties/AssemblyInfo.cs` exists: Keep `GenerateAssemblyInfo=false` OR remove AssemblyInfo.cs
   - If no AssemblyInfo.cs: Remove `GenerateAssemblyInfo=false` (enable auto-generation)

   **Update LangVersion**:
   - Change `<LangVersion>preview</LangVersion>` to `<LangVersion>latest</LangVersion>` or `<LangVersion>12.0</LangVersion>`

   **Remove OutputType** (optional):
   - Since SDK default for class library is `Library`, this can be removed for cleanliness

   **Expected .csproj after cleanup**:
   ```xml
   <Project Sdk="Microsoft.NET.Sdk">
     <PropertyGroup>
       <TargetFramework>net10.0-windows10.0.17763.0</TargetFramework>
       <GenerateAssemblyInfo>false</GenerateAssemblyInfo><!-- Keep only if AssemblyInfo.cs exists -->
       <Configurations>Debug;Release;SaveEdit Packaging</Configurations>
       <LangVersion>latest</LangVersion><!-- Changed from preview -->
     </PropertyGroup>
   </Project>
   ```

5. **Testing Strategy**

   **Build Validation**:
   - Run `dotnet build projects\Gibbed.IO\Gibbed.IO.csproj`
   - Verify zero errors, zero warnings

   **Dependent Projects Validation**:
   - Build all 18 dependent projects to ensure no regressions
   - Run `dotnet build "Mass Effect Andromeda.sln"`

   **Analyzer Validation**:
   - Run `dotnet format projects\Gibbed.IO\Gibbed.IO.csproj --verify-no-changes`
   - If changes needed, apply: `dotnet format projects\Gibbed.IO\Gibbed.IO.csproj`
   - Review and commit formatting changes

   **Unit Tests** (if applicable):
   - Run any unit tests targeting Gibbed.IO
   - Verify all tests pass

6. **Validation Checklist**
   - [ ] .csproj file cleaned of unnecessary properties
   - [ ] `LangVersion` changed from `preview` to explicit version
   - [ ] `GenerateAssemblyInfo` decision documented
   - [ ] Project builds successfully (`dotnet build`)
   - [ ] Solution builds successfully (all dependents validated)
   - [ ] Analyzer runs cleanly (`dotnet format --verify-no-changes`)
   - [ ] No new warnings introduced
   - [ ] Changes committed with clear message (e.g., "Clean legacy properties from Gibbed.IO.csproj")

**Expected Breaking Changes**: None expected (SDK-style already in use)

**Risk Level**: Low

**Complexity**: Low - single project, minimal legacy properties

---

### Tier 2: Core Libraries

#### Tier 2: Batch Cleanup Plan

**Scope**: 7 projects in Tier 2 (Core Libraries)

**Common Current State** (all projects):
- **Format**: SDK-style
- **Target Framework**: net10.0-windows10.0.17763.0
- **Output Type**: Library
- **Common Legacy Settings**:
  - `GenerateAssemblyInfo=false`
  - `LangVersion=preview`
  - `Configurations=Debug;Release;SaveEdit Packaging`
  - `OutputType=Library` (redundant with SDK default)

**Target State**: Clean SDK-style projects with minimal properties

**Migration Steps** (applies to all 7 projects):

1. **Prerequisites**
   - Tier 1 (Gibbed.IO) must be complete and validated
   - Verify all Tier 2 projects build against cleaned Gibbed.IO

2. **Batch Audit**
   - Scan all 7 .csproj files for legacy properties
   - Create property inventory table
   - Identify any project-specific deviations

3. **Batch Cleanup Operations**

   For each project in Tier 2:

   | Property | Action | Justification |
   |----------|--------|---------------|
   | `OutputType=Library` | **Remove** | SDK default for class libraries |
   | `GenerateAssemblyInfo=false` | **Keep (if AssemblyInfo.cs exists)** or **Remove** | Check each project's Properties folder |
   | `LangVersion=preview` | **Change to `latest`** | Align with Tier 1; avoid unstable preview features |
   | `Configurations` | **Keep** | Required for solution-level "SaveEdit Packaging" configuration |
   | `TargetFramework` | **Keep** | Required |

   **Expected .csproj pattern after cleanup**:
   ```xml
   <Project Sdk="Microsoft.NET.Sdk">
     <PropertyGroup>
       <TargetFramework>net10.0-windows10.0.17763.0</TargetFramework>
       <GenerateAssemblyInfo>false</GenerateAssemblyInfo><!-- Only if AssemblyInfo.cs exists -->
       <Configurations>Debug;Release;SaveEdit Packaging</Configurations>
       <LangVersion>latest</LangVersion>
     </PropertyGroup>
     <ItemGroup>
       <ProjectReference Include="..\Gibbed.IO\Gibbed.IO.csproj" />
     </ItemGroup>
   </Project>
   ```

4. **Testing Strategy**

   **Per-Project Build**:
   - Build each of 7 projects individually
   - Verify zero errors, zero warnings

   **Tier Validation**:
   - Build all Tier 2 projects together
   - Validate against Tier 1 (Gibbed.IO)

   **Dependent Projects Check**:
   - Build Tier 3 projects (which depend on Tier 2) to ensure no regressions

   **Analyzer Validation**:
   - Run `dotnet format` on each project
   - Apply fixes, commit separately

5. **Validation Checklist** (per project)
   - [ ] Legacy properties cleaned
   - [ ] `LangVersion` changed to `latest`
   - [ ] Project builds successfully
   - [ ] Tier builds successfully (all 7 together)
   - [ ] Tier 3 projects still build (regression check)
   - [ ] Analyzer clean
   - [ ] Changes committed

**Projects in Tier 2**:
1. **Gibbed.Frostbite3.Common** → Gibbed.IO
2. **Gibbed.Frostbite3.ResourceFormats** → Gibbed.IO
3. **Gibbed.Frostbite3.VfsFormats** → Gibbed.IO
4. **Gibbed.Frostbite3.Zstd** → Gibbed.IO
5. **Gibbed.MassEffectAndromeda.FileFormats** → Gibbed.IO
6. **Gibbed.MassEffectAndromeda.GameInfo** → Gibbed.IO
7. **NDesk.Options** (no internal dependencies, but included in Tier 2 for batching)

**Expected Breaking Changes**: None

**Risk Level**: Low (uniform batch operation)

**Complexity**: Low (simple libraries with expected uniform legacy properties)

---

### Tier 3: Mid-Tier Libraries

#### Tier 3: Mid-Tier Libraries Cleanup Plan

**Scope**: 5 projects in Tier 3

**Common Current State** (most projects):
- **Format**: SDK-style
- **Target Framework**: net10.0-windows10.0.17763.0
- **Output Type**: Library
- **Common Legacy Settings**: Same as Tier 2 (GenerateAssemblyInfo=false, LangVersion=preview, etc.)

**Special Cases**:
- **Gibbed.PortableExecutable**: Has `CodeAnalysisRuleSet=AllRules.ruleset` (custom ruleset)

**Migration Steps**:

1. **Prerequisites**
   - Tiers 1-2 must be complete and validated
   - Verify all Tier 3 projects build against cleaned lower tiers

2. **Tier Audit**

   | Project | Dependencies | Unique Properties | Risk |
   |---------|--------------|-------------------|------|
   | Gibbed.Frostbite3.Dynamic | Common, ResourceFormats, IO | None | Low |
   | Gibbed.Frostbite3.Unbundling | VfsFormats, IO | None | Low |
   | Gibbed.MassEffectAndromeda.SaveFormats | FileFormats, IO | None | Low |
   | Gibbed.MassEffectAndromeda.Dumping | GameInfo, ResourceFormats | None | Low |
   | Gibbed.PortableExecutable | IO | **CodeAnalysisRuleSet** | Medium |

3. **Standard Cleanup (4 projects)**

   For Gibbed.Frostbite3.Dynamic, Gibbed.Frostbite3.Unbundling, Gibbed.MassEffectAndromeda.SaveFormats, Gibbed.MassEffectAndromeda.Dumping:

   - Remove `OutputType=Library` (SDK default)
   - Keep or remove `GenerateAssemblyInfo=false` (check for AssemblyInfo.cs)
   - Change `LangVersion=preview` to `LangVersion=latest`
   - Keep `Configurations` (solution requirement)

   **Expected .csproj**:
   ```xml
   <Project Sdk="Microsoft.NET.Sdk">
     <PropertyGroup>
       <TargetFramework>net10.0-windows10.0.17763.0</TargetFramework>
       <GenerateAssemblyInfo>false</GenerateAssemblyInfo><!-- If AssemblyInfo.cs exists -->
       <Configurations>Debug;Release;SaveEdit Packaging</Configurations>
       <LangVersion>latest</LangVersion>
     </PropertyGroup>
     <ItemGroup>
       <!-- ProjectReferences -->
     </ItemGroup>
   </Project>
   ```

4. **Special Cleanup: Gibbed.PortableExecutable**

   **Current .csproj** (observed):
   ```xml
   <Project Sdk="Microsoft.NET.Sdk">
     <PropertyGroup>
       <TargetFramework>net10.0-windows10.0.17763.0</TargetFramework>
       <OutputType>Library</OutputType>
       <GenerateAssemblyInfo>false</GenerateAssemblyInfo>
       <CodeAnalysisRuleSet>AllRules.ruleset</CodeAnalysisRuleSet>
       <Configurations>Debug;Release;SaveEdit Packaging</Configurations>
       <LangVersion>preview</LangVersion>
     </PropertyGroup>
     <ItemGroup>
       <ProjectReference Include="..\Gibbed.IO\Gibbed.IO.csproj" />
     </ItemGroup>
   </Project>
   ```

   **Cleanup Actions**:

   | Property | Action | Reason |
   |----------|--------|--------|
   | `OutputType=Library` | **Remove** | SDK default |
   | `GenerateAssemblyInfo=false` | **Check AssemblyInfo.cs** | Keep if exists |
   | `CodeAnalysisRuleSet=AllRules.ruleset` | **Verify then decide** | Check if AllRules.ruleset file exists in project; verify compatibility with .NET 10 analyzers |
   | `LangVersion=preview` | **Change to `latest`** | Consistency |

   **CodeAnalysisRuleSet Decision Tree**:
   - If `AllRules.ruleset` file exists in project directory:
     - **Keep property** (custom ruleset still in use)
     - Verify ruleset file is .NET 10 compatible
   - If `AllRules.ruleset` file NOT found:
     - **Remove property** (orphaned reference)
     - Consider migrating to `.editorconfig` for modern analyzer configuration

   **Expected .csproj** (if ruleset exists):
   ```xml
   <Project Sdk="Microsoft.NET.Sdk">
     <PropertyGroup>
       <TargetFramework>net10.0-windows10.0.17763.0</TargetFramework>
       <GenerateAssemblyInfo>false</GenerateAssemblyInfo>
       <CodeAnalysisRuleSet>AllRules.ruleset</CodeAnalysisRuleSet><!-- Keep if file exists -->
       <Configurations>Debug;Release;SaveEdit Packaging</Configurations>
       <LangVersion>latest</LangVersion>
     </PropertyGroup>
     <ItemGroup>
       <ProjectReference Include="..\Gibbed.IO\Gibbed.IO.csproj" />
     </ItemGroup>
   </Project>
   ```

5. **Testing Strategy**

   **Per-Project Build**:
   - Build each of 5 projects individually
   - Verify zero errors, zero warnings

   **Tier Validation**:
   - Build all Tier 3 projects together
   - Validate against Tiers 1-2

   **Analyzer Validation**:
   - Run `dotnet format` on each project
   - For PortableExecutable: verify custom ruleset still applies

   **Dependent Projects Check**:
   - Build Tier 4 projects to ensure no regressions

6. **Validation Checklist** (per project)
   - [ ] Legacy properties cleaned
   - [ ] `LangVersion` changed to `latest`
   - [ ] PortableExecutable: CodeAnalysisRuleSet decision documented
   - [ ] Project builds successfully
   - [ ] Tier builds successfully (all 5 together)
   - [ ] Tier 4 projects still build (regression check)
   - [ ] Analyzer clean (respects custom ruleset if present)
   - [ ] Changes committed

**Expected Breaking Changes**: None

**Risk Level**: Medium (due to custom ruleset in PortableExecutable)

**Complexity**: Medium (one project requires special handling)

---

### Tier 4a: Simple Applications

#### Tier 4a: Simple Applications Cleanup Plan

**Scope**: 7 console application projects

**Common Current State**:
- **Format**: SDK-style
- **Target Framework**: net10.0-windows10.0.17763.0
- **Output Type**: Exe (console applications)
- **Common Legacy Settings**:
  - `GenerateAssemblyInfo=false`
  - `LangVersion=preview`
  - `Configurations=Debug;Release;SaveEdit Packaging`
  - `Platform Condition` (x86 default)
  - `OutputPath` overrides (e.g., `..\..\bin\`)
  - Legacy `<Reference>` elements (System.Data.DataSetExtensions, Microsoft.CSharp)

**Target State**: Clean console applications with minimal properties

**Migration Steps**:

1. **Prerequisites**
   - Tiers 1-3 must be complete and validated
   - Verify all Tier 4a projects build against cleaned lower tiers

2. **Tier Audit**

   Sample from **Gibbed.Frostbite3.UnpackInitFS** (representative):
   ```xml
   <PropertyGroup>
     <TargetFramework>net10.0-windows10.0.17763.0</TargetFramework>
     <Platform Condition=" '$(Platform)' == '' ">x86</Platform>
     <OutputType>Exe</OutputType>
     <GenerateAssemblyInfo>false</GenerateAssemblyInfo>
     <Configurations>Debug;Release;SaveEdit Packaging</Configurations>
     <LangVersion>preview</LangVersion>
   </PropertyGroup>
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Debug|x86' ">
     <OutputPath>..\..\bin\</OutputPath>
   </PropertyGroup>
   <ItemGroup>
     <Reference Include="System.Data.DataSetExtensions" />
     <Reference Include="Microsoft.CSharp" />
   </ItemGroup>
   ```

3. **Batch Cleanup Operations**

   | Property/Element | Action | Reason |
   |------------------|--------|--------|
   | `OutputType=Exe` | **Keep** | Required for console applications |
   | `Platform Condition` | **Keep** | Solution uses x86 platform configuration |
   | `OutputPath` overrides | **Keep** | Custom output path for solution structure (bin folder at root) |
   | `GenerateAssemblyInfo=false` | **Check AssemblyInfo.cs** | Keep if exists, remove otherwise |
   | `LangVersion=preview` | **Change to `latest`** | Consistency with other tiers |
   | `<Reference Include="System.Data.DataSetExtensions">` | **Remove** | Included in .NET 10 by default |
   | `<Reference Include="Microsoft.CSharp">` | **Remove** | Included in .NET 10 by default |
   | `Configurations` | **Keep** | Solution requirement |

   **Expected .csproj pattern after cleanup**:
   ```xml
   <Project Sdk="Microsoft.NET.Sdk">
     <PropertyGroup>
       <TargetFramework>net10.0-windows10.0.17763.0</TargetFramework>
       <Platform Condition=" '$(Platform)' == '' ">x86</Platform>
       <OutputType>Exe</OutputType>
       <GenerateAssemblyInfo>false</GenerateAssemblyInfo><!-- If AssemblyInfo.cs exists -->
       <Configurations>Debug;Release;SaveEdit Packaging</Configurations>
       <LangVersion>latest</LangVersion>
     </PropertyGroup>
     <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Debug|x86' ">
       <OutputPath>..\..\bin\</OutputPath>
     </PropertyGroup>
     <ItemGroup>
       <!-- ProjectReferences only, no legacy References -->
     </ItemGroup>
   </Project>
   ```

4. **Legacy Reference Removal**

   **System.Data.DataSetExtensions and Microsoft.CSharp**:
   - Both are part of .NET 10 runtime and do not require explicit references in SDK-style projects
   - **Action**: Remove `<Reference>` elements
   - **Validation**: Build project; if compilation errors occur, investigate if specific API needs explicit reference (unlikely)

   **Rollback Plan**: If removing references causes errors, add as `<PackageReference>` instead (though unlikely needed)

5. **Testing Strategy**

   **Per-Project Build**:
   - Build each of 7 console applications individually
   - Verify zero errors, zero warnings

   **Execution Test**:
   - Run each console application with test arguments (if applicable)
   - Verify application executes without runtime errors
   - Check output is written to `..\..\bin\` as configured

   **Tier Validation**:
   - Build all Tier 4a projects together
   - Validate against Tiers 1-3

   **Analyzer Validation**:
   - Run `dotnet format` on each project
   - Apply fixes, commit separately

6. **Validation Checklist** (per project)
   - [ ] Legacy `<Reference>` elements removed
   - [ ] `LangVersion` changed to `latest`
   - [ ] `GenerateAssemblyInfo` decision documented
   - [ ] Platform and OutputPath configurations preserved
   - [ ] Project builds successfully
   - [ ] Application executes successfully
   - [ ] Output path correct (..\..\bin\)
   - [ ] Tier builds successfully (all 7 together)
   - [ ] Analyzer clean
   - [ ] Changes committed

**Projects in Tier 4a**:
1. **Gibbed.MassEffectAndromeda.DumpPartyMembers**
2. **Gibbed.MassEffectAndromeda.DumpPlotFlags**
3. **Gibbed.MassEffectAndromeda.DumpItemTypes**
4. **Gibbed.Frostbite3.GeneratePartitionMap**
5. **Gibbed.Frostbite3.ConvertDbObject**
6. **Gibbed.Frostbite3.UnpackPartitions**
7. **Gibbed.Frostbite3.UnpackInitFS**

**Expected Breaking Changes**: None (legacy references are redundant)

**Risk Level**: Medium (removing references requires validation)

**Complexity**: Medium (console apps with platform-specific settings and legacy references)

---

### Tier 4b: Complex Applications

#### Gibbed.Frostbite3.UnpackResources

**Current State**:
- **Format**: SDK-style
- **Output Type**: Console Exe
- **Dependencies**: Multiple (Frostbite3.Common, ResourceFormats, Unbundling, VfsFormats, Zstd, IO, NDesk.Options)
- **Legacy Settings**: 
  - `RestorePackages=true` (obsolete)
  - `SolutionDir` (obsolete)
  - `Platform Condition` (x86)
  - `OutputPath` override
  - **Legacy `<Reference>` elements**: System.Configuration, System.Transactions, System.Data.DataSetExtensions, Microsoft.CSharp
  - `GenerateAssemblyInfo=false`
  - `LangVersion=preview`

**Current .csproj** (observed):
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0-windows10.0.17763.0</TargetFramework>
    <Platform Condition=" '$(Platform)' == '' ">x86</Platform>
    <OutputType>Exe</OutputType>
    <SolutionDir Condition="$(SolutionDir) == '' Or $(SolutionDir) == '*Undefined*'">..\..\</SolutionDir>
    <RestorePackages>true</RestorePackages>
    <GenerateAssemblyInfo>false</GenerateAssemblyInfo>
    <Configurations>Debug;Release;SaveEdit Packaging</Configurations>
    <LangVersion>preview</LangVersion>
  </PropertyGroup>
  <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Debug|x86' ">
    <OutputPath>..\..\bin\</OutputPath>
  </PropertyGroup>
  <ItemGroup>
    <Reference Include="System.Configuration" />
    <Reference Include="System.Transactions" />
    <Reference Include="System.Data.DataSetExtensions" />
    <Reference Include="Microsoft.CSharp" />
  </ItemGroup>
  <ItemGroup>
    <!-- ProjectReferences -->
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="NLog" Version="6.1.0" />
  </ItemGroup>
</Project>
```

**Target State**: Clean SDK-style console application with minimal properties

**Migration Steps**:

1. **Prerequisites**
   - Tiers 1-4a must be complete and validated
   - Verify project builds against cleaned dependencies

2. **Legacy Property Removal**

   | Property | Action | Reason |
   |----------|--------|--------|
   | `RestorePackages=true` | **Remove** | Obsolete; SDK-style projects restore automatically |
   | `SolutionDir Condition` | **Remove** | Obsolete; SDK resolves solution directory automatically |
   | `Platform Condition` | **Keep** | Solution uses x86 platform |
   | `OutputType=Exe` | **Keep** | Required for console app |
   | `OutputPath` override | **Keep** | Custom output structure (..\..\bin\) |
   | `GenerateAssemblyInfo=false` | **Check AssemblyInfo.cs** | Keep if exists |
   | `LangVersion=preview` | **Change to `latest`** | Consistency |
   | `Configurations` | **Keep** | Solution requirement |

3. **Legacy Reference Removal and Investigation**

   | Reference | Likely Needed? | Action |
   |-----------|----------------|--------|
   | `System.Configuration` | Possibly (config files) | **Remove and test**; add back as PackageReference if needed (`System.Configuration.ConfigurationManager`) |
   | `System.Transactions` | Possibly (database transactions) | **Remove and test**; .NET 10 may include it, or add `System.Transactions.Local` package if needed |
   | `System.Data.DataSetExtensions` | No | **Remove** (included in .NET 10) |
   | `Microsoft.CSharp` | No | **Remove** (included in .NET 10) |

   **Strategy**:
   - Remove all four `<Reference>` elements
   - Build project
   - If compilation errors:
     - `System.Configuration`: Add `<PackageReference Include="System.Configuration.ConfigurationManager" Version="9.0.0" />`
     - `System.Transactions`: Add `<PackageReference Include="System.Transactions.Local" Version="9.0.0" />` (if using transactions)
   - If no compilation errors: References were redundant, removal successful

4. **Expected Breaking Changes**

   **Possible**: Removing `System.Configuration` or `System.Transactions` may cause compilation errors if application uses:
   - `ConfigurationManager` class (from System.Configuration)
   - `TransactionScope` class (from System.Transactions)

   **Mitigation**: Add appropriate NuGet packages if needed (see table above)

5. **Code Modifications**

   **Expected .csproj after cleanup**:
   ```xml
   <Project Sdk="Microsoft.NET.Sdk">
     <PropertyGroup>
       <TargetFramework>net10.0-windows10.0.17763.0</TargetFramework>
       <Platform Condition=" '$(Platform)' == '' ">x86</Platform>
       <OutputType>Exe</OutputType>
       <GenerateAssemblyInfo>false</GenerateAssemblyInfo><!-- If AssemblyInfo.cs exists -->
       <Configurations>Debug;Release;SaveEdit Packaging</Configurations>
       <LangVersion>latest</LangVersion>
     </PropertyGroup>
     <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Debug|x86' ">
       <OutputPath>..\..\bin\</OutputPath>
     </PropertyGroup>
     <ItemGroup>
       <!-- ProjectReferences -->
     </ItemGroup>
     <ItemGroup>
       <PackageReference Include="NLog" Version="6.1.0" />
       <!-- Add System.Configuration.ConfigurationManager or System.Transactions.Local if needed -->
     </ItemGroup>
   </Project>
   ```

6. **Testing Strategy**

   **Build Validation**:
   - Build project after removing legacy properties and references
   - If compilation errors, add necessary packages (see table)

   **Execution Test**:
   - Run UnpackResources.exe with test data
   - Verify application executes without runtime errors
   - Verify functionality (unpacking resources) works correctly

   **Regression Test**:
   - Compare output with previous version to ensure no behavior changes

   **Analyzer Validation**:
   - Run `dotnet format`
   - Apply fixes, commit separately

7. **Validation Checklist**
   - [ ] Obsolete properties removed (`RestorePackages`, `SolutionDir`)
   - [ ] Legacy references removed (or replaced with PackageReferences)
   - [ ] `LangVersion` changed to `latest`
   - [ ] Project builds successfully
   - [ ] Application executes successfully
   - [ ] Functionality validated (unpacking works)
   - [ ] Output path correct (..\..\bin\)
   - [ ] Analyzer clean
   - [ ] Changes committed with clear message

**Risk Level**: High (legacy references may cause compilation/runtime errors)

**Complexity**: High (requires investigation and testing of removed references)

#### Gibbed.MassEffectAndromeda.SaveEdit

**Current State**:
- **Format**: SDK-style
- **Output Type**: WinExe (WPF application)
- **Dependencies**: SaveFormats, GameInfo, FileFormats
- **Legacy Settings**: Extensive legacy properties from VS 2010 era:
  - `ExpressionBlendVersion=4.0.20525.0` (obsolete)
  - `SolutionDir` (obsolete)
  - `RestorePackages=true` (obsolete)
  - `Platform Condition` (x86)
  - Hard-coded `CodeAnalysisRuleSetDirectories` (VS 2010 paths)
  - Hard-coded `CodeAnalysisRuleDirectories` (VS 2010 paths)
  - Multiple platform-specific PropertyGroups with duplicated CodeAnalysis settings
  - `ImportWindowsDesktopTargets=true` (may be obsolete with modern SDK)
  - `<AppDesigner Include="Properties\" />` (obsolete)
  - Custom build target `AfterResolveReferences` (embeds referenced DLLs as resources)
  - `GenerateAssemblyInfo=false`
  - `LangVersion=preview`
  - `UseWPF=true`

**Current .csproj** (observed - partial):
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0-windows10.0.17763.0</TargetFramework>
    <Platform Condition=" '$(Platform)' == '' ">x86</Platform>
    <OutputType>WinExe</OutputType>
    <ExpressionBlendVersion>4.0.20525.0</ExpressionBlendVersion>
    <SolutionDir Condition="$(SolutionDir) == '' Or $(SolutionDir) == '*Undefined*'">..\</SolutionDir>
    <RestorePackages>true</RestorePackages>
    <GenerateAssemblyInfo>false</GenerateAssemblyInfo>
    <UseWPF>true</UseWPF>
    <ImportWindowsDesktopTargets>true</ImportWindowsDesktopTargets>
    <Configurations>Debug;Release;SaveEdit Packaging</Configurations>
  </PropertyGroup>
  <PropertyGroup>
    <ApplicationIcon>Resources\app.ico</ApplicationIcon>
  </PropertyGroup>
  <PropertyGroup>
    <StartupObject>Gibbed.MassEffectAndromeda.SaveEdit.Startup</StartupObject>
    <LangVersion>preview</LangVersion>
  </PropertyGroup>
  <PropertyGroup Condition="'$(Configuration)|$(Platform)' == 'SaveEdit Packaging|x86'">
    <!-- Hard-coded VS 2010 paths for CodeAnalysis -->
    <CodeAnalysisRuleSetDirectories>;C:\Program Files (x86)\Microsoft Visual Studio 10.0\Team Tools\Static Analysis Tools\\Rule Sets</CodeAnalysisRuleSetDirectories>
    <CodeAnalysisRuleDirectories>;C:\Program Files (x86)\Microsoft Visual Studio 10.0\Team Tools\Static Analysis Tools\FxCop\\Rules</CodeAnalysisRuleDirectories>
    <!-- More legacy settings -->
  </PropertyGroup>
  <PropertyGroup Condition="'$(Configuration)|$(Platform)' == 'SaveEdit Packaging|AnyCPU'">
    <OutputPath>..\..\bin_saveedit\</OutputPath>
    <!-- Duplicated hard-coded VS 2010 paths -->
  </PropertyGroup>
  <ItemGroup>
    <AppDesigner Include="Properties\" />
  </ItemGroup>
  <ItemGroup>
    <!-- Resources, ProjectReferences, PackageReferences -->
  </ItemGroup>
  <ItemGroup Label="Compile items now included by globbing that were not in the original project file">
    <Compile Remove="Helpers\MyClipboard.cs" />
  </ItemGroup>
  <Target Name="AfterResolveReferences">
    <ItemGroup>
      <EmbeddedResource Include="@(ReferenceCopyLocalPaths)" Condition="'%(ReferenceCopyLocalPaths.Extension)' == '.dll'">
        <LogicalName>%(ReferenceCopyLocalPaths.DestinationSubDirectory)%(ReferenceCopyLocalPaths.Filename)%(ReferenceCopyLocalPaths.Extension)</LogicalName>
      </EmbeddedResource>
    </ItemGroup>
  </Target>
</Project>
```

**Target State**: Clean WPF application with minimal SDK-style properties

**Migration Steps**:

1. **Prerequisites**
   - All other tiers (1-4a and UnpackResources) must be complete and validated
   - Verify project builds against cleaned dependencies
   - Create backup of current .csproj
   - Understand custom build target purpose (embeds DLLs as resources for single-file deployment)

2. **Incremental Cleanup (High-Risk - Do in Stages)**

   **Stage 1: Remove Obsolete Properties**

   | Property | Action | Reason |
   |----------|--------|--------|
   | `ExpressionBlendVersion` | **Remove** | Obsolete; Blend version not used by modern SDKs |
   | `SolutionDir` | **Remove** | Obsolete; SDK resolves automatically |
   | `RestorePackages` | **Remove** | Obsolete; SDK restores automatically |
   | `ImportWindowsDesktopTargets` | **Remove** | Obsolete; `UseWPF=true` handles this in modern SDK |
   | `<AppDesigner>` element | **Remove** | Obsolete; SDK handles Properties folder automatically |
   | Hard-coded `CodeAnalysisRuleSetDirectories` | **Remove** | VS 2010 paths don't exist; modern SDK handles analyzers |
   | Hard-coded `CodeAnalysisRuleDirectories` | **Remove** | VS 2010 paths don't exist; modern SDK handles analyzers |

   **Build and test** after Stage 1.

   **Stage 2: Consolidate PropertyGroups**

   Current .csproj has 4 separate `<PropertyGroup>` elements. Consolidate into logical groups:
   - Main properties (TargetFramework, OutputType, etc.)
   - Configuration-specific properties (SaveEdit Packaging output path)

   **Build and test** after Stage 2.

   **Stage 3: Update Language and Assembly Settings**

   | Property | Action | Reason |
   |----------|--------|--------|
   | `LangVersion=preview` | **Change to `latest`** | Consistency |
   | `GenerateAssemblyInfo=false` | **Check AssemblyInfo.cs** | Keep if exists |

   **Build and test** after Stage 3.

3. **Custom Build Target Review**

   **`AfterResolveReferences` Target**:
   - Purpose: Embeds all referenced DLLs as embedded resources (for single-file deployment or ILMerge-like behavior)
   - Decision:
     - If SaveEdit requires single-file deployment: **Keep target** (modern alternative: use `PublishSingleFile=true`)
     - If functionality still needed: **Keep target**
     - If no longer needed: **Remove target**

   **Investigation Required**: Test SaveEdit deployment to determine if custom target still needed.

   **Modern Alternative**: Consider replacing with:
   ```xml
   <PropertyGroup>
     <PublishSingleFile>true</PublishSingleFile>
     <SelfContained>false</SelfContained>
     <IncludeAllContentForSelfExtract>true</IncludeAllContentForSelfExtract>
   </PropertyGroup>
   ```

4. **Expected Breaking Changes**

   **Possible**:
   - Removing `ImportWindowsDesktopTargets` may break build (unlikely with `UseWPF=true` present)
   - Removing custom build target may break deployment if embedded resources are expected at runtime
   - Removing hard-coded CodeAnalysis paths should not break (those paths don't exist on modern systems)

   **Mitigation**: Test incrementally, keep backups, revert specific property if breaks

5. **Code Modifications**

   **Expected .csproj after cleanup**:
   ```xml
   <Project Sdk="Microsoft.NET.Sdk">
     <PropertyGroup>
       <TargetFramework>net10.0-windows10.0.17763.0</TargetFramework>
       <Platform Condition=" '$(Platform)' == '' ">x86</Platform>
       <OutputType>WinExe</OutputType>
       <GenerateAssemblyInfo>false</GenerateAssemblyInfo><!-- If AssemblyInfo.cs exists -->
       <UseWPF>true</UseWPF>
       <Configurations>Debug;Release;SaveEdit Packaging</Configurations>
       <ApplicationIcon>Resources\app.ico</ApplicationIcon>
       <StartupObject>Gibbed.MassEffectAndromeda.SaveEdit.Startup</StartupObject>
       <LangVersion>latest</LangVersion>
     </PropertyGroup>
     <PropertyGroup Condition="'$(Configuration)|$(Platform)' == 'SaveEdit Packaging|AnyCPU'">
       <OutputPath>..\..\bin_saveedit\</OutputPath>
     </PropertyGroup>
     <ItemGroup>
       <!-- Resources, ProjectReferences, PackageReferences -->
     </ItemGroup>
     <ItemGroup Label="Compile items now included by globbing that were not in the original project file">
       <Compile Remove="Helpers\MyClipboard.cs" />
     </ItemGroup>
     <!-- Keep or remove custom build target based on testing -->
     <Target Name="AfterResolveReferences">
       <ItemGroup>
         <EmbeddedResource Include="@(ReferenceCopyLocalPaths)" Condition="'%(ReferenceCopyLocalPaths.Extension)' == '.dll'">
           <LogicalName>%(ReferenceCopyLocalPaths.DestinationSubDirectory)%(ReferenceCopyLocalPaths.Filename)%(ReferenceCopyLocalPaths.Extension)</LogicalName>
         </EmbeddedResource>
       </ItemGroup>
     </Target>
   </Project>
   ```

6. **Testing Strategy**

   **After Each Stage**:
   - Build project
   - Verify zero errors, zero warnings
   - Run SaveEdit application
   - Test core functionality:
     - Load save file
     - Edit save data
     - Save file
     - Verify UI renders correctly
     - Verify resources load (icons, images)

   **Deployment Test** (for custom build target decision):
   - Build in "SaveEdit Packaging" configuration
   - Check output directory (..\..\bin_saveedit\)
   - Verify all dependencies present or embedded
   - Run from output directory

   **Analyzer Validation**:
   - Run `dotnet format`
   - Review warnings (WPF may have specific analyzer warnings)
   - Apply safe fixes

   **Regression Test**:
   - Compare with previous version to ensure no visual or functional regressions

7. **Validation Checklist**
   - [ ] Stage 1 complete: Obsolete properties removed, builds successfully
   - [ ] Stage 2 complete: PropertyGroups consolidated, builds successfully
   - [ ] Stage 3 complete: LangVersion updated, builds successfully
   - [ ] Custom build target decision documented (keep or remove)
   - [ ] Application executes successfully
   - [ ] WPF UI renders correctly
   - [ ] Core functionality validated (load/edit/save)
   - [ ] Resources load correctly (icons, images)
   - [ ] Deployment configuration validated (SaveEdit Packaging)
   - [ ] Analyzer clean (or warnings documented)
   - [ ] Changes committed with detailed message per stage

**Risk Level**: High (WPF with extensive legacy settings, custom build target)

**Complexity**: High (requires incremental testing, deep understanding of WPF and custom targets)

---

## Risk Management

### High-Risk Changes

| Project | Risk Level | Description | Mitigation |
|---------|-----------|-------------|------------|
| Gibbed.MassEffectAndromeda.SaveEdit | High | WPF application with extensive legacy CodeAnalysis settings, hard-coded VS 2010 paths, custom build targets | Clean incrementally, test WPF functionality thoroughly, verify custom build target still needed, keep legacy settings in comments initially |
| Gibbed.Frostbite3.UnpackResources | High | Contains legacy `<Reference>` elements in SDK-style project (System.Configuration, System.Transactions, etc.) | Verify if references still needed, replace with PackageReference or remove if SDK includes them, test functionality |
| Gibbed.PortableExecutable | Medium | Custom `CodeAnalysisRuleSet=AllRules.ruleset` | Verify ruleset file exists, consider modern .editorconfig alternative, ensure ruleset compatible with .NET 10 |
| All Projects | Low | `GenerateAssemblyInfo=false` may hide issues | Investigate why disabled, consider re-enabling with appropriate attributes |
| All Projects | Low | `LangVersion=preview` may introduce breaking changes | Verify preview features used, consider explicit version (e.g., `latest` or `12.0`) |

### Security Vulnerabilities

**None identified** in the assessment. All projects are SDK-style, and no vulnerable packages were flagged.

### Contingency Plans

#### If Removing Legacy Properties Breaks Build

**Scenario**: Removing a property causes build failure

**Alternatives**:
1. Investigate why property was needed
2. Check if SDK default behavior changed
3. Re-add property with comment explaining necessity
4. Research modern SDK-style alternative

**Example**: If removing `GenerateAssemblyInfo=false` causes duplicate attribute errors:
- Add explicit `<GenerateAssemblyInfo>false</GenerateAssemblyInfo>` back
- Or remove duplicate attributes from AssemblyInfo.cs files

#### If Removing Reference Elements Breaks UnpackResources

**Scenario**: Removing `<Reference Include="System.Configuration">` causes compilation errors

**Alternatives**:
1. Check if .NET 10 includes assembly by default (likely for System.* references)
2. Add as `<PackageReference>` if not included
3. Verify if functionality still needed (legacy code removal opportunity)

**Rollback**: Revert .csproj changes, commit as-is with comment, investigate separately

#### If WPF SaveEdit Breaks After Cleanup

**Scenario**: Removing legacy properties or custom targets breaks WPF application

**Alternatives**:
1. Restore specific property/target that broke
2. Test incrementally (remove one property at a time)
3. Verify `UseWPF=true` and `ImportWindowsDesktopTargets=true` behavior
4. Check if custom `AfterResolveReferences` target still needed for embedded resources

**Rollback**: Keep working version, file issue for investigation

#### If Analyzers Fail or Produce Too Many Warnings

**Scenario**: Running analyzers after cleanup produces hundreds of warnings

**Alternatives**:
1. Apply auto-fixes where safe
2. Suppress non-critical warnings via .editorconfig
3. Address critical warnings only in this migration
4. Create follow-up tasks for code quality improvements

**Decision Criteria**: Analyzer warnings should not block SDK-style cleanup unless they indicate actual build/runtime issues

### Bottom-Up Strategy Risk Factors

**Tier-Specific Risks**:

- **Tier 1 Risk**: If Gibbed.IO cleanup breaks, all 18 dependent projects affected
  - **Mitigation**: Extensive testing of Tier 1, validate all dependents build before proceeding

- **Tier 2-3 Risk**: Batch cleanup may miss project-specific nuances
  - **Mitigation**: Review each project's .csproj before cleanup, document differences

- **Tier 4b Risk**: High-complexity projects may require extended investigation
  - **Mitigation**: Allocate more time, clean incrementally, extensive testing

**Between-Tier Validation Risks**:

- **Risk**: Tier N+1 cleanup reveals issue in Tier N
  - **Mitigation**: Re-validate lower tiers if issues found, update plan accordingly

---

## Testing & Validation Strategy

### Phase-by-Phase Testing Requirements

#### Phase 1: Foundation Cleanup (Tier 1 - Gibbed.IO)

**Smoke Tests**:
- Build Gibbed.IO: `dotnet build projects\Gibbed.IO\Gibbed.IO.csproj`
- Verify zero errors, zero warnings
- Run analyzer: `dotnet format projects\Gibbed.IO\Gibbed.IO.csproj --verify-no-changes`

**Comprehensive Validation**:
- Build entire solution to validate all 18 dependents: `dotnet build "Mass Effect Andromeda.sln"`
- Verify no compilation errors or warnings introduced
- Run solution-level tests (if any)

**Success Criteria**:
- Gibbed.IO builds cleanly
- All dependent projects build successfully
- No new warnings introduced

---

#### Phase 2: Core Libraries Cleanup (Tier 2)

**Smoke Tests** (per project):
- Build each of 7 projects individually
- Verify builds succeed with zero errors

**Comprehensive Validation** (tier-level):
- Build all Tier 2 projects together
- Build Tier 3 projects (regression check - ensure cleaned Tier 2 doesn't break dependents)
- Run `dotnet format` on each project, apply fixes, commit

**Success Criteria**:
- All 7 Tier 2 projects build cleanly
- Tier 3 projects still build (no regressions)
- Analyzers run cleanly or warnings documented

---

#### Phase 3: Mid-Tier Cleanup (Tier 3)

**Smoke Tests** (per project):
- Build each of 5 projects individually
- Special: Verify PortableExecutable custom ruleset applies correctly

**Comprehensive Validation** (tier-level):
- Build all Tier 3 projects together
- Build Tier 4 projects (regression check)
- Run `dotnet format` on each project

**Success Criteria**:
- All 5 Tier 3 projects build cleanly
- PortableExecutable: Custom ruleset validated (or removed if file missing)
- Tier 4 projects still build (no regressions)
- Analyzers run cleanly

---

#### Phase 4: Simple Applications Cleanup (Tier 4a)

**Smoke Tests** (per project):
- Build each of 7 console applications individually
- Run each application with test arguments (if applicable)
- Verify output written to `..\..\bin\` directory

**Comprehensive Validation** (tier-level):
- Build all Tier 4a projects together
- Execute each console application to verify runtime behavior
- Verify legacy `<Reference>` removal didn't break compilation
- Run `dotnet format` on each project

**Execution Tests** (per application):
- Gibbed.Frostbite3.UnpackInitFS: Run with sample data, verify unpacking works
- Gibbed.Frostbite3.UnpackPartitions: Run with sample data, verify unpacking works
- Other tools: Run with `--help` or test data to ensure no runtime errors

**Success Criteria**:
- All 7 applications build cleanly
- All applications execute without errors
- Output paths correct (..\..\bin\)
- Legacy references successfully removed
- Analyzers run cleanly

---

#### Phase 5: Complex Applications Cleanup (Tier 4b)

**UnpackResources Testing**:
1. **Build Validation**:
   - Build after removing legacy references
   - If compilation errors, add necessary PackageReferences
2. **Execution Test**:
   - Run UnpackResources.exe with test data
   - Verify unpacking functionality works correctly
   - Compare output with previous version (regression test)
3. **Analyzer Validation**:
   - Run `dotnet format`, apply fixes

**SaveEdit Testing** (Incremental per Stage):

**Stage 1 Testing** (Obsolete Properties Removed):
- Build project
- Run SaveEdit application
- Quick UI check (application launches, UI renders)

**Stage 2 Testing** (PropertyGroups Consolidated):
- Build project
- Run SaveEdit application
- Full UI check (all windows, dialogs render correctly)

**Stage 3 Testing** (LangVersion Updated):
- Build project
- Run SaveEdit application
- Functional test: Load save file, edit, save, verify integrity

**Final SaveEdit Testing**:
- Build in all configurations (Debug, Release, SaveEdit Packaging)
- Run from output directory (..\..\bin_saveedit\)
- Full regression test:
  - Load various save files
  - Edit different save data fields
  - Save and verify file integrity
  - Test all UI features (tabs, dialogs, resources)
  - Verify icons and images load correctly
- Deployment test (if custom build target kept):
  - Check if DLLs embedded as resources
  - Run from clean directory (no dependency DLLs)

**Success Criteria**:
- UnpackResources builds and executes correctly
- SaveEdit builds in all configurations
- SaveEdit UI fully functional (no visual/functional regressions)
- SaveEdit deployment works (SaveEdit Packaging configuration)
- Custom build target decision documented (keep or remove)
- Analyzers run cleanly (or warnings documented for WPF-specific issues)

---

### Solution-Level Validation

**After All Phases Complete**:

1. **Full Solution Build**:
   - `dotnet build "Mass Effect Andromeda.sln"` in all configurations
   - Verify zero errors, document acceptable warnings

2. **Solution-Level Format**:
   - Run `dotnet format "Mass Effect Andromeda.sln" --verify-no-changes`
   - If changes needed, apply and commit

3. **Regression Test Suite** (if applicable):
   - Run any existing unit/integration tests
   - Verify all tests pass

4. **Smoke Test All Applications**:
   - Run each of 8 application projects
   - Verify basic functionality
   - Document any issues

5. **Build Artifacts Check**:
   - Verify output directories contain expected files
   - Verify no missing dependencies
   - Verify custom output paths honored (bin, bin_saveedit)

---

### Tier Validation Checkpoints (Bottom-Up Strategy)

**Between-Tier Validation**:

After completing each tier cleanup:
1. **Build Current Tier**: Verify all projects in tier build successfully
2. **Build Lower Tiers**: Verify no regressions in already-cleaned tiers (re-build Tiers 1 through current)
3. **Build Next Tier**: Verify next tier still builds with cleaned dependencies (if applicable)
4. **Analyzer Check**: Run analyzers on current tier, apply fixes
5. **Commit Checkpoint**: Commit tier cleanup with clear message (e.g., "Tier 2: Clean legacy properties from core libraries")

**Checkpoint Criteria for Proceeding to Next Tier**:
- [ ] All projects in current tier build successfully
- [ ] All lower tiers still build (no regressions)
- [ ] Analyzers run cleanly or warnings documented
- [ ] Changes committed
- [ ] No blocking issues identified

**If Checkpoint Fails**:
- Investigate issue
- Revert problematic changes if needed
- Document issue and workaround
- Decide whether to block tier progression or proceed with documented risk

---

### Validation Tools and Commands

**Build Commands**:
- Project-level: `dotnet build <project_path.csproj>`
- Solution-level: `dotnet build "Mass Effect Andromeda.sln"`
- Configuration-specific: `dotnet build "Mass Effect Andromeda.sln" -c "SaveEdit Packaging"`

**Analyzer Commands**:
- Verify: `dotnet format <path> --verify-no-changes`
- Apply fixes: `dotnet format <path>`

**Test Commands** (if tests exist):
- Run all tests: `dotnet test "Mass Effect Andromeda.sln"`
- Run specific test project: `dotnet test <test_project.csproj>`

**Restore Command** (if needed):
- `dotnet restore "Mass Effect Andromeda.sln"`

---

### Bottom-Up Strategy Testing Considerations

**Tier Dependency Validation**:
- Each tier tested against all lower tiers (cumulative validation)
- Higher tiers remain on old .csproj format until their turn (validated against cleaned lower tiers)

**Tier-Scoped Testing**:
- Tier 1-3: Build tests sufficient (libraries)
- Tier 4: Build + execution tests (applications)
- Tier 4b: Build + execution + functional tests (complex applications)

**Testing Parallelism**:
- Within a tier: Projects can be tested in parallel (if resources available)
- Between tiers: Sequential testing (Tier N+1 waits for Tier N completion)

---

## Complexity & Effort Assessment

### Per-Tier Complexity (Bottom-Up Strategy)

| Tier | Projects | Complexity Rating | Primary Dependencies | Risk Factors |
|------|----------|-------------------|---------------------|--------------|
| 1 | 1 (Gibbed.IO) | Low | None | Foundation for all others; extensive testing needed |
| 2 | 7 (Core libraries) | Low | Tier 1 only | Uniform legacy properties expected; batch operation |
| 3 | 5 (Mid-tier libraries) | Medium | Tiers 1-2 | May have more diverse legacy settings; PortableExecutable has custom ruleset |
| 4a | 7 (Simple apps) | Medium | Tiers 1-3 | Console applications; platform-specific properties |
| 4b | 2 (Complex apps) | High | Tiers 1-3 | UnpackResources: legacy References; SaveEdit: extensive WPF legacy settings |

### Phase Complexity Assessment

**Phase 1: Foundation Cleanup (Tier 1)**
- **Complexity**: Low
- **Effort**: Single project, straightforward audit and cleanup
- **Key Activities**: Audit legacy properties, remove obsolete settings, validate build/tests
- **Dependencies**: None (leaf node)
- **Success Criteria**: Gibbed.IO builds cleanly, all dependents still compile

**Phase 2: Core Libraries Cleanup (Tier 2)**
- **Complexity**: Low (batch operation)
- **Effort**: 7 projects, but uniform pattern expected
- **Key Activities**: Audit all 7 projects for legacy properties, batch cleanup, validate builds
- **Dependencies**: Tier 1 must be complete
- **Success Criteria**: All 7 projects build cleanly, no analyzer warnings

**Phase 3: Mid-Tier Cleanup (Tier 3)**
- **Complexity**: Medium
- **Effort**: 5 projects, may have diverse legacy settings
- **Key Activities**: Individual audit per project, batch cleanup, special attention to PortableExecutable ruleset
- **Dependencies**: Tiers 1-2 must be complete
- **Success Criteria**: All 5 projects build cleanly, custom ruleset validated

**Phase 4: Simple Applications Cleanup (Tier 4a)**
- **Complexity**: Medium
- **Effort**: 7 console applications, platform-specific considerations
- **Key Activities**: Audit console app settings, remove platform/packaging legacy properties, validate executables run
- **Dependencies**: Tiers 1-3 must be complete
- **Success Criteria**: All 7 applications build and execute correctly

**Phase 5: Complex Applications Cleanup (Tier 4b)**
- **Complexity**: High
- **Effort**: 2 projects requiring individual attention
- **Key Activities**: 
  - UnpackResources: Remove legacy Reference elements, verify functionality
  - SaveEdit: Incremental cleanup of extensive WPF legacy settings, thorough UI testing
- **Dependencies**: Tiers 1-4a must be complete
- **Success Criteria**: UnpackResources runs correctly; SaveEdit UI fully functional

### Resource Requirements

**Skill Levels Needed**:
- **.NET SDK knowledge**: Understanding SDK-style defaults vs legacy properties
- **MSBuild knowledge**: Understanding PropertyGroup, ItemGroup, custom targets
- **WPF knowledge**: For SaveEdit cleanup (Phase 5)
- **Testing capability**: Build, run, and validate applications

**Parallel Work Capacity**:
- **Tiers 1-4a**: Can be executed sequentially by single resource
- **Tier 4b**: May benefit from two resources (one for UnpackResources, one for SaveEdit)
- **Overall**: Sequential execution preferred due to dependency validation needs

### Relative Complexity Ratings

**Low Complexity** (Tiers 1, 2):
- Projects with minimal legacy settings
- Straightforward audit and cleanup
- High confidence in success
- Quick validation

**Medium Complexity** (Tiers 3, 4a):
- Projects with some unique legacy settings (custom rulesets, platform properties)
- Requires individual review
- Moderate confidence, may need iteration
- Standard validation

**High Complexity** (Tier 4b):
- Projects with extensive or unusual legacy settings
- Requires investigation and incremental cleanup
- Lower confidence, expect multiple iterations
- Extensive validation and testing needed

### Dependency Ordering Impact (Bottom-Up)

**Sequential Dependencies**:
- Each tier must complete before next begins
- No parallel tier execution
- Validation checkpoints between tiers

**Within-Tier Parallelism**:
- Tiers 2, 3, 4a: Projects can be cleaned in parallel (same tier)
- Tier 4b: Sequential recommended due to complexity

**Cumulative Effort**:
- Tier 1: ~5% of total effort
- Tier 2: ~20% of total effort
- Tier 3: ~20% of total effort
- Tier 4a: ~25% of total effort
- Tier 4b: ~30% of total effort (high complexity)

---

## Source Control Strategy

### Branching Strategy

**Main Branch**: Assumed to be `main` or `master` (current state)

**Working Branch**: `upgrade-to-NET10` (already exists and is current)

**Approach**: All SDK-style cleanup work will be performed on `upgrade-to-NET10` branch.

**Branch Protection**:
- Keep `upgrade-to-NET10` branch throughout migration
- Do not merge to main until all phases complete and validated
- Consider creating backup branch before starting: `backup-pre-sdk-cleanup`

**Merge Strategy**:
- After all 5 phases complete: Create PR from `upgrade-to-NET10` to `main`
- Squash commits or preserve tier structure based on team preference
- Require review and approval before merging

---

### Commit Strategy

**Commit Frequency**: One commit per tier cleanup, plus separate commits for analyzer fixes

**Tier Commit Pattern**:
1. **Tier Cleanup Commit**:
   - Message: `Tier X: Clean legacy properties from <tier description>`
   - Example: `Tier 1: Clean legacy properties from Gibbed.IO (foundation)`
   - Example: `Tier 2: Clean legacy properties from core libraries (7 projects)`
   - Includes all .csproj changes for that tier

2. **Analyzer Fixes Commit** (separate):
   - Message: `Tier X: Apply analyzer fixes`
   - Includes code formatting changes from `dotnet format`
   - Kept separate for easier review

**Commit Message Format**:
```
<Tier/Phase>: <Action> - <Scope>

- Removed: <list of removed properties>
- Changed: <list of changed properties>
- Kept: <list of kept properties with justification>

<Any issues encountered and resolutions>
```

**Example Commit Messages**:

```
Tier 1: Clean legacy properties from Gibbed.IO (foundation)

- Removed: OutputType=Library (SDK default)
- Changed: LangVersion from 'preview' to 'latest'
- Kept: GenerateAssemblyInfo=false (AssemblyInfo.cs exists)
- Kept: Configurations (solution requirement)

Validation: Builds cleanly, all 18 dependents validated
```

```
Tier 4b: Clean legacy properties from Gibbed.MassEffectAndromeda.SaveEdit (Stage 1)

- Removed: ExpressionBlendVersion, SolutionDir, RestorePackages, ImportWindowsDesktopTargets
- Removed: Hard-coded CodeAnalysisRuleSetDirectories and CodeAnalysisRuleDirectories
- Removed: <AppDesigner> element

Validation: Builds successfully, application launches, UI renders correctly
```

---

### Checkpoints and Milestones

**Checkpoint After Each Tier**:
1. Commit tier cleanup changes
2. Commit analyzer fixes (if any)
3. Tag tier completion (optional): `sdk-cleanup-tier-X-complete`
4. Document tier completion in tracking file or PR description

**Milestones**:
- Tier 1 complete: Foundation validated
- Tier 2 complete: 36% of projects cleaned
- Tier 3 complete: 59% of projects cleaned
- Tier 4a complete: 91% of projects cleaned
- Tier 4b complete: 100% of projects cleaned - **Ready for PR**

---

### Review and Merge Process

**Per-Tier Review** (Optional, for team visibility):
- After each tier commits, push to remote `upgrade-to-NET10`
- Team can review tier-by-tier changes
- Address feedback before proceeding to next tier

**Final PR Requirements**:
1. **PR Title**: `SDK-Style Cleanup: Remove legacy properties from all 22 projects`
2. **PR Description Template**:
   ```markdown
   ## Summary
   Cleaned legacy properties from all 22 projects in solution per SDK-style best practices.

   ## Changes
   - Removed obsolete properties: RestorePackages, SolutionDir, ExpressionBlendVersion, ImportWindowsDesktopTargets
   - Removed legacy <Reference> elements (replaced by SDK defaults or PackageReferences)
   - Updated LangVersion from 'preview' to 'latest' across all projects
   - Removed hard-coded VS 2010 CodeAnalysis paths

   ## Testing
   - All 22 projects build successfully
   - All 8 applications execute correctly
   - SaveEdit WPF application fully functional
   - Analyzer fixes applied

   ## Tier Breakdown
   - Tier 1 (Foundation): 1 project
   - Tier 2 (Core Libraries): 7 projects
   - Tier 3 (Mid-Tier): 5 projects
   - Tier 4a (Simple Apps): 7 projects
   - Tier 4b (Complex Apps): 2 projects

   ## Validation
   - [x] All tiers validated per Bottom-Up strategy
   - [x] Solution builds in all configurations
   - [x] Applications tested and functional
   - [x] Analyzer warnings addressed
   ```

3. **PR Checklist** (before merging):
   - [ ] All tier commits present
   - [ ] Solution builds successfully
   - [ ] All applications tested
   - [ ] Analyzer warnings addressed or documented
   - [ ] No regressions identified
   - [ ] Review approved by team

**Merge Criteria**:
- All validation checkpoints passed
- Team review approved
- No blocking issues
- All tests passing (if test suite exists)

---

### Rollback Strategy

**If Issues Discovered After Tier Commit**:
1. Identify problematic commit
2. `git revert <commit_hash>` to undo tier changes
3. Investigate and fix issue
4. Re-apply tier cleanup with fix

**If Issues Discovered After PR Merge**:
1. Create hotfix branch from `main`
2. Revert specific problematic changes
3. Test and validate fix
4. Create PR for hotfix

**Full Rollback** (if catastrophic failure):
1. Revert entire `upgrade-to-NET10` branch to backup point
2. Or: Restore from `backup-pre-sdk-cleanup` branch
3. Re-plan and re-execute with lessons learned

---

### Bottom-Up Strategy Source Control Considerations

**Tier Commits**:
- Each tier is a distinct commit (or commit pair: cleanup + analyzer fixes)
- Allows easy identification of which tier introduced an issue
- Supports incremental rollback (revert Tier 4b without affecting Tier 1-4a)

**Dependency Validation in Commits**:
- Commit message includes validation of dependents
- Example: "Tier 1 validated against all 18 dependents"

**Branching Discipline**:
- No merging from other branches into `upgrade-to-NET10` during migration
- Keep migration branch focused on SDK-style cleanup only

---

## Success Criteria

### Technical Criteria

**All Projects Cleaned**:
- [ ] All 22 projects have legacy properties removed or justified
- [ ] All projects use SDK-style format with minimal properties
- [ ] No obsolete properties remain (RestorePackages, SolutionDir, ExpressionBlendVersion, etc.)

**Language Version Standardized**:
- [ ] All projects use `LangVersion=latest` instead of `LangVersion=preview`
- [ ] Consistent language version across solution

**Legacy References Removed**:
- [ ] No legacy `<Reference>` elements for SDK-included assemblies (System.Data.DataSetExtensions, Microsoft.CSharp)
- [ ] System.Configuration and System.Transactions replaced with PackageReferences if needed

**Builds Pass**:
- [ ] Solution builds successfully: `dotnet build "Mass Effect Andromeda.sln"` exits with code 0
- [ ] All configurations build (Debug, Release, SaveEdit Packaging)
- [ ] All platforms build (x86, AnyCPU)
- [ ] Zero build errors

**Warnings Addressed**:
- [ ] Build warnings reviewed and addressed or documented as acceptable
- [ ] No new warnings introduced by cleanup
- [ ] Analyzer warnings addressed or suppressed with justification

**Applications Functional**:
- [ ] All 8 application projects execute without runtime errors
- [ ] SaveEdit WPF application fully functional (UI, load/save operations)
- [ ] Console applications produce expected output
- [ ] Output paths honored (bin, bin_saveedit directories)

---

### Quality Criteria

**Code Quality Maintained**:
- [ ] Analyzer fixes applied via `dotnet format`
- [ ] Code formatting consistent across solution
- [ ] No code functionality changes (only .csproj cleanup)

**Documentation Updated**:
- [ ] Plan.md completed and accurate
- [ ] Commit messages document changes per tier
- [ ] Any kept legacy properties have justification documented (in .csproj comments or commit messages)

**Test Coverage Maintained**:
- [ ] All existing tests still pass (if test suite exists)
- [ ] No tests removed or disabled
- [ ] Test coverage percentage unchanged

**No Regressions**:
- [ ] SaveEdit: Load/save functionality works
- [ ] SaveEdit: UI renders correctly (all dialogs, resources)
- [ ] Console apps: Execution successful with test data
- [ ] Build times not significantly increased
- [ ] Output artifacts match previous structure

---

### Process Criteria

**Bottom-Up Strategy Followed**:
- [ ] Tiers cleaned in order: 1 → 2 → 3 → 4a → 4b
- [ ] No tier skipped
- [ ] Each tier validated before proceeding to next

**Dependency Ordering Respected**:
- [ ] Lower tiers cleaned before dependent higher tiers
- [ ] No circular dependency issues introduced
- [ ] Each tier validated against cleaned lower tiers

**Source Control Strategy Followed**:
- [ ] All work on `upgrade-to-NET10` branch
- [ ] One commit per tier cleanup
- [ ] Separate commits for analyzer fixes
- [ ] Commit messages follow format
- [ ] Tier checkpoints documented

**Validation Checkpoints Passed**:
- [ ] Tier 1 checkpoint: Gibbed.IO validated, all dependents build
- [ ] Tier 2 checkpoint: 7 core libraries validated, Tier 3 still builds
- [ ] Tier 3 checkpoint: 5 mid-tier libraries validated, Tier 4 still builds
- [ ] Tier 4a checkpoint: 7 simple apps validated and execute correctly
- [ ] Tier 4b checkpoint: UnpackResources and SaveEdit fully functional

**Testing Strategy Followed**:
- [ ] Smoke tests executed per tier
- [ ] Comprehensive validation per tier
- [ ] Between-tier regression checks performed
- [ ] Solution-level validation performed after all tiers complete

---

### Deliverables

**Code Changes**:
- [ ] All 22 .csproj files cleaned and committed
- [ ] Analyzer fixes applied and committed
- [ ] All changes on `upgrade-to-NET10` branch

**Documentation**:
- [ ] Plan.md complete and accurate
- [ ] Commit history documents tier-by-tier progress
- [ ] PR description summarizes changes and validation

**Validation Artifacts**:
- [ ] Build logs showing successful builds
- [ ] Test results (if applicable)
- [ ] Execution test results for applications

**Ready for Review**:
- [ ] PR created from `upgrade-to-NET10` to `main`
- [ ] PR description complete with tier breakdown
- [ ] All checkboxes in PR checklist marked
- [ ] Ready for team review and approval

---

### Definition of Done

**Migration is complete when**:

1. ✅ All technical criteria met (builds pass, applications functional)
2. ✅ All quality criteria met (code quality maintained, no regressions)
3. ✅ All process criteria met (Bottom-Up strategy followed, validation checkpoints passed)
4. ✅ All deliverables produced (code changes, documentation, validation artifacts)
5. ✅ PR created and ready for review
6. ✅ Team review approved
7. ✅ Changes merged to `main` branch

**Post-Merge Validation**:
- [ ] `main` branch builds successfully after merge
- [ ] CI/CD pipeline passes (if applicable)
- [ ] No issues reported by team in first 24-48 hours

---

### Acceptance Criteria Summary

| Category | Criteria | Status |
|----------|----------|--------|
| **Build** | Solution builds in all configurations | ☐ |
| **Build** | Zero build errors | ☐ |
| **Warnings** | Build warnings addressed or documented | ☐ |
| **Applications** | All 8 apps execute successfully | ☐ |
| **SaveEdit** | WPF application fully functional | ☐ |
| **Properties** | All legacy properties removed or justified | ☐ |
| **LangVersion** | All projects use `latest` instead of `preview` | ☐ |
| **References** | Legacy `<Reference>` elements removed | ☐ |
| **Strategy** | Bottom-Up tiers completed in order | ☐ |
| **Validation** | All tier checkpoints passed | ☐ |
| **Testing** | Smoke and comprehensive tests passed | ☐ |
| **Source Control** | Commit strategy followed, PR ready | ☐ |
| **Documentation** | Plan.md complete, commits documented | ☐ |
| **Regressions** | No functional or visual regressions | ☐ |

**Overall Success**: All checkboxes marked ✅ and PR merged to `main`.

---

### Bottom-Up Strategy Specific Success Criteria

**Tier Completion**:
- [ ] Each tier completed before next tier started
- [ ] Tier validation checkpoints documented
- [ ] No tier rollbacks required (or rollbacks documented and resolved)

**Cumulative Validation**:
- [ ] Each tier validated against all lower tiers
- [ ] No regressions introduced in lower tiers by higher tier cleanups

**Incremental Benefits Realized**:
- [ ] Tier 1 complete: Foundation clean
- [ ] Tier 2 complete: 36% of projects clean
- [ ] Tier 3 complete: 59% of projects clean
- [ ] Tier 4a complete: 91% of projects clean
- [ ] Tier 4b complete: 100% of projects clean
