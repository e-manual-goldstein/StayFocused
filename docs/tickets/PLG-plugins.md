# Epic PLG — Plugins

**Project:** StayFocused
**Code:** `PLG`
**Scope:** Load external DLL modules at startup via `plugin-config.json`; plugins can register services and run hooks after DI is built.

**Depends on:** FND-001, FND-003
**Blocks:** —

---

## Primary user story

> I can extend StayFocused without forking the main app — drop in a plugin DLL and configure it in JSON.

---

## Ticket summary

| ID | Status | Title | Depends on |
|----|--------|-------|------------|
| [PLG-001](#plg-001) | Done | PluginService and plugin-config.json loading | FND-001, FND-003 |
| [PLG-002](#plg-002) | Shelved | Example plugin registers a sample handler |

---

## Design notes

### Interface (`IPlugin`)

- `PluginName`
- `OnPluginLoaded(IServiceCollection)` — before `BuildServiceProvider`
- `OnServicesBuilt(IServiceProvider)` — after build

### Loading flow (`App.WithPlugins`)

1. Build interim provider → `PluginService.Initialise()`
2. Each plugin `OnPluginLoaded` may modify `ServiceCollection`
3. Rebuild provider if needed
4. Each plugin `OnServicesBuilt`

### Config

`plugin-config.json` — dictionary of `PluginDefinition` (Name, FullTypeName, ModuleLocation).

`PluginService` reads path from `ConfigManager.GetConfigSetting("PluginConfig")`.

### ExampleModule

`ExamplePlugin` — empty stubs; demonstrates project reference to `StayFocused.Api`.

### Out of scope (epic v1)

- Plugin hot-reload
- Sandboxed plugin security model

---

## Tickets

### PLG-001

| Field | Detail |
|-------|--------|
| **ID** | PLG-001 |
| **Title** | PluginService and plugin-config.json loading |
| **Status** | Done |
| **Description** | `PluginService` deserialises config, `Assembly.LoadFrom`, instantiates `IPlugin` types, integrates with `App` startup pipeline. |
| **Test / demo** | Valid `plugin-config.json` pointing to built `ExampleModule.dll` → app starts without load errors. |
| **Depends on** | FND-001, FND-003 |

### PLG-002

| Field | Detail |
|-------|--------|
| **ID** | PLG-002 |
| **Title** | Example plugin registers a sample handler |
| **Status** | Shelved |
| **Description** | Extend `ExamplePlugin` to register demo handler. Shelved 2026-08-27 — not primary goal. |
| **Test / demo** | Enable ExampleModule in config → app starts → console/log shows plugin registered handler (or handler fires for test process). |
| **Depends on** | PLG-001, HND-001 |
