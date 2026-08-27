# Epic UI — Daily Summary

**Project:** StayFocused
**Code:** `UI`
**Scope:** WPF **Daily Summary** window — today's activity grouped by process and window title, with sorting, filtering, and row selection.

**Depends on:** FND-001, MON-001
**Blocks:** —

---

## Primary user story

> At the end of the day I open Daily Summary from the tray and see how long I spent in each app and window, filter noise, and spot where my time went.

---

## Ticket summary

| ID | Status | Title | Depends on |
|----|--------|-------|------------|
| [UI-001](#ui-001) | Done | Daily summary ListView with sort and filter | FND-002, MON-001 |
| [UI-002](#ui-002) | Shelved | Fix ActivityViewModel filter bugs and dead code |
| [UI-003](#ui-003) | Shelved | Date range picker for daily summary view |
| [UI-004](#ui-004) | Cancelled | Complete CustomGridView alternative |

---

## Design notes

### Window

`DailySummary.xaml` — `ListView` + `GridView` columns: Process Name, Window Title, Total Duration, Is Selected.

### Duration calculation

`TotalDuration = recordCount × Constants.MonitoringIntervalMilliseconds`

### View model

`ActivityViewModel` — `ObservableCollection<ActivitySummary>`, column filters, sort, select all/unselect all.

### Column header menus

Built in `DailySummary.xaml.cs` — sort ascending/descending, filter (via `AddFilterDialog`), select/unselect all on checkbox column.

### Known issues

- `ActivityViewModel.UpdateFilter(object, object)` throws `NotImplementedException` (duplicate dead method).
- Summary loads **today only** (`TimeStamp.Date == DateTime.Today`).

### Out of scope (epic v1)

- Charts / pie graphs
- Real-time live updating while window is open

---

## Tickets

### UI-001

| Field | Detail |
|-------|--------|
| **ID** | UI-001 |
| **Title** | Daily summary ListView with sort and filter |
| **Status** | Done |
| **Description** | Tray → Daily Summary opens window; groups today's `ActivityRecords` by process + title; `ActivityViewModel` with sort and `AddFilterDialog` (starts with / contains / ends with). |
| **Test / demo** | Generate activity → open summary → rows appear → sort by duration → filter process name. |
| **Depends on** | FND-002, MON-001 |

### UI-002

| Field | Detail |
|-------|--------|
| **ID** | UI-002 |
| **Title** | Fix ActivityViewModel filter bugs and dead code |
| **Status** | Shelved |
| **Description** | Remove duplicate `UpdateFilter` dead code. Shelved 2026-08-27 — not primary goal. |
| **Test / demo** | Filter Window Title "Contains notepad" → only matching rows → clear filter restores all → sort does not drop filter. |
| **Depends on** | UI-001 |

### UI-003

| Field | Detail |
|-------|--------|
| **ID** | UI-003 |
| **Title** | Date range picker for daily summary view |
| **Status** | Shelved |
| **Description** | Add date selector to `DailySummary`. Shelved 2026-08-27 — not primary goal. |
| **Test / demo** | Pick yesterday → summary shows yesterday's data only → pick today → today's data returns. |
| **Depends on** | UI-001 |

### UI-004

| Field | Detail |
|-------|--------|
| **ID** | UI-004 |
| **Title** | Complete CustomGridView alternative |
| **Status** | Cancelled |
| **Description** | `CustomGridView` was an alternate grid with context menus — incomplete and unused. Cancelled because `DailySummary` column header menus already implement filtering/sorting. |
| **Test / demo** | — |
| **Depends on** | UI-001 |
