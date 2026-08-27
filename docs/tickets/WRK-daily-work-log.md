# Epic WRK — Daily Work Log

**Project:** StayFocused
**Code:** `WRK`
**Scope:** **Primary goal.** A continuously running tray application that prompts the user once per day with a lean dialog — *"What did you work on today?"* — and on **OK** creates an Azure DevOps **Task** work item using the entered text plus mandatory field values from `appsettings.json`. **Cancel** closes the dialog with no action until the next daily prompt.

**Depends on:** FND-002 (tray app runs continuously in background)
**Blocks:** —

---

## Primary user story

> StayFocused runs quietly in the background. Each day it asks me one question: what did I work on today? I type a short answer and click OK — it becomes a Task in Azure DevOps with the right project and fields filled in. If I'm not ready, I click Cancel and it leaves me alone until tomorrow.

---

## Ticket summary

| ID | Status | Title | Depends on |
|----|--------|-------|------------|
| [WRK-001](#wrk-001) | Todo | appsettings configuration for Azure DevOps and daily prompt |
| [WRK-002](#wrk-002) | Todo | Azure DevOps Work Item API client (create Task) |
| [WRK-003](#wrk-003) | Todo | Daily prompt dialog — text field, OK, Cancel |
| [WRK-004](#wrk-004) | Todo | Wire OK to create Task with user text |
| [WRK-005](#wrk-005) | Todo | Daily scheduler — trigger prompt once per calendar day |

---

## Design notes

### Continuous service

The existing tray application (`SystemMenu`, `ShutdownMode.OnExplicitShutdown`) is the host. No separate Windows Service for v1 — the WPF app must be running (typically started at login). WRK-005 adds the daily trigger on top of this lifecycle.

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
    "Organization": "my-org",
    "Project": "MyProject",
    "PersonalAccessToken": "",  // prefer User Secrets or env var in production
    "ApiVersion": "7.0"
  },
  "DailyPrompt": {
    "PromptTime": "17:00",
    "UserTextField": "System.Title"
  },
  "WorkItem": {
    "WorkItemType": "Task",
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

- Endpoint: `POST https://dev.azure.com/{org}/{project}/_apis/wit/workitems/${WorkItemType}?api-version={version}`
- Content-Type: `application/json-patch+json`
- Auth: `Basic` with empty username and PAT as password, or `Bearer` depending on token type
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
| **Status** | Todo |
| **Description** | Add `appsettings.json` (and optional `appsettings.Development.json`). Define strongly-typed options classes: `AzureDevOpsOptions`, `DailyPromptOptions`, `WorkItemOptions` with `MandatoryFields` dictionary. Register with `IOptions` via `Microsoft.Extensions.Configuration`. Wire configuration in `App` startup. PAT should support environment variable override (e.g. `STAYFOCUSED_ADO_PAT`). Do not start activity monitor if not needed for WRK epic (optional: disable MON in startup for this phase). |
| **Test / demo** | App builds; options bind correctly from appsettings; missing required keys fail fast with clear log/message at startup. |
| **Depends on** | FND-002 |

### WRK-002

| Field | Detail |
|-------|--------|
| **ID** | WRK-002 |
| **Title** | Azure DevOps Work Item API client (create Task) |
| **Status** | Todo |
| **Description** | Implement `IAzureDevOpsClient` (or `IWorkItemService`) that creates a work item: builds JSON Patch from `MandatoryFields` + user text field, POSTs to Azure DevOps REST API, returns work item ID or throws with API error detail. Use `HttpClient` with PAT auth. Work item type from `WorkItem:WorkItemType` (default `Task`). |
| **Test / demo** | With valid PAT and project in appsettings, call client with test title → Task appears in Azure DevOps portal. Invalid PAT → structured error logged/shown. |
| **Depends on** | WRK-001 |

### WRK-003

| Field | Detail |
|-------|--------|
| **ID** | WRK-003 |
| **Title** | Daily prompt dialog — text field, OK, Cancel |
| **Status** | Todo |
| **Description** | New WPF window `DailyWorkPromptDialog` (or similar): label *"What did you work on today?"*, single text field, **OK** and **Cancel** buttons. Cancel sets `DialogResult = false` and closes — no other side effects. OK sets `DialogResult = true` and exposes entered text via property. Small, centered, minimal chrome. Can be shown manually from tray menu item *"Log today's work"* for testing before scheduler exists. |
| **Test / demo** | Show dialog → type text → OK returns text → Cancel closes with no return value and no file/API changes. |
| **Depends on** | WRK-001 |

### WRK-004

| Field | Detail |
|-------|--------|
| **ID** | WRK-004 |
| **Title** | Wire OK to create Task with user text |
| **Status** | Todo |
| **Description** | On OK: read text from dialog, call `IAzureDevOpsClient` with user text mapped to configured field (`UserTextField`). On success: close dialog, optional brief confirmation (toast or message). On failure: show error, keep dialog open or close with error message — prefer show error and allow retry. Empty text: disable OK or show validation message. |
| **Test / demo** | OK with *"Fixed login bug"* → new Task in ADO with that title + mandatory fields from appsettings. |
| **Depends on** | WRK-002, WRK-003 |

### WRK-005

| Field | Detail |
|-------|--------|
| **ID** | WRK-005 |
| **Title** | Daily scheduler — trigger prompt once per calendar day |
| **Status** | Todo |
| **Description** | `DailyPromptScheduler` service: background timer checks if today's prompt is due (`PromptTime` passed and `LastPromptDate < today`). Show `DailyWorkPromptDialog` on UI thread. On dismiss (OK or Cancel), set `LastPromptDate = today` so dialog does not reappear until next calendar day. Start scheduler in `App.Start()`. Remove or disable unrelated startup work (activity monitor) unless human wants both. |
| **Test / demo** | Set `PromptTime` to 1 minute from now → dialog appears once → Cancel → no second prompt same day. Next day (or manually reset `LastPromptDate`) → dialog appears again. |
| **Depends on** | WRK-003, WRK-004 |
