# Unfinished Ticket Backlog

Ordered list of **open** tickets across all epics. When a ticket is completed, add it to **Recently completed** (table below) and remove it from **Active**. That section shows **only the latest** completed ticket — replace the row when a new one lands. If you complete **multiple tickets in one batch** (same session/commit), list every ticket from that batch in the table instead. When a new ticket is created in any epic doc, add a row in the appropriate position in **Active**.

**Source epics:** see [tickets/](tickets/README.md) for full acceptance criteria.

**Component reference:** see [COMPONENTS.md](../COMPONENTS.md) for architecture overview.

**Primary goal:** daily *"What did you work on today?"* prompt → Azure DevOps Task ([WRK epic](tickets/WRK-daily-work-log.md)). All other epics shelved.

## Recently completed

| TicketId | Epic | Description |
| -------- | ---- | ----------- |
| — | — | No tickets completed under this workflow yet |

## Active (recommended order)

| TicketId | Epic | Description |
| -------- | ---- | ----------- |
| WRK-001 | [WRK](tickets/WRK-daily-work-log.md) | appsettings configuration for Azure DevOps and daily prompt |
| WRK-002 | [WRK](tickets/WRK-daily-work-log.md) | Azure DevOps Work Item API client (create Task) |
| WRK-003 | [WRK](tickets/WRK-daily-work-log.md) | Daily prompt dialog — text field, OK, Cancel |
| WRK-004 | [WRK](tickets/WRK-daily-work-log.md) | Wire OK to create Task with user text |
| WRK-005 | [WRK](tickets/WRK-daily-work-log.md) | Daily scheduler — trigger prompt once per calendar day |

## Epic progress

In-progress epics only. **100%** completed epics are listed at the [end of this file](#completed-epics-100).

| Epic | Description | Tickets Completed | Tickets Shelved | Total Tickets | Progress |
| ---- | ----------- | ----------------- | --------------- | ------------- | -------- |
| [Daily Work Log (WRK)](tickets/WRK-daily-work-log.md) | Daily prompt → Azure DevOps Task | 0 | 0 | 5 | ⬜⬜⬜⬜⬜⬜⬜⬜⬜⬜ 0% |
| [Foundation (FND)](tickets/FND-foundation.md) | App shell, DI, tray icon, config | 3 | 1 | 4 | 🟩🟩🟩🟩🟩🟩🟩⬜⬜⬜ 75% |
| [Monitoring (MON)](tickets/MON-monitoring.md) | Activity polling (deprioritised) | 1 | 2 | 3 | 🟩🟩🟩🟨🟨⬜⬜⬜⬜⬜ 33% |
| [Handlers (HND)](tickets/HND-handlers.md) | Per-process handlers (deprioritised) | 0 | 4 | 4 | 🟨🟨🟨🟨⬜⬜⬜⬜⬜⬜ 0% |
| [Daily Summary UI (UI)](tickets/UI-daily-summary.md) | Activity summary view (deprioritised) | 1 | 2 | 4 | 🟩🟩🟨🟨⬜⬜⬜⬜⬜⬜ 25% |
| [Plugins (PLG)](tickets/PLG-plugins.md) | Plugin loading (deprioritised) | 1 | 1 | 2 | 🟩🟩🟩🟩🟩🟨⬜⬜⬜⬜ 50% |
| [API Contracts (API)](tickets/API-contracts.md) | Namespace cleanup (deprioritised) | 0 | 2 | 2 | 🟨🟨⬜⬜⬜⬜⬜⬜⬜⬜ 0% |

*Progress bar is always 10 squares: 🟩 completed, 🟨 shelved, ⬜ open — each segment uses* `floor(count ÷ total × 10)`*; percentage shown is completed only.*

## Shelved

Not in the active queue. Shelved **2026-08-27** — focus shifted to daily Azure DevOps work log (WRK epic).

| TicketId | Epic | Description | Notes |
| -------- | ---- | ----------- | ----- |
| MON-002 | [MON](tickets/MON-monitoring.md) | Pause activity recording when session locked | Not primary goal |
| MON-003 | [MON](tickets/MON-monitoring.md) | File-based JSON persistence task | Superseded by SQLite; not primary goal |
| HND-001 | [HND](tickets/HND-handlers.md) | Wire handlers into ActivityMonitor | Not primary goal |
| HND-002 | [HND](tickets/HND-handlers.md) | Firefox activity handler | Not primary goal |
| HND-003 | [HND](tickets/HND-handlers.md) | Edge URL extraction via COM | Not primary goal |
| HND-004 | [HND](tickets/HND-handlers.md) | Outlook email subject handler | Not primary goal |
| UI-002 | [UI](tickets/UI-daily-summary.md) | Fix ActivityViewModel filter bugs | Not primary goal |
| UI-003 | [UI](tickets/UI-daily-summary.md) | Date range picker for summary | Not primary goal |
| PLG-002 | [PLG](tickets/PLG-plugins.md) | Example plugin registers handler | Not primary goal |
| API-001 | [API](tickets/API-contracts.md) | Consolidate Api namespaces | Not primary goal |
| API-002 | [API](tickets/API-contracts.md) | Align IActivityHandler contract | Not primary goal |
| FND-004 | [FND](tickets/FND-foundation.md) | Remove dead code (Startup, MainWindow) | Not primary goal |
| IDE-001 | [IDE](tickets/IDE-ideas.md) | Weekly/monthly summary reports | Idea — not primary goal |
| IDE-002 | [IDE](tickets/IDE-ideas.md) | Export selected rows to CSV | Idea — not primary goal |
| IDE-003 | [IDE](tickets/IDE-ideas.md) | Configurable monitoring interval | Idea — not primary goal |
| IDE-004 | [IDE](tickets/IDE-ideas.md) | Category tags for processes | Idea — not primary goal |

## Cancelled

Closed — will not be implemented.

| TicketId | Epic | Description | Notes |
| -------- | ---- | ----------- | ----- |
| UI-004 | [UI](tickets/UI-daily-summary.md) | Complete CustomGridView alternative | Cancelled — DailySummary column menus sufficient |

## Ideas

None active — see shelved IDE tickets above.

---

## Completed epics (100%)

None yet.
