# Agent methodology — backlog + epic tickets

This document describes a **ticket-driven, human-in-the-loop** workflow for working with coding agents. It is designed for projects where work is broken into small, reviewable units with clear acceptance criteria, and the human says **“Next”** to advance the queue.

Copy this folder into a new repo, replace the template backlog with real epics, and point agents at `instructions.md` plus `docs/backlog.md`.

---

## Core idea


| Layer     | File                       | Purpose                                                          |
| --------- | -------------------------- | ---------------------------------------------------------------- |
| **Queue** | `docs/backlog.md`          | What to do **next**, across all epics — one ordered list         |
| **Spec**  | `docs/tickets/<EPIC>-*.md` | **Why and how** for each epic — design notes + per-ticket detail |
| **Index** | `docs/tickets/README.md`   | Epic catalogue, status vocabulary, links to plan docs            |


The backlog is the **control panel**. Epic files are the **source of truth** for acceptance criteria. Agents read the backlog to find the active ticket, then open the linked epic file for full context.

---



## Interactive loop (human ↔ agent)

Typical session rhythm:

1. Human says **“Next”** (or names a ticket explicitly).
2. Agent reads `docs/backlog.md` → first row in **Active (recommended order)**.
3. Agent opens the linked epic file → finds the ticket section → reads **Design notes** and **Depends on**.
4. Agent implements the ticket (code, tests, migrations, etc.).
5. Agent **updates documentation**:
  - Epic file: ticket **Status** → `Done` (or `Shelved` / `In Progress`), refresh **Description** to past tense / what shipped.
  - Epic **Ticket summary** table: same status update.
  - `backlog.md`: move ticket to **Recently completed**, remove from **Active**, adjust **Epic progress** counts.
6. Agent gives **manual test steps** (and notes any server restart / rebuild / logout required).
7. Human tests, commits, and says **“Next”** again.

Agents should **not** create git commits unless the human explicitly asks. The human owns review and commit timing.

---



## Ticket ID convention

```
<EPIC>-<NNN>
```

Examples: `CAL-001`, `MOB-003`, `FFU-004`.

- **EPIC** — 2–4 letter code (`CAL`, `AUT`, `INV`).
- **NNN** — zero-padded sequence within the epic (`001`, `002`, …).

Epic filenames: `<CODE>-<short-slug>.md` (e.g. `CAL-calendar.md`, `INV-inventory.md`).

---



## Status values

Use consistently in epic ticket tables and ticket detail blocks:


| Status          | Meaning                                                                          |
| --------------- | -------------------------------------------------------------------------------- |
| **Todo**        | Not started; eligible for **Active** queue                                       |
| **In Progress** | Agent or human is actively working on it                                         |
| **Done**        | Shipped and verified (or accepted by human)                                      |
| **Blocked**     | Cannot proceed — document blocker in ticket or backlog                           |
| **Shelved**     | Paused / rejected approach — not in **Active**; listed in **Shelved** with notes |
| **Cancelled**   | Will not implement                                                               |
| **Idea**        | Unprioritized — lives in **Ideas** section of backlog, not **Active**            |


When shelving after partial implementation, **revert code** to the last good state and document why in backlog **Shelved** notes (see UPD-007 pattern).

---



## `backlog.md` rules



### Active (recommended order)

- Ordered list of **open** tickets the human wants done next.
- One row per ticket: `TicketId`, epic link, short description.
- Cross-epic ordering is intentional (e.g. API before UI).
- When a **new** ticket is added to an epic doc, insert it in the right position here.



### Recently completed

- Shows **only the latest** completed ticket(s).
- Replace the row when a new ticket lands.
- If multiple tickets complete in one batch (same session/commit), list **all** of them.
- Use strikethrough on ticket ID: `~~CAL-002~~`.



### Epic progress

- Include **in-progress epics only** (not 100% complete).
- Columns: Tickets Completed, Tickets Shelved, Total Tickets, Progress bar.
- **100% completed** epics move to **Completed epics (100%)** at the bottom.
- Progress bar: always 10 squares — 🟩 completed, 🟨 shelved, ⬜ open.  
Formula: `floor(count ÷ total × 10)` per segment type.  
Percentage shown = **completed only**.



### Shelved / Cancelled / Ideas

- **Shelved** — out of active queue; include **Notes** (why, what to revisit).
- **Cancelled** — closed forever; brief reason.
- **Ideas** — unprioritized; link to ideas epic doc.

---



## Epic file structure

Each epic markdown file should contain:

1. **Header** — project name, epic code, scope paragraph.
2. **Depends on / Blocks** — cross-epic dependencies.
3. **Primary user story** — one paragraph in the user's voice.
4. **Ticket summary** — table with ID, Status, Title, Depends on (anchor links to ticket sections).
5. **Design notes** — entities, API sketch, permissions, UI notes, out-of-scope list. Shared context for all tickets in the epic.
6. **Tickets** — one `### EPIC-NNN` section per ticket:


| Field           | Detail                                                       |
| --------------- | ------------------------------------------------------------ |
| **ID**          | EPIC-NNN                                                     |
| **Title**       | Short imperative title                                       |
| **Status**      | Todo / Done / …                                              |
| **Description** | What to build; for Done tickets, what was actually delivered |
| **Test / demo** | Concrete steps to verify                                     |
| **Depends on**  | Other ticket IDs                                             |


**Design notes** hold stable architecture; **ticket Description** holds ticket-specific scope. Avoid duplicating long design text in every ticket — reference design notes instead.

---



## What agents should do on “Next”

1. **Confirm scope** — active ticket from backlog; read full ticket + design notes.
2. **Check dependencies** — Depends on tickets should be Done (or document if not).
3. **Implement minimally** — match existing project patterns; don’t expand scope.
4. **Test** — run relevant tests/build; fix failures.
5. **Update docs** — epic status + backlog (all sections affected).
6. **Report** — summary of changes + manual test steps + anything the human must do (restart server, reinstall app, re-login).

If blocked (missing credentials, ambiguous requirement, failing CI outside scope), say so clearly and suggest updating ticket to **Blocked** with notes.

---



## Splitting work across tickets

Good ticket boundaries:

- **Vertical slices** when possible (API + one client surface).
- **Platform splits** when shared API is done first (e.g. CAL-001 server → CAL-002 MAUI → CAL-003 WPF).
- **Foundation before features** (FND, AUT, API before domain epics).

Bad ticket boundaries:

- “Finish calendar” (too large).
- “Fix bugs” (no acceptance criteria).
- Tickets that silently depend on unlisted work.

When adding tickets mid-epic, update both epic **Ticket summary** and backlog **Active**.

---



## Shelving and reverting

When the human rejects an approach:

1. Revert implementation commits or code changes.
2. Set ticket **Status** → `Shelved` in epic file.
3. Remove from backlog **Active**; add to **Shelved** with explanation.
4. Increment **Tickets Shelved** in epic progress.
5. Restore **Active** to the next sensible ticket (often the shelved ticket’s predecessor or an alternative).

Do not leave half-implemented features on main unless the human agrees.

---



## Prompting other agents

Minimal bootstrap prompt:

> Follow `agent-methodology/instructions.md`. Work ticket-by-ticket from `docs/backlog.md`. When I say **Next**, implement the first **Active** ticket, update the epic doc and backlog, and give me manual test steps. Do not commit unless I ask.

Optional additions:

- Point to project-specific rules (`.cursor/rules/`, `plan.md`).
- Specify stack and test commands.
- Clarify which surfaces exist (mobile, desktop, server).

---



## Files in this folder


| File                  | Purpose                                                                   |
| --------------------- | ------------------------------------------------------------------------- |
| `instructions.md`     | This document — workflow for humans and agents                            |
| `backlog.template.md` | Fake example backlog — copy to `docs/backlog.md` and replace placeholders |
| `epic.template.md`    | Fake example epic — copy to `docs/tickets/` when starting a new epic      |


---



## Checklist — ticket complete

- [ ] Code/tests/build pass (or human informed of environment blockers)
- [ ] Epic ticket **Status** = Done (description updated to past tense)
- [ ] Epic **Ticket summary** row updated
- [ ] Backlog **Recently completed** updated
- [ ] Ticket removed from **Active**
- [ ] **Epic progress** counts and bar updated; 100% epics moved to **Completed epics**
- [ ] Manual test steps provided to human
