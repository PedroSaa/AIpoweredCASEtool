# AICASE Architecture

## Overview

AICASE (AI-powered Computer-Aided Software Engineering) follows a pipeline architecture that transforms a declarative JSON project definition into complete, runnable full-stack code.

---

## High-Level Architecture

```
╔═══════════════════════════════════════════════════════════════════╗
║                     AICASE Generator Pipeline                     ║
╠═══════════════════════════════════════════════════════════════════╣
║                                                                   ║
║  INPUT                                                            ║
║  ┌────────────────────────────────────────────┐                   ║
║  │          proyecto.json                     │                   ║
║  │  {project, globalConfig, entities[]}       │                   ║
║  └──────────────────────┬─────────────────────┘                   ║
║                         │                                         ║
║  STEP 1: VALIDATION     ▼                                         ║
║  ┌────────────────────────────────────────────┐                   ║
║  │          SchemaValidator                   │                   ║
║  │  NJsonSchema + aicase-schema.json          │                   ║
║  │  → validates structure, types, required    │                   ║
║  └──────────────────────┬─────────────────────┘                   ║
║                         │ valid JSON                              ║
║  STEP 2: PARSING        ▼                                         ║
║  ┌────────────────────────────────────────────┐                   ║
║  │          ProjectParser                     │                   ║
║  │  System.Text.Json                          │                   ║
║  │  → ProjectDefinition (internal models)     │                   ║
║  └──────────────────────┬─────────────────────┘                   ║
║                         │ ProjectDefinition                       ║
║  STEP 3: GENERATION     ▼                                         ║
║  ┌────────────────────────────────────────────┐                   ║
║  │          Code Generators                   │                   ║
║  │                                            │                   ║
║  │  ┌──────────────┐  ┌──────────────────┐   │                   ║
║  │  │  Backend     │  │  Frontend        │   │                   ║
║  │  │  Generator   │  │  Generator       │   │                   ║
║  │  │              │  │                  │   │                   ║
║  │  │  .NET 8 C#:  │  │  Angular 17:     │   │                   ║
║  │  │  • Models    │  │  • TS interfaces │   │                   ║
║  │  │  • DTOs      │  │  • Services      │   │                   ║
║  │  │  • Repos     │  │  • List comps    │   │                   ║
║  │  │  • Services  │  │  • Form comps    │   │                   ║
║  │  │  • Ctrl      │  │  • Routing       │   │                   ║
║  │  │  • Validators│  │                  │   │                   ║
║  │  │  • EF Config │  │                  │   │                   ║
║  │  └──────┬───────┘  └───────┬──────────┘   │                   ║
║  │         │                  │              │                   ║
║  │         │   ┌──────────────┘              │                   ║
║  │         │   │  ┌───────────────────────┐  │                   ║
║  │         │   │  │  Database Generator   │  │                   ║
║  │         │   │  │  SQL Server:          │  │                   ║
║  │         │   │  │  • CREATE TABLE       │  │                   ║
║  │         │   │  │  • Stored Procedures  │  │                   ║
║  │         │   │  │  • Master migration   │  │                   ║
║  │         │   │  └───────────┬───────────┘  │                   ║
║  │         │   │              │              │                   ║
║  └─────────┼───┼──────────────┼──────────────┘                   ║
║            │   │              │                                   ║
║  TEMPLATE  ▼   ▼              ▼                                   ║
║  ENGINE    ┌────────────────────────────────────────────┐         ║
║  (Scriban) │          TemplateEngine                   │         ║
║            │  • Loads .scriban files                   │         ║
║            │  • Registers helper functions             │         ║
║            │    - Type mapping (C# / TS / SQL)         │         ║
║            │    - String transforms (PascalCase, etc.) │         ║
║            │  • Renders with ScriptObject              │         ║
║            │  • Writes output files                    │         ║
║            └──────────────────────────────────────────-┘         ║
║                                                                   ║
║  OUTPUT                                                           ║
║  ┌────────────────────────────────────────────┐                   ║
║  │  output/                                   │                   ║
║  │  ├── backend/   (C# .NET 8 Web API)         │                   ║
║  │  ├── frontend/  (Angular 17)                │                   ║
║  │  └── database/  (SQL Server scripts)        │                   ║
║  └────────────────────────────────────────────┘                   ║
╚═══════════════════════════════════════════════════════════════════╝
```

---

## Component Details

### AicaseCli (Entry Point – Program.cs)

Orchestrates the entire pipeline:
1. Parses CLI arguments (`--input`, `--output`, `--templates`, `--verbose`)
2. Calls `SchemaValidator` → stops on errors
3. Calls `ProjectParser` → produces `ProjectDefinition`
4. Creates `TemplateEngine` with templates directory
5. Calls `BackendGenerator`, `FrontendGenerator`, `DatabaseGenerator`
6. Prints summary of generated files

### SchemaValidator

Uses **NJsonSchema** to validate the input JSON against `schema/aicase-schema.json`. Returns a list of validation error messages. If the schema file is not found, validation is skipped with a warning (development convenience).

### ProjectParser

Uses `System.Text.Json.Nodes.JsonNode` (not `[JsonSerializable]`) for flexibility. Walks the JSON tree and produces strongly-typed `ProjectDefinition` objects. Includes computed properties on `EntityDefinition` and `FieldDefinition` (e.g., `CSharpType`, `TypeScriptType`, `SqlType`) that templates use directly.

### TemplateEngine

Wraps the **Scriban** template engine. Key features:
- Template caching (parses each `.scriban` file once)
- Registers helper functions as Scriban callables: `pascal_case`, `camel_case`, `kebab_case`, `csharp_type`, `typescript_type`, `sql_type`, `plural`, `join`
- Uses `ScriptObject.Import(model)` with lowercase member renaming to match Scriban's snake_case convention
- Throws on template parse errors with file path and line numbers

### BackendGenerator

Iterates each entity and calls `TemplateEngine.RenderToFileAsync` for 9 files per entity:

| Template | Output File |
|---|---|
| `backend/model.scriban` | `Models/{Entity}.cs` |
| `backend/dto.scriban` | `DTOs/{Entity}Dto.cs` |
| `backend/repository-interface.scriban` | `Repositories/I{Entity}Repository.cs` |
| `backend/repository.scriban` | `Repositories/{Entity}Repository.cs` |
| `backend/service-interface.scriban` | `Services/I{Entity}Service.cs` |
| `backend/service.scriban` | `Services/{Entity}Service.cs` |
| `backend/controller.scriban` | `Controllers/{Entity}Controller.cs` |
| `backend/validator.scriban` | `Validators/{Entity}Validator.cs` |
| `backend/dbcontext-config.scriban` | `Data/Configurations/{Entity}Configuration.cs` |

### FrontendGenerator

Generates 6 files per entity + 1 routing module:

| Template | Output File |
|---|---|
| `frontend/model.scriban` | `models/{entity}.model.ts` |
| `frontend/service.scriban` | `services/{entity}.service.ts` |
| `frontend/list-component-ts.scriban` | `components/{entity}/{entity}-list/{entity}-list.component.ts` |
| `frontend/list-component-html.scriban` | `components/{entity}/{entity}-list/{entity}-list.component.html` |
| `frontend/form-component-ts.scriban` | `components/{entity}/{entity}-form/{entity}-form.component.ts` |
| `frontend/form-component-html.scriban` | `components/{entity}/{entity}-form/{entity}-form.component.html` |
| `frontend/routing-module.scriban` | `app-routing.module.ts` (once, all entities) |

### DatabaseGenerator

Generates 2 files per entity + 1 master migration:

| Template | Output File |
|---|---|
| `database/create-table.scriban` | `database/tables/Create_{Table}.sql` |
| `database/stored-procedure-crud.scriban` | `database/stored-procedures/sp_{Entity}_CRUD.sql` |
| _(inline C# code)_ | `database/00_master_migration.sql` |

---

## Type Mapping

The type mapping is centralized in `TemplateEngine.cs` static methods and exposed as Scriban helper functions:

| Schema Type | C# Type | SQL Server Type | TypeScript Type |
|---|---|---|---|
| `string` | `string` | `nvarchar(N)` | `string` |
| `int` | `int` | `int` | `number` |
| `decimal` | `decimal` | `decimal(P,S)` | `number` |
| `date` | `DateTime` | `datetime2` | `Date` |
| `bool` | `bool` | `bit` | `boolean` |
| `guid` | `Guid` | `uniqueidentifier` | `string` |

---

## Hybrid AI Approach

Business rules in the project JSON can be marked with `"aiGenerate": true`. The current generator scaffolds a `// TODO (AI-Generate)` comment in place. 

**Step 5 (planned):** The generator will call the OpenAI / Azure OpenAI API with:
- Entity context (fields, types, relations)
- Business rule description and trigger
- Surrounding method context

And replace the TODO comment with generated C# code.

---

## Scriban Template Model

Every template receives an object with these properties:

| Property | Type | Description |
|---|---|---|
| `entity` | `EntityDefinition` | The entity being generated |
| `namespace` | `string` | C# project namespace |
| `project` | `ProjectMetadata` | Project metadata |
| `global_config` | `GlobalConfig` | Auth, roles, database config |
| `all_entities` | `EntityDefinition[]` | All entities (for cross-references) |
| `kebab_name` | `string` | Entity name in kebab-case (frontend only) |

`EntityDefinition` exposes computed helpers:
- `entity.primary_key` – the PK `FieldDefinition`
- `entity.name_lower` – camelCase entity name
- `field.c_sharp_type` – mapped C# type (nullable-aware)
- `field.typescript_type` – mapped TypeScript type
- `field.sql_type` – mapped SQL Server type
- `field.name_pascal` – PascalCase field name

---

## Security Considerations

- JWT secrets are placeholder strings in generated `appsettings.json` — must be replaced before deployment
- Generated controllers apply `[Authorize]` and `[Authorize(Roles = "...")]` based on `apiConfig.roles`
- The auth interceptor in Angular handles 401 responses and redirects to login
- Connection strings use placeholder tokens — never commit real credentials
