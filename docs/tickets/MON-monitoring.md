# Epic MON — Activity Monitoring

**Project:** StayFocused
**Code:** `MON`
**Scope:** Core time-tracking loop — poll foreground window every 5 seconds, record process name and window title to SQLite, track in-memory activity scores.

**Depends on:** FND-001
**Blocks:** HND-001 (handler integration extends GetActivity)

---

## Primary user story

> While I work, StayFocused silently records what window I'm focused on so I can see how my time was spent at the end of the day.

---

## Ticket summary

| ID | Status | Title | Depends on |
|----|--------|-------|------------|
| [MON-001](#mon-001) | Done | Core polling loop and ActivityRecord persistence | FND-001 |
| [MON-002](#mon-002) | Shelved | Pause activity recording when Windows session is locked | MON-001 |
| [MON-003](#mon-003) | Shelved | File-based JSON persistence task | MON-001 |

---

## Design notes

### Polling

- Interval: `Constants.MonitoringIntervalMilliseconds` (5000 ms)
- Executor: `TaskRunner` background loop
- Entry point: `ActivityMonitor.StayFocused()`

### Activity identity

- Key: `processName;windowTitle`
- In-memory: `ConcurrentDictionary<string, Activity>`
- Each tick: new `ActivityRecord` row + `ActivityScore++`

### WinApi

`GetForegroundWindow()`, `GetWindowTitle()`, `GetWindowThreadProcessId()` → process name.

### Session lock

`SystemEvents.SessionSwitch` sets `_stationLocked` on lock/unlock — **not yet used** to skip recording (MON-002).

### Entity (`ActivityRecord`)

| Field | Type | Notes |
|-------|------|-------|
| `ActivityRecordId` | `Guid` | PK |
| `ProcessName` | `string` | Active process |
| `WindowTitle` | `string` | Foreground window title |
| `TimeStamp` | `DateTime` | UTC poll time |

### Out of scope (epic)

- Idle detection (no keyboard/mouse)
- Multiple displays / virtual desktops

---

## Tickets

### MON-001

| Field | Detail |
|-------|--------|
| **ID** | MON-001 |
| **Title** | Core polling loop and ActivityRecord persistence |
| **Status** | Done |
| **Description** | `ActivityMonitor` implements `IActivityMonitor`; `Begin()` starts `TaskRunner`; each tick resolves foreground window, upserts `Activity`, writes `ActivityRecord`, increments score. |
| **Test / demo** | Run app 30s with Notepad focused → open Daily Summary → Notepad row with ~6 intervals of duration. |
| **Depends on** | FND-001 |

### MON-002

| Field | Detail |
|-------|--------|
| **ID** | MON-002 |
| **Title** | Pause activity recording when Windows session is locked |
| **Status** | Shelved |
| **Description** | When `_stationLocked` is true, skip `StayFocused()` tick (no DB write, no score increment). Shelved 2026-08-27 — not primary goal (WRK epic). |
| **Test / demo** | Run app → lock workstation (Win+L) → wait 30s → unlock → only pre-lock and post-lock windows appear in summary; lock period not attributed to last active app. |
| **Depends on** | MON-001 |

### MON-003

| Field | Detail |
|-------|--------|
| **ID** | MON-003 |
| **Title** | File-based JSON persistence task |
| **Status** | Shelved |
| **Description** | Separate `TaskRunner` at `PersistenceIntervalMilliseconds` to save activities to dated JSON files. Commented out in `ActivityMonitor`; SQLite replaced this approach. |
| **Test / demo** | — |
| **Depends on** | MON-001 |
