# StayFocused — Component Reference

> **Planning:** active work queue is [`docs/backlog.md`](docs/backlog.md). Agent workflow: [`agent-methodology/instructions.md`](agent-methodology/instructions.md).

StayFocused is a Windows desktop application (.NET 6) that tracks how you spend time on your computer. It periodically polls the foreground window, records which process and window title are active, and persists that data to a local SQLite database. A system tray icon provides access to a daily summary view.

The solution is split across three projects:

| Project | Role |
|---|---|
| **StayFocused** | Main WPF/WinForms application |
| **StayFocused.Api** | Shared interfaces and contracts for plugins |
| **ExampleModule** | Example plugin demonstrating the plugin system |

---

## Architecture Overview

```
┌─────────────────────────────────────────────────────────┐
│  App (entry point, DI setup)                            │
│    ├── SystemMenu (tray icon)                           │
│    ├── ActivityMonitor (polling loop)                   │
│    │     ├── WinApi (foreground window)                 │
│    │     ├── Activity handlers (per-process logic)      │
│    │     └── TaskRunner (interval timer)                │
│    ├── SFDbContext (SQLite persistence)                 │
│    ├── ConfigManager (config.json)                      │
│    └── PluginService (dynamic plugin loading)           │
└─────────────────────────────────────────────────────────┘
```

On startup the app registers services via `Microsoft.Extensions.DependencyInjection`, loads plugins from `plugin-config.json`, ensures the database exists, and begins the activity monitor. The app runs in the background with `ShutdownMode.OnExplicitShutdown` — it only exits when the user chooses Exit from the tray menu.

---

## StayFocused.Api

Shared contracts consumed by the main app and external plugin modules.

### `IActivityMonitor`

Defines the monitoring lifecycle:

- `Begin()` — starts polling.
- `AddCustomHandler(processName, handler)` — registers a per-process handler for richer activity descriptions.

### `IActivity`

Represents a tracked activity:

- `ProcessName`, `ActivityScore` — identity and accumulated time score.
- `IncrementActivityScore()` — increments the score on each poll interval.

### `IActivityHandler`

Per-process handler for extracting a meaningful activity description from a window handle:

- `GetActivityDescription(hWnd)` — returns a human-readable description (e.g. browser tab title, email subject).

### `IPlugin`

Plugin contract for extending the app at startup:

- `PluginName` — display name.
- `OnPluginLoaded(services)` — called before the service provider is built; plugins can register additional services.
- `OnServicesBuilt(serviceProvider)` — called after the service provider is built; plugins can consume services.

### `ILogManager`

Simple logging interface with a single `Log(string)` method.

### `PluginDefinition`

JSON-serialisable record describing a plugin entry in `plugin-config.json`:

- `Name`, `FullTypeName`, `ModuleLocation` — plugin identity, CLR type name, and path to the DLL.

---

## Core Application

### `App` (`App.xaml` / `App.xaml.cs`)

Application entry point and composition root.

- Registers all services (singletons) into a `ServiceCollection`.
- Configures SQLite via EF Core, storing the database at `%AppData%\StayFocused\sf.db`.
- Loads plugins through `PluginService`, allowing them to modify the service collection before the final `ServiceProvider` is built.
- On startup: initialises the system tray, ensures the database schema exists, registers activity handlers, and starts the monitor.
- Handles missing config values by showing an `InputDialog`; shuts down if the user cancels.

### `ActivityMonitor`

The heart of the application. Implements `IActivityMonitor`.

- Runs `StayFocused()` every 5 seconds (via `TaskRunner` and `Constants.MonitoringIntervalMilliseconds`).
- On each tick:
  1. Gets the foreground window handle via `WinApi`.
  2. Resolves the process name and window title.
  3. Creates or retrieves an `Activity` from a concurrent dictionary keyed by `processName;windowTitle`.
  4. Writes an `ActivityRecord` to the database.
  5. Increments the activity score.
- Listens for Windows session lock/unlock events (`SystemEvents.SessionSwitch`) and sets an internal `_stationLocked` flag (currently the flag is set but not yet used to pause recording).
- Supports custom per-process handlers via `AddCustomHandler`, though the handler dictionary is not yet consulted during `GetActivity()`.

### `WinApi`

Static wrapper around Windows user32/psapi DLL imports:

- `GetForegroundWindow()` — handle of the currently focused window.
- `GetWindowTitle(handle)` — window title text.
- `GetWindowThreadProcessId()` — process ID for a window handle.

### `TaskRunner`

Generic interval-based task executor.

- Accepts an `Action` and an interval in milliseconds.
- Runs the action in a background `Task.Run` loop with `Task.Delay` between invocations.
- Supports cancellation via `End()`.
- Used by `ActivityMonitor` for the polling loop.

### `Constants`

Application-wide timing constants:

- `MonitoringIntervalMilliseconds` = 5000 (5 seconds between polls).
- `PersistenceIntervalMilliseconds` = 60000 (60 seconds — defined but the persistence task is commented out).

### `SFDbContext`

EF Core `DbContext` for SQLite persistence.

- Single entity set: `DbSet<ActivityRecord> ActivityRecords`.
- Database file location is configured in `App.ConfigureDataAccess()`.

### `ConfigManager`

Manages a local `config.json` file as a `Dictionary<string, string>`.

- `GetConfigSetting(key)` — reads a value; creates a default empty config if the file doesn't exist.
- If a key is missing, fires the `SettingNotFound` event (handled by `App` to show an `InputDialog`) and persists the new value.
- Supports a `fullReset` constructor option to delete the config file.

### `DefaultLogger`

Implements `ILogManager` by writing messages to `Console.WriteLine`.

### `SystemMenu`

Manages the system tray `NotifyIcon` and its context menu.

- Loads `icon.ico` from the application directory.
- Context menu items:
  - **Daily Summary** — opens the `DailySummary` window populated with today's records.
  - **Exit** — hides and disposes the tray icon.
- Double-click on the tray icon triggers shutdown.
- Registers `OnExit` with `AppDomain.CurrentDomain.ProcessExit` for cleanup.

### `MainWindow`

A minimal WPF `Window` with an empty grid. On close it hides itself rather than shutting down the application, keeping the tray icon alive.

### `CoreMarshal`

COM interop helper for locating running COM objects by programmatic ID.

- `GetActiveObject(progId)` — enumerates the Running Object Table to find and return a COM object (used by `OutlookHandler` to connect to a running Outlook instance).

### `Startup`

Empty stub class — no implementation.

---

## Activities

### `Activity`

Concrete implementation of `IActivity`. Represents a single tracked window/process combination.

- `ProcessName`, `WindowTitle`, `ActivityScore` — identity and accumulated score.
- `Timespan` — derived property: `ActivityScore × MonitoringIntervalMilliseconds` converted to a `TimeSpan`.
- `IncrementActivityScore()` — increments the score by 1.
- `CreateNewEntry()` — creates an `ActivityRecord` with the current UTC timestamp.

### `ActivityRecord`

EF Core entity persisted to SQLite.

| Field | Type | Description |
|---|---|---|
| `ActivityRecordId` | `Guid` | Primary key |
| `ProcessName` | `string` | Process name of the active window |
| `WindowTitle` | `string` | Title text of the active window |
| `TimeStamp` | `DateTime` | UTC timestamp of the poll |

### `ActivitySummary`

View-model class (`INotifyPropertyChanged`) used by the daily summary UI.

- `ProcessName`, `WindowTitle`, `TotalDuration` — aggregated activity data.
- `IsSelected` — checkbox state for bulk selection in the summary grid.

### `InActivity`

Subclass of `Activity` representing an inactive/idle state. Returns `"Inactive"` as a description. Currently unused in the monitoring loop.

---

## Activity Handlers

Per-process handlers that can extract richer activity descriptions from a window handle. Registered via `ActivityMonitor.AddCustomHandler()`.

### `BasicActivityHandler`

Stub handler. `GetActivityDescription()` throws `NotImplementedException`. Provides a `GetBasicActivity()` factory that creates a plain `Activity` with a description as the process name.

### `FirefoxActivityHandler`

Registered at startup for the `"firefox"` process. Returns the window title via `WinApi.GetWindowTitle(hWnd)` — effectively the browser tab title.

### `EdgeActivityHandler`

Experimental handler for Microsoft Edge. Attempts to extract the current URL via COM (`Shell.Application`, `CoGetObject`, `IAccessible`). Most of the implementation is incomplete and ultimately throws `NotImplementedException`. Not registered at startup.

### `OutlookHandler`

Attempts to connect to a running Outlook instance via COM (`CoreMarshal.GetActiveObject`) and read the subject of the currently selected email. Excluded from the build in `StayFocused.csproj` (`Compile Remove`). Not registered at startup (commented out in `App.StartActivityMonitor()`).

---

## GUI

### `DailySummary` (`DailySummary.xaml`)

WPF window displaying today's activity records in a `ListView` with a `GridView`.

Columns: Process Name, Window Title, Total Duration, Is Selected (checkbox).

- Loads records from `SFDbContext` filtered to today's date.
- Groups records by process name and window title, computing total duration as `recordCount × MonitoringIntervalMilliseconds`.
- Column headers have context menus for sorting (ascending/descending), filtering, and select/unselect all.

### `ActivityViewModel`

MVVM view model backing the `DailySummary` list.

- Maintains an `ObservableCollection<ActivitySummary>` for display and a full `_allActivitySummaries` list for filtering.
- `UpdateFilter(columnName, filter)` — applies a predicate filter per column.
- `UpdateSorting(column, descending)` — sorts by Process Name, Window Title, or Total Duration.
- `SelectAllVisible()` / `UnselectAllVisible()` — toggles the `IsSelected` checkbox on all visible rows.

### `AddFilterDialog` (`AddFilterDialog.xaml`)

Dialog for defining a text filter on a grid column.

- Filter modes: Starts With, Contains, Ends With (case-insensitive).
- Returns a `Func<ActivitySummary, bool>` predicate applied by `ActivityViewModel`.

### `InputDialog` (`InputDialog.xaml`)

Generic single-field input dialog.

- Displays a prompt message and a text box.
- Used by `App` when a required config setting is missing.
- Returns `InputValue` on OK, or `DialogResult = false` on cancel.

### `CustomGridView`

Incomplete alternative to the standard `GridView` with built-in column context menus for filtering and ordering. Methods throw `NotImplementedException`. Not used by any window.

---

## Plugins

### `PluginService`

Loads and manages external plugin modules at startup.

1. Reads the plugin config path from `ConfigManager` (`"PluginConfig"` key).
2. Deserialises `plugin-config.json` into a `Dictionary<string, PluginDefinition>`.
3. Loads each plugin DLL via `Assembly.LoadFrom()`.
4. Instantiates the plugin type and verifies it implements `IPlugin`.
5. Calls `OnPluginLoaded()` on each plugin (before service provider build).
6. Rebuilds the service provider if any plugin modified the service collection.
7. Calls `OnServicesBuilt()` on each plugin (after service provider build).

### `plugin-config.json`

Example plugin configuration:

```json
{
  "ExamplePlugin": {
    "Name": "ExamplePlugin",
    "FullTypeName": "ExampleModule.ExamplePlugin",
    "ModuleName": "..\\ExampleModule\\bin\\Debug\\net6.0\\ExampleModule.dll"
  }
}
```

---

## ExampleModule

A reference plugin project demonstrating the `IPlugin` interface.

### `ExamplePlugin`

- `PluginName` returns `"Example Plugin"`.
- `OnPluginLoaded()` and `OnServicesBuilt()` are empty stubs.
- References `StayFocused.Api` for the `IPlugin` contract.

---

## Data Flow

```
Every 5 seconds:
  WinApi.GetForegroundWindow()
    → process name + window title
    → Activity (in-memory, keyed by process;title)
    → ActivityRecord (saved to SQLite)
    → ActivityScore incremented

On "Daily Summary" click:
  SFDbContext.ActivityRecords (today's records)
    → grouped by process + title
    → ActivitySummary (with computed TotalDuration)
    → DailySummary ListView (via ActivityViewModel)
```

---

## Incomplete / Commented-Out Work

Several areas were started but not finished:

- **Handler integration** — `ActivityMonitor` registers handlers but never calls them during `GetActivity()`.
- **Session lock** — `_stationLocked` flag is set on lock/unlock but not used to pause recording.
- **Persistence task** — a separate `TaskRunner` for periodic file-based persistence is commented out.
- **Edge URL extraction** — `EdgeActivityHandler` has experimental COM code but is not functional.
- **Outlook integration** — `OutlookHandler` is excluded from the build.
- **CustomGridView** — alternative grid implementation is incomplete.
- **MainWindow** — empty shell window, not wired into the startup flow.
- **Startup class** — empty stub.
- **Namespace inconsistency** — some interfaces live in `StayFocused.Api` namespace, others in `StayFocused` namespace, and `IActivity` is defined in `StayFocused.Api` project but uses the `StayFocused` namespace.
