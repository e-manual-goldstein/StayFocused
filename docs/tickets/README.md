# Epic tickets index

Catalogue of epics for **StayFocused** — a Windows tray application that prompts daily *"What did you work on today?"* and creates Azure DevOps Task work items.

Agents: read `agent-methodology/instructions.md` and work from `docs/backlog.md`. Say **Next** to advance the active queue.

## Primary goal

**WRK** — Continuously running tray app → daily lean dialog → OK creates Azure DevOps **Task** with user text + mandatory fields from `appsettings.json`. Cancel closes until next day.

## Status vocabulary

| Status | Meaning |
| ------ | ------- |
| Todo | Not started |
| In Progress | Active work |
| Done | Shipped and verified |
| Blocked | Cannot proceed — see ticket notes |
| Shelved | Paused or rejected approach |
| Cancelled | Will not implement |
| Idea | Unprioritized |

## Epics

| Code | File | Scope | Status |
| ---- | ---- | ----- | ------ |
| **WRK** | [WRK-daily-work-log.md](WRK-daily-work-log.md) | **Primary** — daily prompt + Azure DevOps Task | Active (0%) |
| FND | [FND-foundation.md](FND-foundation.md) | Tray app, DI, config (partially reused) | In progress (75%) |
| MON | [MON-monitoring.md](MON-monitoring.md) | Activity polling | Shelved |
| HND | [HND-handlers.md](HND-handlers.md) | Per-process handlers | Shelved |
| UI | [UI-daily-summary.md](UI-daily-summary.md) | Activity summary window | Shelved |
| PLG | [PLG-plugins.md](PLG-plugins.md) | Plugin loading | Shelved |
| API | [API-contracts.md](API-contracts.md) | Namespace cleanup | Shelved |
| IDE | [IDE-ideas.md](IDE-ideas.md) | Ideas (all shelved) | Shelved |

## Related docs

- [COMPONENTS.md](../COMPONENTS.md) — component reference (legacy architecture map)
- [agent-methodology/instructions.md](../agent-methodology/instructions.md) — agent workflow

## Ticket ID format

`<EPIC>-<NNN>` — e.g. `WRK-001`, `WRK-005`.
