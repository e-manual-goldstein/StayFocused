# Unfinished Ticket Backlog

Ordered list of **open** tickets across all epics. When a ticket is completed, add it to **Recently completed** (table below) and remove it from **Active**. That section shows **only the latest** completed ticket — replace the row when a new one lands. If you complete **multiple tickets in one batch** (same session/commit), list every ticket from that batch in the table instead. When a new ticket is created in any epic doc, add a row in the appropriate position in **Active**.

**Source epics:** see [tickets/](tickets/README.md) for full acceptance criteria.

## Recently completed


| TicketId    | Epic                               | Description                                      |
| ----------- | ---------------------------------- | ------------------------------------------------ |
| ~~INV-002~~ | [INV](tickets/INV-inventory.md)    | MAUI — scan item barcode and adjust quantity     |


## Active (recommended order)


| TicketId | Epic                             | Description                                           |
| -------- | -------------------------------- | ----------------------------------------------------- |
| INV-003  | [INV](tickets/INV-inventory.md)  | WPF — inventory management view with bulk edit        |
| RPT-001  | [RPT](tickets/RPT-reporting.md)  | Report model, API, and permissions (v1)                |
| RPT-002  | [RPT](tickets/RPT-reporting.md)  | WPF — weekly usage dashboard                          |
| DEP-001  | [DEP](tickets/DEP-deployment.md) | Staging environment + CI deploy pipeline            |


## Epic progress

In-progress epics only. **100%** completed epics are listed at the [end of this file](#completed-epics-100).


| Epic                                                          | Description                          | Tickets Completed | Tickets Shelved | Total Tickets | Progress                  |
| ------------------------------------------------------------- | ------------------------------------ | ----------------- | --------------- | ------------- | ------------------------- |
| [Inventory (INV)](tickets/INV-inventory.md)                   | Stock tracking across warehouses     | 2                 | 0               | 3             | 🟩🟩🟩🟩🟩🟩🟨⬜⬜⬜ 67%   |
| [Reporting (RPT)](tickets/RPT-reporting.md)                   | Usage and export reports             | 0                 | 0               | 2             | ⬜⬜⬜⬜⬜⬜⬜⬜⬜⬜ 0%             |
| [Mobile App (MOB)](tickets/MOB-mobile-app.md)                 | React Native field app               | 4                 | 1               | 6             | 🟩🟩🟩🟩🟩🟩🟨⬜⬜⬜ 67%   |
| [Production Deployment (DEP)](tickets/DEP-deployment.md)      | Staging + production hosting         | 0                 | 1               | 2             | 🟨⬜⬜⬜⬜⬜⬜⬜⬜⬜ 0%             |


*Progress bar is always 10 squares: 🟩 completed, 🟨 shelved, ⬜ open — each segment uses* `floor(count ÷ total × 10)`*; percentage shown is completed only.*

## Shelved

Not in the active queue; revisit when relevant.


| TicketId | Epic                                      | Description                         | Notes                                                                 |
| -------- | ----------------------------------------- | ----------------------------------- | --------------------------------------------------------------------- |
| MOB-005  | [MOB](tickets/MOB-mobile-app.md)          | Offline sync for inventory counts   | Shelved — conflict resolution UX unclear; revisit after INV-003 ships |
| DEP-002  | [DEP](tickets/DEP-deployment.md)          | Blue/green production cutover       | Shelved — single staging env sufficient for v1                        |
| INV-004  | [INV](tickets/INV-inventory.md)           | Auto-reorder when below threshold   | Shelved — needs purchasing epic (PUR) scope first                     |


## Cancelled

Closed — will not be implemented.


| TicketId | Epic                            | Description              | Notes                                    |
| -------- | ------------------------------- | ------------------------ | ---------------------------------------- |
| MOB-006  | [MOB](tickets/MOB-mobile-app.md) | iOS App Store release    | Cancelled — Android-only for v1          |
| RPT-003  | [RPT](tickets/RPT-reporting.md)  | PDF email on schedule    | Cancelled — CSV export enough for v1     |


## Ideas

Unprioritized — not in the active queue. See [IDE-ideas.md](tickets/IDE-ideas.md).


| TicketId | Epic                        | Description                           |
| -------- | --------------------------- | ------------------------------------- |
| IDE-001  | [IDE](tickets/IDE-ideas.md) | Slack notifications for low stock     |
| IDE-002  | [IDE](tickets/IDE-ideas.md) | Barcode label printer integration     |
| IDE-003  | [IDE](tickets/IDE-ideas.md) | Multi-tenant SaaS mode                |


---

## Completed epics (100%)


| Epic                                                         | Description                     | Completed         |
| ------------------------------------------------------------ | ------------------------------- | ----------------- |
| [Foundation (FND)](tickets/FND-foundation-solution.md)       | Repo skeleton, API host, DB     | FND-001 – FND-003 |
| [Authentication (AUT)](tickets/AUT-authentication.md)      | Users, JWT, roles               | AUT-001 – AUT-004 |
| [API Platform (API)](tickets/API-api-platform.md)          | REST conventions, OpenAPI       | API-001 – API-002 |
