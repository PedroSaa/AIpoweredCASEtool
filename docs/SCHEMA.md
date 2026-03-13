# AICASE Schema Documentation

## Table of Contents
1. [Overview](#overview)
2. [Root Object](#root-object)
3. [project](#project)
4. [globalConfig](#globalconfig)
5. [entities](#entities)
6. [field](#field)
7. [fieldValidation](#fieldvalidation)
8. [uiInput](#uiinput)
9. [relation](#relation)
10. [businessRule](#businessrule)
11. [screenConfig](#screenconfig)
12. [apiConfig](#apiconfig)
13. [Type Mappings](#type-mappings)
14. [Complete Example](#complete-example)

---

## Overview

The AICASE schema (`aicase-schema.json`) defines the structure of project definition files consumed by the `AicaseCli` generator. A single JSON file describes the entire application — entities, validations, business rules, UI configuration, and API settings — from which full-stack code is generated.

**Schema version:** JSON Schema Draft-07  
**File extension:** `.json`

---

## Root Object

| Property | Type | Required | Description |
|---|---|---|---|
| `project` | object | ✅ | Project metadata |
| `globalConfig` | object | ❌ | Auth, roles, database, CORS settings |
| `entities` | array | ✅ | Domain entities (minimum 1) |

---

## project

Project metadata and technology stack selection.

| Property | Type | Required | Description | Example |
|---|---|---|---|---|
| `name` | string | ✅ | Project/namespace name | `"SistemaVentas"` |
| `version` | string | ✅ | Semantic version (x.y.z) | `"1.0.0"` |
| `description` | string | ❌ | Human-readable description | `"Sistema de ventas"` |
| `technologies.backend` | enum | ❌ | Backend stack | `"dotnet8"` |
| `technologies.frontend` | enum | ❌ | Frontend stack | `"angular17"` |
| `technologies.database` | enum | ❌ | Database engine | `"sqlserver"` |

### Allowed Technology Values

**backend:** `dotnet8`, `dotnet6`, `java-spring`, `node-express`  
**frontend:** `angular17`, `angular16`, `react18`, `vue3`  
**database:** `sqlserver`, `postgresql`, `mysql`, `sqlite`

```json
"project": {
  "name": "SistemaVentas",
  "version": "1.0.0",
  "description": "Sistema de gestión de ventas",
  "technologies": {
    "backend": "dotnet8",
    "frontend": "angular17",
    "database": "sqlserver"
  }
}
```

---

## globalConfig

Global configuration applied across the entire generated project.

### globalConfig.authentication

| Property | Type | Default | Description |
|---|---|---|---|
| `enabled` | boolean | `true` | Enable authentication |
| `type` | enum | `"jwt"` | Auth type: `jwt`, `oauth2`, `apikey` |
| `jwtSecret` | string | — | JWT signing secret (use placeholder in dev) |
| `jwtExpireMinutes` | integer | `60` | Token expiry in minutes |
| `refreshTokenEnabled` | boolean | `true` | Enable refresh tokens |

### globalConfig.roles

Array of role objects:

| Property | Type | Required | Description |
|---|---|---|---|
| `name` | string | ✅ | Role name (e.g., `"Admin"`) |
| `description` | string | ❌ | Human-readable description |
| `isDefault` | boolean | — | Assign this role by default to new users |

### globalConfig.database

| Property | Type | Default | Description |
|---|---|---|---|
| `schema` | string | `"dbo"` | Database schema name |
| `connectionStringPlaceholder` | string | — | Template connection string |
| `enableMigrations` | boolean | `true` | Generate EF Core migrations |
| `enableSeedData` | boolean | `false` | Generate seed data scripts |

### globalConfig.cors

| Property | Type | Description |
|---|---|---|
| `allowedOrigins` | string[] | List of allowed CORS origins |

---

## entities

An array of entity objects. Each entity maps to a database table, a C# model, and Angular components.

| Property | Type | Required | Description |
|---|---|---|---|
| `name` | string | ✅ | Entity class name (PascalCase) |
| `table` | string | ✅ | Database table name |
| `description` | string | ❌ | Entity description |
| `fields` | array | ✅ | Entity fields (minimum 1) |
| `relations` | array | ❌ | Entity relationships |
| `businessRules` | array | ❌ | Business logic rules |
| `screenConfig` | object | ❌ | UI list/form configuration |
| `apiConfig` | object | ❌ | REST API configuration |

---

## field

Defines a single field within an entity.

| Property | Type | Required | Description |
|---|---|---|---|
| `name` | string | ✅ | Field name (camelCase) |
| `type` | enum | ✅ | Logical type (see Type Mappings below) |
| `length` | integer | ❌ | Max length for string fields |
| `precision` | integer | ❌ | Total digits for decimal |
| `scale` | integer | ❌ | Decimal places for decimal |
| `required` | boolean | — | Marks field as NOT NULL |
| `unique` | boolean | — | Add UNIQUE constraint |
| `isPK` | boolean | — | Designate as primary key |
| `autoIncrement` | boolean | — | Auto-increment (IDENTITY) |
| `defaultValue` | any | ❌ | Default value |
| `description` | string | ❌ | Field documentation |
| `validation` | object | ❌ | Validation rules (see below) |
| `ui` | object | ❌ | UI input configuration (see below) |

### Allowed field types

| Type | C# | SQL Server | TypeScript |
|---|---|---|---|
| `string` | `string` | `nvarchar` | `string` |
| `int` | `int` | `int` | `number` |
| `decimal` | `decimal` | `decimal` | `number` |
| `date` | `DateTime` | `datetime2` | `Date` |
| `bool` | `bool` | `bit` | `boolean` |
| `guid` | `Guid` | `uniqueidentifier` | `string` |

---

## fieldValidation

Validation rules applied at both API (FluentValidation) and frontend (Angular reactive forms) level.

| Property | Type | Description | Example |
|---|---|---|---|
| `minLength` | integer | Minimum string length | `3` |
| `maxLength` | integer | Maximum string length | `200` |
| `min` | number | Minimum numeric/date value | `0` |
| `max` | number | Maximum numeric/date value | `100` |
| `email` | boolean | Validate as email address | `true` |
| `regex` | string | Regular expression pattern | `"^[A-Z0-9]+$"` |
| `regexMessage` | string | Custom message for regex failure | `"Solo letras y números"` |
| `maxDate` | string | ISO date or `"today"` | `"today"` |
| `minDate` | string | ISO date or `"today"` | `"2020-01-01"` |
| `custom` | string | C# expression or service method name | `"ValidarRUC"` |

```json
"validation": {
  "minLength": 3,
  "maxLength": 50,
  "regex": "^[A-Z0-9\\-]+$",
  "regexMessage": "Solo letras mayúsculas, números y guiones"
}
```

---

## uiInput

Controls how a field is rendered in Angular forms.

| Property | Type | Description |
|---|---|---|
| `type` | enum | Input widget type (see below) |
| `label` | string | Display label |
| `placeholder` | string | Input placeholder |
| `hint` | string | Helper text below the input |
| `readonly` | boolean | Render as read-only |
| `hidden` | boolean | Hide from form |
| `dataSource` | object | Options source for dropdowns/autocomplete |

### Allowed input types

| Type | Angular Component | Use For |
|---|---|---|
| `text` | `mat-input` | Short text fields |
| `email` | `mat-input` type=email | Email addresses |
| `number` | `mat-input` type=number | Numeric values |
| `datepicker` | `mat-datepicker` | Date/DateTime fields |
| `dropdown` | `mat-select` | Enumerated or FK values |
| `checkbox` | `mat-checkbox` | Boolean fields |
| `textarea` | `mat-textarea` | Long text |
| `radio` | `mat-radio-group` | Small fixed option sets |
| `autocomplete` | `mat-autocomplete` | Searchable FK dropdowns |
| `password` | `mat-input` type=password | Passwords |

### dataSource

| Property | Description |
|---|---|
| `entity` | Load options from another entity via API |
| `valueField` | Field to use as option value |
| `labelField` | Field to display as option label |
| `staticOptions` | Hard-coded `[{value, label}]` array |

---

## relation

Defines a relationship between two entities.

| Property | Type | Required | Description |
|---|---|---|---|
| `type` | enum | ✅ | `ManyToOne`, `OneToMany`, `ManyToMany` |
| `target` | string | ✅ | Target entity name |
| `field` | string | ❌ | FK field on this entity |
| `targetField` | string | ❌ | Field on target (usually PK) |
| `joinTable` | string | ❌ | Join table name (ManyToMany only) |
| `cascade` | boolean | — | Enable cascade delete |
| `nullable` | boolean | — | FK is nullable |
| `eager` | boolean | — | Eager-load navigation property |

```json
"relations": [
  {
    "type": "ManyToOne",
    "target": "TipoCliente",
    "field": "tipoClienteId",
    "targetField": "id",
    "cascade": false,
    "nullable": false
  }
]
```

---

## businessRule

Encodes a business logic rule that the generator will scaffold (and optionally ask AI to implement).

| Property | Type | Required | Description |
|---|---|---|---|
| `name` | string | ✅ | Rule identifier (camelCase) |
| `description` | string | ❌ | Human-readable description |
| `trigger` | enum | ✅ | When the rule fires (see below) |
| `condition` | string | ❌ | C# condition expression |
| `action` | enum | ❌ | What to do when condition is true |
| `message` | string | ❌ | Error or notification message |
| `targetField` | string | ❌ | Field to set (for `setField` action) |
| `targetValue` | any | ❌ | Value to assign (for `setField`) |
| `aiGenerate` | boolean | — | Let AI generate the implementation |

### Trigger values

| Trigger | When |
|---|---|
| `beforeInsert` | Before persisting a new record |
| `afterInsert` | After a new record is saved |
| `beforeUpdate` | Before updating an existing record |
| `afterUpdate` | After an update is saved |
| `beforeDelete` | Before deleting a record |
| `afterDelete` | After a record is deleted |
| `onValidate` | During model validation |
| `onLoad` | When entity is loaded from database |

### Action values

| Action | Description |
|---|---|
| `throw` | Throw a business exception with `message` |
| `log` | Log an entry |
| `setField` | Set `targetField` to `targetValue` |
| `callService` | Call an external service |
| `sendNotification` | Send email/push notification |
| `custom` | Custom code (AI-generated if `aiGenerate: true`) |

---

## screenConfig

Configures the generated Angular list and form screens.

### screenConfig.list

| Property | Description |
|---|---|
| `title` | Screen/page title |
| `columns` | Array of columns to display |
| `actions` | Row-level action buttons |
| `filters` | Filter controls above the grid |
| `pagination` | Pagination settings |
| `sort` | Default sort field and direction |

#### column object

| Property | Type | Description |
|---|---|---|
| `field` | string | Entity field name |
| `header` | string | Column header text |
| `sortable` | boolean | Enable column sorting |
| `filterable` | boolean | Enable column filter |
| `width` | string | CSS width (e.g., `"120px"`) |
| `pipe` | string | Angular pipe expression |

### screenConfig.form

| Property | Description |
|---|---|
| `title` | Form title |
| `layout` | `single-column`, `two-columns`, `three-columns`, `sectioned` |
| `sections` | Array of named field groups (for `sectioned` layout) |
| `fields` | Field-level overrides (colspan, order) |

---

## apiConfig

Configures the generated REST API controller.

| Property | Type | Description |
|---|---|---|
| `route` | string | API route prefix, e.g. `/api/v1/clientes` |
| `operations` | string[] | Enabled endpoints: `getAll`, `getById`, `create`, `update`, `delete`, `search`, `export` |
| `authentication` | boolean | Require JWT authentication |
| `versioning` | string | API version string, e.g. `"v1"` |
| `roles` | object | Per-operation role restrictions |
| `rateLimiting.enabled` | boolean | Enable rate limiting |
| `rateLimiting.requestsPerMinute` | integer | Max requests per minute |

---

## Type Mappings

| Schema Type | C# Type | SQL Server Type | TypeScript Type | Angular Validator |
|---|---|---|---|---|
| `string` | `string` | `nvarchar(N)` | `string` | `Validators.maxLength(N)` |
| `int` | `int` | `int` | `number` | — |
| `decimal` | `decimal` | `decimal(P,S)` | `number` | — |
| `date` | `DateTime` | `datetime2` | `Date` | — |
| `bool` | `bool` | `bit` | `boolean` | — |
| `guid` | `Guid` | `uniqueidentifier` | `string` | — |

---

## Complete Example

Minimal working example:

```json
{
  "project": {
    "name": "MiProyecto",
    "version": "1.0.0"
  },
  "entities": [
    {
      "name": "Producto",
      "table": "Productos",
      "fields": [
        { "name": "id",     "type": "int",     "isPK": true, "autoIncrement": true, "required": true },
        { "name": "nombre", "type": "string",  "length": 200, "required": true,
          "validation": { "minLength": 2, "maxLength": 200 },
          "ui": { "type": "text", "label": "Nombre del Producto" }
        },
        { "name": "precio", "type": "decimal", "precision": 18, "scale": 2, "required": true,
          "validation": { "min": 0.01 },
          "ui": { "type": "number", "label": "Precio" }
        }
      ],
      "apiConfig": {
        "route": "/api/v1/productos",
        "operations": ["getAll","getById","create","update","delete"],
        "authentication": true
      }
    }
  ]
}
```

See `schema/ejemplo-proyecto.json` for a full real-world example with all features used.
