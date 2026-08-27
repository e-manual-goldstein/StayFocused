# Unfinished Ticket Backlog

Ordered list of **open** tickets across all epics. When a ticket is completed, add it to **Recently completed** (table below) and remove it from **Active**. That section shows **only the latest** completed ticket — replace the row when a new one lands. If you complete **multiple tickets in one batch** (same session/commit), list every ticket from that batch in the table instead.

**Source epics:** see [tickets/](tickets/README.md) for full acceptance criteria.

**Primary app:** standalone project [`DailyWorkLog/`](../../DailyWorkLog/) — not the legacy `StayFocused` activity monitor.

## Recently completed

| TicketId | Epic | Description |
| -------- | ---- | ----------- |
| ~~WRK-005~~ | [WRK](tickets/WRK-daily-work-log.md) | Daily scheduler — trigger prompt once per calendar day |
| ~~WRK-004~~ | [WRK](tickets/WRK-daily-work-log.md) | Wire OK to create Task with user text |
| ~~WRK-003~~ | [WRK](tickets/WRK-daily-work-log.md) | Daily prompt dialog — text field, OK, Cancel |
| ~~WRK-002~~ | [WRK](tickets/WRK-daily-work-log.md) | Azure DevOps Work Item API client (create Task) |
| ~~WRK-001~~ | [WRK](tickets/WRK-daily-work-log.md) | appsettings configuration for Azure DevOps and daily prompt |

## Active (recommended order)

| TicketId | Epic | Description |
| -------- | ---- | ----------- |
| — | — | WRK epic complete — configure `DailyWorkLog/appsettings.json` and run |

## Epic progress

In-progress epics only. **100%** completed epics are listed at the [end of this file](#completed-epics-100).

| Epic | Description | Tickets Completed | Tickets Shelved | Total Tickets | Progress |
| ---- | ----------- | ----------------- | --------------- | ------------- | -------- |
| [Foundation (FND)](tickets/FND-foundation.md) | Legacy StayFocused shell (deprioritised) | 3 | 1 | 4 | 🟩🟩🟩🟩🟩🟩🟩⬜⬜⬜ 75% |
| [Monitoring (MON)](tickets/MON-monitoring.md) | Activity polling (deprioritised) | 1 | 2 | 3 | 🟩🟩🟩🟨🟨⬜⬜⬜⬜⬜ 33% |
| [Handlers (HND)](tickets/HND-handlers.md) | Per-process handlers (deprioritised) | 0 | 4 | 4 | 🟨🟨🟨🟨⬜⬜⬜⬜⬜⬜ 0% |
| [Daily Summary UI (UI)](tickets/UI-daily-summary.md) | Activity summary view (deprioritised) | 1 | 2 | 4 | 🟩🟩🟨🟨⬜⬜⬜⬜⬜⬜ 25% |
| [Plugins (PLG)](tickets/PLG-plugins.md) | Plugin loading (deprioritised) | 1 | 1 | 2 | 🟩🟩🟩🟩🟩🟨⬜⬜⬜⬜ 50% |
| [API Contracts (API)](tickets/API-contracts.md) | Namespace cleanup (deprioritised) | 0 | 2 | 2 | 🟨🟨⬜⬜⬜⬜⬜⬜⬜⬜ 0% |

*Progress bar is always 10 squares: 🟩 completed, 🟨 shelved, ⬜ open.*

## Shelved

Not in the active queue. Legacy `StayFocused` scope — see individual epic files.

| TicketId | Epic | Description | Notes |
| -------- | ---- | ----------- | ----- |
| MON-002 | [MON](tickets/MON-monitoring.md) | Pause activity recording when session locked | Not primary goal |
| MON-003 | [MON](tickets/MON-monitoring.md) | File-based JSON persistence task | Not primary goal |
| HND-001–004 | [HND](tickets/HND-handlers.md) | Activity handlers | Not primary goal |
| UI-002, UI-003 | [UI](tickets/UI-daily-summary.md) | Summary UI improvements | Not primary goal |
| PLG-002 | [PLG](tickets/PLG-plugins.md) | Example plugin handler | Not primary goal |
| API-001, API-002 | [API](tickets/API-contracts.md) | Namespace cleanup | Not primary goal |
| FND-004 | [FND](tickets/FND-foundation.md) | Remove dead code | Not primary goal |
| IDE-001–004 | [IDE](tickets/IDE-ideas.md) | Ideas | Not primary goal |

## Cancelled

| TicketId | Epic | Description | Notes |
| -------- | ---- | ----------- | ----- |
| UI-004 | [UI](tickets/UI-daily-summary.md) | Complete CustomGridView alternative | Cancelled |

## Ideas

None active.

---

## Completed epics (100%)

| Epic | Description | Completed | Project |
| ---- | ----------- | --------- | ------- |
| [Daily Work Log (WRK)](tickets/WRK-daily-work-log.md) | Daily prompt → Azure DevOps Task | WRK-001 – WRK-005 | `DailyWorkLog/` |
