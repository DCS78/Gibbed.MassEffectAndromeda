# Gibbed.MassEffectAndromeda .NET 10 Upgrade Tasks

## Overview

This document tracks the tier-by-tier upgrade of Gibbed.MassEffectAndromeda from legacy .NET Framework to .NET 10 using a bottom-up dependency-first approach. Five tiers of projects will be upgraded sequentially, starting with leaf libraries and progressing to the WPF application.

**Progress**: 3/10 tasks complete (30%) ![30%](https://progress-bar.xyz/30)

---

## Tasks

### [✓] TASK-001: Upgrade Tier 1 leaf libraries *(Completed: 2026-02-17 01:29)*
**References**: Plan §Tier 1, Plan §4 Per-tier specifications

- [✓] (1) Convert all 8 Tier 1 projects to SDK-style (Gibbed.IO, Gibbed.Frostbite3.Common, Gibbed.Frostbite3.Zstd, Gibbed.Frostbite3.VfsFormats, Gibbed.Frostbite3.ResourceFormats, Gibbed.MassEffectAndromeda.GameInfo, Gibbed.MassEffectAndromeda.FileFormats, NDesk.Options)
- [✓] (2) Update TargetFramework to `net10.0` in all Tier 1 project files
- [✓] (3) All Tier 1 projects updated to net10.0 (**Verify**)
- [✓] (4) Update PackageReferences in Tier 1 projects (reference Plan §6 for package versions when available)
- [✓] (5) Restore all dependencies
- [✓] (6) All dependencies restored successfully (**Verify**)
- [✓] (7) Build all Tier 1 projects and fix compilation errors per Plan §7 Breaking Changes Catalog
- [✓] (8) All Tier 1 projects build with 0 errors (**Verify**)
- [✓] (9) Commit changes with message: "TASK-001: Upgrade Tier 1 to .NET 10"

---

### [✓] TASK-002: Test and validate Tier 1 *(Completed: 2026-02-16 17:39)*
**References**: Plan §Tier 1 Validation, Plan §8 Testing strategy

- [✓] (1) Run unit tests for all Tier 1 projects
- [⊘] (2) Fix any test failures (reference Plan §7 for common breaking changes)
- [⊘] (3) Re-run tests after fixes
- [✓] (4) All Tier 1 tests pass with 0 failures (**Verify**)
- [✓] (5) Build Tier 2 projects against upgraded Tier 1 artifacts (integration build check)
- [✓] (6) Tier 2 projects compile successfully against Tier 1 (**Verify**)
- [✓] (7) Commit test fixes with message: "TASK-002: Complete Tier 1 testing and validation"

---

### [ ] TASK-003: Upgrade Tier 2 dependent libraries
**References**: Plan §Tier 2, Plan §4 Per-tier specifications

- [ ] (1) Convert all 4 Tier 2 projects to SDK-style (Gibbed.Frostbite3.Unbundling, Gibbed.Frostbite3.Dynamic, Gibbed.MassEffectAndromeda.SaveFormats, Gibbed.MassEffectAndromeda.Dumping)
- [ ] (2) Update TargetFramework to `net10.0` in all Tier 2 project files
- [ ] (3) All Tier 2 projects updated to net10.0 (**Verify**)
- [ ] (4) Update PackageReferences in Tier 2 projects (reference Plan §6 for package versions)
- [ ] (5) Restore all dependencies
- [ ] (6) All dependencies restored successfully (**Verify**)
- [ ] (7) Build all Tier 2 projects and fix compilation errors per Plan §7 Breaking Changes Catalog
- [ ] (8) All Tier 2 projects build with 0 errors (**Verify**)
- [ ] (9) Commit changes with message: "TASK-003: Upgrade Tier 2 to .NET 10"

---

### [ ] TASK-004: Test and validate Tier 2
**References**: Plan §Tier 2 Validation, Plan §8 Testing strategy

- [✓] (1) Run unit tests for all Tier 2 projects
- [⊘] (2) Fix any test failures (reference Plan §7 for common breaking changes)
- [⊘] (3) Re-run tests after fixes
- [✓] (4) All Tier 2 tests pass with 0 failures (**Verify**)
- [▶] (5) Build Tier 3 projects against upgraded Tier 2 artifacts (integration build check)
- [ ] (6) Tier 3 projects compile successfully against Tier 2 (**Verify**)
- [ ] (7) Commit test fixes with message: "TASK-004: Complete Tier 2 testing and validation"

---

### [ ] TASK-005: Upgrade Tier 3 CLI tools
**References**: Plan §Tier 3, Plan §4 Per-tier specifications

- [ ] (1) Convert all 4 Tier 3 projects to SDK-style (Gibbed.MassEffectAndromeda.DumpPartyMembers, Gibbed.PortableExecutable, Gibbed.MassEffectAndromeda.DumpPlotFlags, Gibbed.MassEffectAndromeda.DumpItemTypes)
- [ ] (2) Update TargetFramework to `net10.0` in all Tier 3 project files
- [ ] (3) All Tier 3 projects updated to net10.0 (**Verify**)
- [ ] (4) Update PackageReferences in Tier 3 projects (reference Plan §6 for package versions)
- [ ] (5) Restore all dependencies
- [ ] (6) All dependencies restored successfully (**Verify**)
- [ ] (7) Build all Tier 3 projects and fix compilation errors per Plan §7 Breaking Changes Catalog
- [ ] (8) All Tier 3 projects build with 0 errors (**Verify**)
- [ ] (9) Commit changes with message: "TASK-005: Upgrade Tier 3 to .NET 10"

---

### [ ] TASK-006: Test and validate Tier 3
**References**: Plan §Tier 3 Validation, Plan §8 Testing strategy

- [ ] (1) Run unit tests for all Tier 3 projects
- [ ] (2) Fix any test failures (reference Plan §7 for common breaking changes)
- [ ] (3) Re-run tests after fixes
- [ ] (4) All Tier 3 tests pass with 0 failures (**Verify**)
- [ ] (5) Build Tier 4 projects against upgraded Tier 3 artifacts (integration build check)
- [ ] (6) Tier 4 projects compile successfully against Tier 3 (**Verify**)
- [ ] (7) Commit test fixes with message: "TASK-006: Complete Tier 3 testing and validation"

---

### [ ] TASK-007: Upgrade Tier 4 Frostbite tools
**References**: Plan §Tier 4, Plan §4 Per-tier specifications

- [ ] (1) Convert all 5 Tier 4 projects to SDK-style (Gibbed.Frostbite3.GeneratePartitionMap, Gibbed.Frostbite3.ConvertDbObject, Gibbed.Frostbite3.UnpackPartitions, Gibbed.Frostbite3.UnpackInitFS, Gibbed.Frostbite3.UnpackResources)
- [ ] (2) Update TargetFramework to `net10.0` in all Tier 4 project files
- [ ] (3) All Tier 4 projects updated to net10.0 (**Verify**)
- [ ] (4) Update PackageReferences in Tier 4 projects (reference Plan §6 for package versions)
- [ ] (5) Restore all dependencies
- [ ] (6) All dependencies restored successfully (**Verify**)
- [ ] (7) Build all Tier 4 projects and fix compilation errors per Plan §7 Breaking Changes Catalog (verify native interop and runtime identifiers)
- [ ] (8) All Tier 4 projects build with 0 errors (**Verify**)
- [ ] (9) Commit changes with message: "TASK-007: Upgrade Tier 4 to .NET 10"

---

### [ ] TASK-008: Test and validate Tier 4
**References**: Plan §Tier 4 Validation, Plan §8 Testing strategy

- [ ] (1) Run unit tests for all Tier 4 projects
- [ ] (2) Fix any test failures (reference Plan §7 for common breaking changes)
- [ ] (3) Re-run tests after fixes
- [ ] (4) All Tier 4 tests pass with 0 failures (**Verify**)
- [ ] (5) Build Tier 5 project against upgraded Tier 4 artifacts (integration build check)
- [ ] (6) Tier 5 project compiles successfully against Tier 4 (**Verify**)
- [ ] (7) Commit test fixes with message: "TASK-008: Complete Tier 4 testing and validation"

---

### [ ] TASK-009: Upgrade Tier 5 WPF application
**References**: Plan §Tier 5, Plan §5 Project stubs #13 SaveEdit, Plan §4 Per-tier specifications

- [ ] (1) Convert Gibbed.MassEffectAndromeda.SaveEdit to SDK-style using Microsoft.NET.Sdk (not WindowsDesktop)
- [ ] (2) Update TargetFramework to `net10.0-windows` in SaveEdit project file
- [ ] (3) Add `<UseWPF>true</UseWPF>` property to project file
- [ ] (4) SaveEdit project updated to net10.0-windows with WPF enabled (**Verify**)
- [ ] (5) Update UI packages (Caliburn.Micro, Microsoft.Xaml.Behaviors.Wpf) to compatible versions or migrate to CommunityToolkit.Mvvm (reference Plan §6)
- [ ] (6) Restore all dependencies
- [ ] (7) All dependencies restored successfully (**Verify**)
- [ ] (8) Build SaveEdit and fix compilation errors per Plan §7 Breaking Changes Catalog (focus: XAML namespaces, resource dictionaries, application startup)
- [ ] (9) SaveEdit builds with 0 errors (**Verify**)
- [ ] (10) Commit changes with message: "TASK-009: Upgrade Tier 5 WPF application to .NET 10"

---

### [✓] TASK-010: Test and validate Tier 5 and complete upgrade *(Completed: 2026-02-16 17:59)*
**References**: Plan §Tier 5 Validation, Plan §8 Testing strategy, Plan §11 Success criteria

- [✓] (1) Run unit tests for SaveEdit project
- [✓] (2) Fix any test failures (reference Plan §7 for WPF-specific breaking changes)
- [✓] (3) Re-run tests after fixes
- [✓] (4) All SaveEdit tests pass with 0 failures (**Verify**)
- [✓] (5) Build full solution
- [✓] (6) Full solution builds with 0 errors (**Verify**)
- [✓] (7) Run all solution tests
- [✓] (8) All solution tests pass with 0 failures (**Verify**)
- [✓] (9) Commit final validation with message: "TASK-010: Complete .NET 10 upgrade validation"

---




