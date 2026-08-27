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

### Azure DevOps API

- Endpoint: `POST {ServerUrl}/{project}/_apis/wit/workitems/${WorkItemType}?api-version={version}`
- Content-Type: `application/json-patch+json`
- Auth: **NTLM** — `HttpClientHandler.UseDefaultCredentials = true` (Windows integrated auth for the logged-in user; no PAT)
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
| **Description** | `AzureDevOpsWorkItemService` — JSON Patch POST; NTLM (`UseDefaultCredentials`); URL from `ServerUrl` + `Project`. |
| **Test / demo** | With valid PAT and project in appsettings, call client with test title → Task appears in Azure DevOps portal. Invalid PAT → structured error logged/shown. |
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
| **Description** | `WorkPromptCoordinator` — OK validates text, calls API, retries on error; Cancel closes with no API call. |
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
