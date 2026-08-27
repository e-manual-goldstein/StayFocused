# Epic API — Contracts & Namespace Cleanup

**Project:** StayFocused
**Code:** `API`
**Scope:** Consolidate shared interfaces in `StayFocused.Api` with consistent namespaces and clear contracts for handlers, monitoring, and plugins.

**Depends on:** FND-001
**Blocks:** HND-001 (cleaner contracts)

---

## Primary user story

> As a developer (or plugin author), I have one clear API project with predictable namespaces when I reference StayFocused contracts.

---

## Ticket summary

| ID | Status | Title | Depends on |
|----|--------|-------|------------|
| [API-001](#api-001) | Shelved | Consolidate shared interfaces into StayFocused.Api namespaces |
| [API-002](#api-002) | Shelved | Align IActivityHandler with handler integration pattern |

---

## Design notes

### Current inconsistencies

| Type | Project | Namespace |
|------|---------|-----------|
| `IActivityMonitor`, `ILogManager` | StayFocused.Api | `StayFocused.Api` |
| `IActivity` | StayFocused.Api | `StayFocused` |
| `IActivityHandler`, `IPlugin`, `PluginDefinition` | StayFocused.Api | `StayFocused` |

### Target

- Public contracts: `StayFocused.Api` namespace (or documented split if intentional).
- Update all usings in main app and `ExampleModule`.
- No behaviour change — refactor only.

### API-002

After API-001, consider whether `IActivityHandler` should return enriched title only or a small result type (title + optional metadata). Keep minimal for v1.

### Out of scope

- Publishing NuGet package
- Versioning policy

---

## Tickets

### API-001

| Field | Detail |
|-------|--------|
| **ID** | API-001 |
| **Title** | Consolidate shared interfaces into StayFocused.Api namespaces |
| **Status** | Shelved |
| **Description** | Move interfaces to `StayFocused.Api` namespace. Shelved 2026-08-27 — not primary goal. |
| **Test / demo** | `dotnet build` succeeds; ExampleModule still loads; no duplicate type definitions. |
| **Depends on** | FND-001 |

### API-002

| Field | Detail |
|-------|--------|
| **ID** | API-002 |
| **Title** | Align IActivityHandler with handler integration pattern |
| **Status** | Shelved |
| **Description** | Document `IActivityHandler` contract. Shelved 2026-08-27 — not primary goal. |
| **Test / demo** | Handler returning null/empty → fallback title used; throwing handler → logged, fallback title used. |
| **Depends on** | API-001, HND-001 |
