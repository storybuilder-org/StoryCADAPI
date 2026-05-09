# Proposal: Enforce NuGet Vulnerability Audit at Build Time

## Background

Uno.Sdk 6.5.31 transitively pulled in `Tmds.DBus.Protocol 0.21.2`, which
carries a high-severity advisory (GHSA-xrw6-gwf8-vvr9). Uno patched on
`master` but did not backport to the 6.5 line. The vulnerability only
surfaced because a developer happened to notice the VS2026 banner and
manually ran `dotnet list package --vulnerable --include-transitive`.

That is not a reliable detection mechanism. The pin in issue #1383 closes
*this* CVE; this document proposes a standing policy so the *next* CVE
doesn't take us by surprise.

## Goal

A vulnerable NuGet package — direct or transitive — should break the build
immediately, for every developer, on every compile. No CI check, no release
gate, no reminder memo.

## Proposed change

Add four properties to `Directory.Build.props` in an existing or new
`<PropertyGroup>`:

```xml
<NuGetAudit>true</NuGetAudit>
<NuGetAuditMode>all</NuGetAuditMode>
<NuGetAuditLevel>low</NuGetAuditLevel>
<WarningsAsErrors>$(WarningsAsErrors);NU1903;NU1904;NU1905</WarningsAsErrors>
```

What each does:

- `NuGetAudit=true` — enable the built-in NuGet audit (already default on
  .NET 8+, but explicit is safer).
- `NuGetAuditMode=all` — audit both direct and transitive packages. The
  default `direct` would have missed the Tmds.DBus case entirely.
- `NuGetAuditLevel=low` — report every severity level, not just high/critical.
  We want to know about moderate CVEs before they become high ones.
- `WarningsAsErrors=NU1903;NU1904;NU1905` — promote the three audit warnings
  (vulnerable package direct, vulnerable package transitive, deprecated
  package) from warnings to build errors.

## Why MSBuild-level and not CI-level

| Approach | Catches dev builds? | Catches release builds? | Depends on process? |
|----------|:-------------------:|:-----------------------:|:-------------------:|
| Memo / checklist | no | no | yes |
| CI workflow gate | no | yes | yes (CI must stay in place) |
| MSBuild `NuGetAudit` | **yes** | **yes** | no |

Every developer hits the failure on first local build. No reliance on Jake's
CI workflow, no reliance on reviewers remembering to run `--vulnerable`
during PR review. This is why the proposal lives at MSBuild level.

## Interaction with PR #1382 (CI/CD rework)

Zero overlap. PR #1382 modifies:

- `.github/workflows/ci.yml`
- `.github/workflows/store-publish.yml`
- `docs/For Developers/CI_CD_Pipeline.md`

This proposal modifies `Directory.Build.props`, which is build-infrastructure
at the MSBuild level and independent of any CI workflow.

## Prerequisites

1. Issue #1383's pin (Tmds.DBus.Protocol → 0.21.3) must be merged first.
   Otherwise this change breaks the build on day one.
2. Run `dotnet list package --vulnerable --include-transitive` on the
   post-pin tree to confirm zero vulnerabilities, before enabling the audit.

## When a future CVE breaks the build

The failing package name and advisory URL appear directly in the build
output. The developer has three realistic responses:

1. **Upgrade** the direct package to a version that no longer brings the
   vulnerable transitive in.
2. **Pin** the transitive to a patched version (same pattern as issue #1383).
3. **Suppress** the specific CVE with `<NuGetAuditSuppress>` — last resort,
   requires a comment explaining why and an exit condition.

All three are reversible.

## Risks

- **Unshipping a fix upstream.** A pin can hide the fact that the parent
  package already shipped a newer version. Periodic review of pins is
  required — covered by the exit-condition comment pattern established
  in issue #1383.
- **Build breakage from new CVEs on a Friday afternoon.** Mitigated by the
  fact that fixing a CVE is almost always a same-day version bump. Worst
  case, suppress temporarily with a tracked issue.
- **False positives.** NuGet advisory data occasionally flags packages that
  aren't actually reachable in our code paths (e.g., a Linux-only transitive
  on a Windows-only build). `<NuGetAuditSuppress>` handles these.

## Recommendation

Land this after issue #1383's pin lands on `dev`. Either a separate small PR
against `dev`, or fold into the pin PR itself — caller's choice.

## Out of scope for this proposal

- CI-level secondary check. Unnecessary given the MSBuild gate, but not
  harmful if Jake wants to add one for belt-and-suspenders.
- Container image vulnerability scanning (the TestDb Docker MySQL image is
  separate from NuGet).
- Source code static analysis (SAST) — a different category of tool.
