# Epic INV — Inventory Management

**Project:** Acme Warehouse (example)
**Code:** `INV`
**Scope:** Track **inventory items** across locations — SKU, quantity on hand, last counted date. Mobile app for floor counts; desktop for bulk edits and reports.

**Depends on:** FND, AUT, API-001
**Blocks:** RPT (reporting needs inventory data)

---

## Primary user story

> I need to know what's on the shelf without opening a spreadsheet. I scan a barcode on my phone, adjust the count, and everyone sees the updated quantity. Managers fix mistakes on desktop.

---

## Ticket summary

| ID | Status | Title | Depends on |
|----|--------|-------|------------|
| [INV-001](#inv-001) | Done | Inventory item model, API, and permissions (v1) | FND-003, API-001, AUT-002 |
| [INV-002](#inv-002) | Done | MAUI — scan item barcode and adjust quantity | INV-001, MOB-001 |
| [INV-003](#inv-003) | Todo | WPF — inventory management view with bulk edit | INV-001, DSK-001 |
| [INV-004](#inv-004) | Shelved | Auto-reorder when below threshold | INV-001, PUR-001 *(future)* |

---

## Design notes

### Entity (`InventoryItem`)

| Field | Type | Notes |
|-------|------|--------|
| `Id` | `Guid` | PK |
| `Sku` | `string` | Required, unique per tenant |
| `Name` | `string` | Required |
| `QuantityOnHand` | `int` | Non-negative |
| `LocationCode` | `string?` | Aisle / bin |
| `LastCountedAt` | `DateTimeOffset?` | Set on mobile count |
| `CreatedAt` | `DateTimeOffset` | UTC |
| `UpdatedAt` | `DateTimeOffset?` | Last mutation |

**Validation (v1):** non-empty `Sku` and `Name`; `QuantityOnHand` ≥ 0.

### Permissions

| Permission | Use |
|------------|-----|
| `inventory:read` | List and view items |
| `inventory:write` | Create, update quantities |
| `inventory:delete` | Remove items (admin only) |

### API sketch — `/api/v1/inventory-items`

| Method | Route | Policy | Purpose |
|--------|-------|--------|---------|
| GET | `/api/v1/inventory-items` | `inventory:read` | List with optional `location` filter |
| POST | `/api/v1/inventory-items` | `inventory:write` | Create item |
| PATCH | `/api/v1/inventory-items/{id}` | `inventory:write` | Update quantity, name, location |
| DELETE | `/api/v1/inventory-items/{id}` | `inventory:delete` | Hard delete v1 |

### Out of scope (epic v1)

- Purchase orders / auto-reorder (INV-004 shelved)
- Multi-warehouse transfers
- External ERP sync

---

## Tickets

### INV-001

| Field | Detail |
|-------|--------|
| **ID** | INV-001 |
| **Title** | Inventory item model, API, and permissions (v1) |
| **Status** | Done |
| **Description** | Added `InventoryItem` entity, migration, DTOs, CRUD endpoints, permission policies, integration tests. |
| **Test / demo** | POST item → GET list includes it; PATCH quantity → persists after restart. |
| **Depends on** | FND-003, API-001, AUT-002 |

### INV-002

| Field | Detail |
|-------|--------|
| **ID** | INV-002 |
| **Title** | MAUI — scan item barcode and adjust quantity |
| **Status** | Done |
| **Description** | Home tile (`inventory:read`). Barcode scan page; lookup by SKU; increment/decrement with Save. |
| **Test / demo** | Scan SKU → adjust +1 → desktop list shows new quantity after refresh. |
| **Depends on** | INV-001, MOB-001 |

### INV-003

| Field | Detail |
|-------|--------|
| **ID** | INV-003 |
| **Title** | WPF — inventory management view with bulk edit |
| **Status** | Todo |
| **Description** | Sidebar **Inventory**. DataGrid list; detail pane for edit; multi-select bulk quantity adjust. Theme brushes; empty state. |
| **Test / demo** | Desktop: create item → appears on mobile; bulk select 3 rows → add 10 to each → quantities update. |
| **Depends on** | INV-001, DSK-001 |

### INV-004

| Field | Detail |
|-------|--------|
| **ID** | INV-004 |
| **Title** | Auto-reorder when below threshold |
| **Status** | Shelved |
| **Description** | When `QuantityOnHand` < `ReorderPoint`, create draft purchase request. |
| **Test / demo** | Set reorder point 5, quantity 4 → draft PO appears. |
| **Depends on** | INV-001, PUR-001 *(purchasing epic not started)* |
