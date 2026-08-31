# Epic WRK — Daily Work Log

**Project:** StayFocused / **App:** `DailyWorkLog` (standalone)
**Code:** `WRK`
**Scope:** **Primary goal.** A continuously running tray application that prompts the user once per day with a lean dialog — *"What did you work on today?"* — and on **OK** creates an Azure DevOps **Task** work item using the entered text plus mandatory field values from `appsettings.json`. **Cancel** closes the dialog with no action until the next daily prompt.

**Implementation:** All WRK tickets shipped in the standalone [`DailyWorkLog/`](../../DailyWorkLog/) project — not integrated into the legacy `StayFocused` activity monitor.

**Depends on:** —
**Blocks:** —

---

## Primary user story

> StayFocused runs quietly in the background. Each day it asks me one question: what did I work on today? I type a short answer and click OK — it becomes a Task in Azure DevOps with the right project and fields filled in. If I'm not ready, I click Cancel and it leaves me alone until tomorrow.

---

## Ticket summary

| ID | Status | Title | Depends on |
|----|--------|-------|------------|
| [WRK-001](#wrk-001) | Done | appsettings configuration for Azure DevOps and daily prompt |
| [WRK-002](#wrk-002) | Done | Azure DevOps Work Item API client (create Task) |
| [WRK-003](#wrk-003) | Done | Daily prompt dialog — text field, OK, Cancel |
| [WRK-004](#wrk-004) | Done | Wire OK to create Task with user text |
| [WRK-005](#wrk-005) | Done | Daily scheduler — trigger prompt once per calendar day |
| [WRK-006](#wrk-006) | Done | NTLM authentication and ServerUrl configuration |
| [WRK-007](#wrk-007) | Done | GetWorkItemAsync and tray ADO connection test |
| [WRK-008](#wrk-008) | Shelved | Run at Windows startup (registry + tray toggle) |

---

## Design notes

### Continuous service

Standalone WPF tray app in `DailyWorkLog/`. No separate Windows Service for v1 — app must be running (start at login). `TrayHost` + `DailyPromptScheduler`.

### Daily prompt dialog

Lean WPF `Window` (not the existing `DailySummary`):

| Element | Behaviour |
|---------|-----------|
| Prompt label | *"What did you work on today?"* |
| Text field | Single-line or multi-line text box for user's answer |
| **OK** | Validate non-empty text (optional — clarify in WRK-004), create work item, close dialog |
| **Cancel** | Close dialog immediately; no API call, no persistence of draft |

After either OK or Cancel, the dialog must not reappear until the next calendar day (WRK-005).

### User text → work item

User-entered text is sent as **`System.Title`** by default. Optional `appsettings` key `WorkItem:UserTextField` to override (e.g. `System.Description`).

### appsettings structure (sketch)

```json
{
  "AzureDevOps": {
    "ServerUrl": "https://tfs.mycompany.com/tfs/DefaultCollection",
    "Project": "MyProject",
    "ApiVersion": "7.0"
  },
  "DailyPrompt": {
    "PromptTime": "17:00",
    "UserTextField": "System.Title"
  },
  "WorkItem": {
    "WorkItemType": "Task",
    "UserTextField": "System.Title",
    "MandatoryFields": {
      "System.AreaPath": "MyProject\\MyArea",
      "System.IterationPath": "MyProject\\Current Sprint",
      "System.Tags": "StayFocused; daily-log"
    }
  }
}
```

`MandatoryFields` is a `Dictionary<string, string>` merged into the create payload. User text is added as an additional field (not duplicated if already in dictionary).

### Default create fields

Applied on every **create** unless overridden in `MandatoryFields`:

| Field | Reference name | Value |
|-------|----------------|-------|
| Start date | `Microsoft.VSTS.Scheduling.StartDate` | Today's date (`yyyy-MM-dd`) |
| Assigned to | `System.AssignedTo` | Current user via `connectionData` API (TFS `@me`) |
| Completed work | `Microsoft.VSTS.Scheduling.CompletedWork` | `7.5` (hours) |

Set `"System.AssignedTo": "@me"` explicitly in config if you want to document intent; any missing `AssignedTo` is resolved the same way.

### Azure DevOps API

- Endpoint: `POST {ServerUrl}/{project}/_apis/wit/workitems/${WorkItemType}?api-version={version}`
- Content-Type: `application/json-patch+json`
- Auth: **NTLM** — `HttpClientHandler.UseDefaultCredentials = true` (Windows integrated auth for the logged-in user; no PAT)
- GET (smoke test): `GET {ServerUrl}/{project}/_apis/wit/workitems/{id}?api-version={version}` → `WorkItemService.GetWorkItemAsync`
- Body: JSON Patch array — `{ "op": "add", "path": "/fields/{fieldRef}", "value": "{value}" }` per field

### Once-per-day logic

- Persist `LastPromptDate` (local file in `%AppData%\StayFocused\` or SQLite) when dialog is shown and dismissed (OK or Cancel)
- Scheduler checks: if `Today > LastPromptDate` and current time ≥ `PromptTime`, show dialog
- Timer interval: e.g. every 60 seconds (configurable constant)

### Out of scope (epic v1)

- Activity monitoring / time tracking (see shelved MON/HND epics)
- Editing or updating existing work items
- Multiple prompts per day
- Azure DevOps work item templates beyond Task type

---

## Tickets

### WRK-001

| Field | Detail |
|-------|--------|
| **ID** | WRK-001 |
| **Title** | appsettings configuration for Azure DevOps and daily prompt |
| **Status** | Done |
| **Description** | Added `appsettings.json`, options classes, `ConfigurationValidator`, DI in `App.xaml.cs`. Auth: NTLM via `UseDefaultCredentials` on `HttpClientHandler`. |
| **Test / demo** | App builds; options bind correctly from appsettings; missing required keys fail fast with clear log/message at startup. |
| **Depends on** | FND-002 |

### WRK-002

| Field | Detail |
|-------|--------|
| **ID** | WRK-002 |
| **Title** | Azure DevOps Work Item API client (create Task) |
| **Status** | Done |
| **Description** | `WorkItemService` — JSON Patch POST; NTLM; URL from `ServerUrl` + `Project`. |
| **Test / demo** | With valid NTLM access and project in appsettings, create with test title → Task appears in ADO. |
| **Depends on** | WRK-001 |

### WRK-003

| Field | Detail |
|-------|--------|
| **ID** | WRK-003 |
| **Title** | Daily prompt dialog — text field, OK, Cancel |
| **Status** | Done |
| **Description** | `DailyWorkPromptDialog` — label, text box, OK/Cancel. Tray menu *Log today's work* for manual open. |
| **Test / demo** | Show dialog → type text → OK returns text → Cancel closes with no return value and no file/API changes. |
| **Depends on** | WRK-001 |

### WRK-004

| Field | Detail |
|-------|--------|
| **ID** | WRK-004 |
| **Title** | Wire OK to create Task with user text |
| **Status** | Done |
| **Description** | `WorkPromptCoordinator` — OK awaits `CreateTaskAsync`, shows errors inline, retries on failure; Cancel closes with no API call. |
| **Test / demo** | OK with *"Fixed login bug"* → new Task in ADO with that title + mandatory fields from appsettings. |
| **Depends on** | WRK-002, WRK-003 |

### WRK-005

| Field | Detail |
|-------|--------|
| **ID** | WRK-005 |
| **Title** | Daily scheduler — trigger prompt once per calendar day |
| **Status** | Done |
| **Description** | `DailyPromptScheduler` + `PromptStateStore` (`%AppData%\DailyWorkLog\prompt-state.json`). Once per day after `PromptTime`; manual tray prompt does not affect scheduler state. |
| **Test / demo** | Set `PromptTime` to 1 minute from now → dialog appears once → Cancel → no second prompt same day. Next day (or manually reset `LastPromptDate`) → dialog appears again. |
| **Depends on** | WRK-003, WRK-004 |

### WRK-006

| Field | Detail |
|-------|--------|
| **ID** | WRK-006 |
| **Title** | NTLM authentication and ServerUrl configuration |
| **Status** | Done |
| **Description** | Replaced PAT/Basic auth with `UseDefaultCredentials` on `HttpClientHandler`. Config uses `ServerUrl` (collection base) + `Project` instead of `Organization` + PAT. |
| **Test / demo** | App starts without PAT; ADO calls authenticate as logged-in Windows user. |
| **Depends on** | WRK-002 |

### WRK-007

| Field | Detail |
|-------|--------|
| **ID** | WRK-007 |
| **Title** | GetWorkItemAsync and tray ADO connection test |
| **Status** | Done |
| **Description** | `WorkItemService.GetWorkItemAsync` returns `WorkItemSummary`. Tray **Test: get work item by ID**; optional `TestWorkItemId` in appsettings. |
| **Test / demo** | Tray → test get → known ID returns title/type/state; bad URL/auth shows API error body. |
| **Depends on** | WRK-006 |

### WRK-008

| Field | Detail |
|-------|--------|
| **ID** | WRK-008 |
| **Title** | Run at Windows startup (registry + tray toggle) |
| **Status** | Shelved |
| **Description** | `StartupRegistration` writes HKCU `Run` key for `DailyWorkLog`. Tray checkbox **Run at Windows startup**. Shelved — implementation reverted; not wanted yet. |
| **Test / demo** | — |
| **Depends on** | WRK-005 |
