# Epic FND — Foundation

**Project:** StayFocused
**Code:** `FND`
**Scope:** Application shell — WPF/WinForms hybrid host, dependency injection, SQLite database, system tray, configuration, and logging.

**Depends on:** —
**Blocks:** MON, HND, UI, PLG

---

## Primary user story

> I install StayFocused and it runs quietly in the background from the system tray. It remembers settings, stores my activity data locally, and I can exit cleanly when I want.

---

## Ticket summary

| ID | Status | Title | Depends on |
|----|--------|-------|------------|
| [FND-001](#fnd-001) | Done | Solution structure, DI container, and SQLite database | — |
| [FND-002](#fnd-002) | Done | System tray icon with Exit and Daily Summary menu | FND-001 |
| [FND-003](#fnd-003) | Done | ConfigManager and missing-setting input dialog | FND-001 |
| [FND-004](#fnd-004) | Shelved | Remove dead code (Startup stub, unused MainWindow path) | FND-001 |

---

## Design notes

### Projects

| Project | Role |
|---------|------|
| `StayFocused` | Main WPF/WinForms app |
| `StayFocused.Api` | Shared plugin contracts |
| `ExampleModule` | Reference plugin |

### Data location

- SQLite: `%AppData%\StayFocused\sf.db`
- Config: `config.json` in working directory (plugin config path via `PluginConfig` key)

### Services (singletons)

`SystemMenu`, `ConfigManager`, `ILogManager` → `DefaultLogger`, `IActivityMonitor` → `ActivityMonitor`, `PluginService`, `SFDbContext`

### Shutdown

`ShutdownMode.OnExplicitShutdown` — app survives window close; tray **Exit** or double-click shuts down.

### Out of scope (epic)

- Installer / auto-update
- Multi-user profiles

---

## Tickets

### FND-001

| Field | Detail |
|-------|--------|
| **ID** | FND-001 |
| **Title** | Solution structure, DI container, and SQLite database |
| **Status** | Done |
| **Description** | Three-project solution; `App` registers services via `Microsoft.Extensions.DependencyInjection`; EF Core SQLite with `SFDbContext` and `ActivityRecord` entity; `Database.EnsureCreated()` on startup. |
| **Test / demo** | Build solution; run app; confirm `%AppData%\StayFocused\sf.db` is created. |
| **Depends on** | — |

### FND-002

| Field | Detail |
|-------|--------|
| **ID** | FND-002 |
| **Title** | System tray icon with Exit and Daily Summary menu |
| **Status** | Done |
| **Description** | `SystemMenu` with `NotifyIcon`, `icon.ico`, context menu (Daily Summary, Exit), double-click shutdown, `ProcessExit` cleanup. |
| **Test / demo** | Run app → tray icon visible → Daily Summary opens window → Exit removes icon and stops process. |
| **Depends on** | FND-001 |

### FND-003

| Field | Detail |
|-------|--------|
| **ID** | FND-003 |
| **Title** | ConfigManager and missing-setting input dialog |
| **Status** | Done |
| **Description** | `ConfigManager` reads/writes `config.json`; `SettingNotFound` event shows `InputDialog`; app shuts down if user cancels required setting. |
| **Test / demo** | Delete config key → app prompts for value → value persisted on OK. |
| **Depends on** | FND-001 |

### FND-004

| Field | Detail |
|-------|--------|
| **ID** | FND-004 |
| **Title** | Remove dead code (Startup stub, unused MainWindow path) |
| **Status** | Shelved |
| **Description** | Delete empty `Startup` class; remove or document `MainWindow`. Shelved 2026-08-27 — not primary goal. |
| **Test / demo** | Build succeeds; tray app still runs; no references to removed types. |
| **Depends on** | FND-001 |
