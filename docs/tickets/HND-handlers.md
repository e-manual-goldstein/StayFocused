# Epic HND — Activity Handlers

**Project:** StayFocused
**Code:** `HND`
**Scope:** Per-process handlers that enrich activity tracking beyond raw window titles — browser tabs, email subjects, etc.

**Depends on:** MON-001, API-001 (recommended before HND-001)
**Blocks:** —

---

## Primary user story

> When I'm in Firefox or Outlook, I want StayFocused to capture something meaningful (tab title, email subject) not just the generic window caption.

---

## Ticket summary

| ID | Status | Title | Depends on |
|----|--------|-------|------------|
| [HND-001](#hnd-001) | Shelved | Wire custom handlers into ActivityMonitor.GetActivity() | MON-001 |
| [HND-002](#hnd-002) | Shelved | Verify and harden Firefox activity handler | HND-001 |
| [HND-003](#hnd-003) | Shelved | Edge browser URL extraction via COM | HND-001 |
| [HND-004](#hnd-004) | Shelved | Outlook email subject handler (re-enable in build) | HND-001 |

---

## Design notes

### Interface

`IActivityHandler.GetActivityDescription(IntPtr hWnd)` → string used as enriched window title or description.

### Registration

`ActivityMonitor.AddCustomHandler(processName, handler)` — keyed by process name (case sensitivity to be normalised in HND-001).

**Current gap:** handlers are registered in `App.StartActivityMonitor()` but `GetActivity()` never consults `_handlers`.

### Existing handlers

| Handler | Process | State |
|---------|---------|-------|
| `FirefoxActivityHandler` | `firefox` | Registered; returns `WinApi.GetWindowTitle` |
| `EdgeActivityHandler` | `msedge` | Not registered; throws `NotImplementedException` |
| `OutlookHandler` | `OUTLOOK` | Excluded from build (`Compile Remove`) |
| `BasicActivityHandler` | — | Stub |

### Integration sketch (HND-001)

```
GetActivity():
  resolve processName, hWnd
  if _handlers.TryGetValue(processName, out handler):
    windowTitle = handler.GetActivityDescription(hWnd) ?? WinApi.GetWindowTitle(hWnd)
  else:
    windowTitle = WinApi.GetWindowTitle(hWnd)
```

### Out of scope (epic v1)

- Chrome handler
- Auto-discovery of handlers from plugins (see PLG-002)

---

## Tickets

### HND-001

| Field | Detail |
|-------|--------|
| **ID** | HND-001 |
| **Title** | Wire custom handlers into ActivityMonitor.GetActivity() |
| **Status** | Shelved |
| **Description** | Consult `_handlers` by process name during `GetActivity()`. Shelved 2026-08-27 — not primary goal. |
| **Test / demo** | Register a test handler for `notepad` → focus Notepad → DB records use handler output in `WindowTitle`. |
| **Depends on** | MON-001 |

### HND-002

| Field | Detail |
|-------|--------|
| **ID** | HND-002 |
| **Title** | Verify and harden Firefox activity handler |
| **Status** | Shelved |
| **Description** | Confirm `FirefoxActivityHandler` works with current Firefox. Shelved 2026-08-27 — not primary goal. |
| **Test / demo** | Open Firefox with two tabs → switch tabs → Daily Summary shows distinct window titles per tab. |
| **Depends on** | HND-001 |

### HND-003

| Field | Detail |
|-------|--------|
| **ID** | HND-003 |
| **Title** | Edge browser URL extraction via COM |
| **Status** | Shelved |
| **Description** | `EdgeActivityHandler` experiments with `Shell.Application`, `CoGetObject`, `IAccessible` — incomplete. Shelved until HND-001 provides integration path; may need WebView2 or UI Automation instead. |
| **Test / demo** | — |
| **Depends on** | HND-001 |

### HND-004

| Field | Detail |
|-------|--------|
| **ID** | HND-004 |
| **Title** | Outlook email subject handler (re-enable in build) |
| **Status** | Shelved |
| **Description** | Re-include `OutlookHandler.cs` in build. Shelved 2026-08-27 — not primary goal. |
| **Test / demo** | Outlook running → select email → focus Outlook window → summary shows email subject in Window Title column. |
| **Depends on** | HND-001 |
