# DailyWorkLog

Standalone tray app: once per day (configurable time) it asks **"What did you work on today?"** and creates an Azure DevOps **Task** when you click OK.

Not wired into the legacy `StayFocused` activity monitor — this project is self-contained.

## Setup

1. Edit committed defaults in `appsettings.json`, then override private values locally.

2. **Local settings (optional):** copy `appsettings.Local.json.example` to `appsettings.Local.json` in the **same folder as the running app** (e.g. `bin/Debug/net6.0-windows/` when using `dotnet run`). This file is optional; when present it is loaded after `appsettings.json` and **overrides** matching keys. Keep it out of git (e.g. `.git/info/exclude`).

   Only include keys you want to override — you do not need to duplicate the full file:

3. Common settings:
   - `AzureDevOps:ServerUrl` — collection base URL (e.g. `https://tfs.mycompany.com/tfs/DefaultCollection`)
   - `AzureDevOps:Project` — project name
   - `WorkItem:MandatoryFields` — dictionary of ADO field reference names and values
   - `DailyPrompt:PromptTime` — local time to show the prompt (e.g. `17:00`)

4. **Authentication** uses **NTLM** (Windows integrated auth). The app runs as the logged-in Windows user — no PAT required. Ensure that user has permission to read/create work items in the project.

5. **Test connection** before creating tasks:
   - Set `AzureDevOps:TestWorkItemId` to a known work item ID, or leave `0` to be prompted
   - Tray → **Test: get work item by ID**
   - Success shows id, type, title, and state

6. Build and run:

```bash
dotnet run --project DailyWorkLog
```

## Tray menu

- **Log today's work** — open the prompt anytime (does not affect the once-per-day scheduler)
- **Exit** — quit the app

Double-click the tray icon to open the prompt manually.

## Scheduler

After `PromptTime` each calendar day, the dialog appears once. **OK** creates the work item; **Cancel** closes with no API call. Either dismisses until the next day.

State file: `%AppData%\DailyWorkLog\prompt-state.json`

## User text field

User input is sent to the field named in `WorkItem:UserTextField` (default `System.Title`), merged with `MandatoryFields`.

On **create**, these fields are also set automatically (unless already present in `MandatoryFields`):

| Field | ADO reference | Default |
|-------|---------------|---------|
| Start date | `Microsoft.VSTS.Scheduling.StartDate` | Today |
| Assigned to | `System.AssignedTo` | `authenticatedUser.properties.Account.$value` from `connectionData` |
| Completed work | `Microsoft.VSTS.Scheduling.CompletedWork` | 7.5 hours |

## API URL

Work items are created at:

`{ServerUrl}/{Project}/_apis/wit/workitems/${WorkItemType}?api-version=...`
