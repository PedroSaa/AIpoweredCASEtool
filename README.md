# AICASE – AI-powered Computer-Aided Software Engineering

> **Generate production-ready full-stack code from a single JSON definition file.**

AICASE is an AI-powered CASE (Computer-Aided Software Engineering) tool that takes a structured project definition JSON and generates complete, compilable code for your entire application — backend, frontend, and database — in seconds.

---

## What is AICASE?

Traditional CASE tools generate boilerplate code that requires significant manual effort to complete. AICASE uses a **Hybrid Approach** that combines template-based generation for structural/repetitive code with AI-assisted generation for complex business logic — giving you the best of both worlds.

### Hybrid Approach (Approach C)

```
┌─────────────────────────────────────────────────────────────┐
│                     Project JSON Input                       │
└────────────────────────────┬────────────────────────────────┘
                             │
                ┌────────────▼────────────┐
                │     Schema Validator    │  ← JSON Schema draft-07
                └────────────┬────────────┘
                             │
                ┌────────────▼────────────┐
                │     Project Parser      │  ← System.Text.Json
                └────────────┬────────────┘
                             │
           ┌─────────────────┼──────────────────┐
           │                 │                  │
  ┌────────▼────────┐  ┌─────▼──────┐  ┌───────▼───────┐
  │ Template-Based  │  │ Template-  │  │  Template-    │
  │ Backend Gen     │  │ Based      │  │  Based DB     │
  │ (Scriban)       │  │ Frontend   │  │  Generation   │
  └────────┬────────┘  └─────┬──────┘  └───────┬───────┘
           │                 │                  │
  ┌────────▼────────┐        │                  │
  │  AI Generation  │        │                  │
  │  (Step 5 - WIP) │        │                  │
  │  Complex rules  │        │                  │
  └─────────────────┘        │                  │
           │                 │                  │
           └─────────────────▼──────────────────┘
                             │
                ┌────────────▼────────────┐
                │    Generated Output     │
                │  backend/ frontend/     │
                │  database/              │
                └─────────────────────────┘
```

**What templates handle (fast, deterministic):**
- C# Models, DTOs, Repositories, Services, Controllers
- FluentValidation validators
- EF Core entity configurations
- Angular TypeScript interfaces, services, components
- SQL CREATE TABLE scripts and stored procedures

**What AI handles (Step 5, coming soon):**
- Complex business rules flagged with `"aiGenerate": true`
- Custom validation logic
- Domain-specific calculations

---

## Target Stack

| Layer | Technology |
|---|---|
| Backend API | C# .NET 8 Web API |
| ORM | Entity Framework Core 8 |
| Validation | FluentValidation |
| Documentation | Swagger / OpenAPI |
| Authentication | JWT Bearer |
| Frontend | Angular 19 + Angular Material |
| Database | SQL Server |
| Template Engine | Scriban |

---

## Project Structure

```
AIpoweredCASEtool/
├── schema/
│   ├── aicase-schema.json        ← JSON Schema (draft-07) for project validation
│   └── ejemplo-proyecto.json     ← Complete example project (SistemaVentas)
│
├── docs/
│   ├── SCHEMA.md                 ← Schema property reference
│   └── ARCHITECTURE.md           ← Architecture diagrams
│
├── scaffolding/
│   ├── backend/                  ← Base .NET 8 project scaffolding
│   └── frontend/                 ← Base Angular 19 scaffolding
│
├── generator/
│   └── AicaseCli/                ← .NET 8 CLI code generator
│       ├── Program.cs
│       ├── Core/                 ← SchemaValidator, ProjectParser, TemplateEngine
│       ├── Models/               ← Internal model classes
│       └── Generators/           ← BackendGenerator, FrontendGenerator, DatabaseGenerator
│
└── templates/
    ├── backend/                  ← 9 Scriban templates (.NET 8)
    ├── frontend/                 ← 7 Scriban templates (Angular 19)
    └── database/                 ← 2 Scriban templates (SQL Server)
```

---

## Quick Start

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- [Node.js 18+](https://nodejs.org/) (for Angular frontend)

### 1. Build the CLI

```bash
cd generator/AicaseCli
dotnet restore
dotnet build
```

### 2. Generate from the example project

```bash
dotnet run --project generator/AicaseCli -- \
  --input schema/ejemplo-proyecto.json \
  --output ./output/SistemaVentas \
  --verbose
```

### 3. What gets generated

```
output/SistemaVentas/
├── backend/
│   ├── Models/          ← Cliente.cs, Producto.cs, TipoCliente.cs
│   ├── DTOs/            ← ClienteDto.cs, ProductoDto.cs, TipoClienteDto.cs
│   ├── Repositories/    ← IClienteRepository.cs + ClienteRepository.cs × 3
│   ├── Services/        ← IClienteService.cs + ClienteService.cs × 3
│   ├── Controllers/     ← ClienteController.cs × 3
│   ├── Validators/      ← ClienteValidator.cs × 3
│   └── Data/Configurations/ ← ClienteConfiguration.cs × 3
├── frontend/src/app/
│   ├── models/          ← cliente.model.ts × 3
│   ├── services/        ← cliente.service.ts × 3
│   └── components/      ← list + form components × 3 entities
└── database/
    ├── tables/          ← Create_Clientes.sql × 3
    ├── stored-procedures/ ← sp_Cliente_CRUD.sql × 3
    └── 00_master_migration.sql
```

---

## CLI Reference

```
aicase --input <project.json> --output <dir> [options]

  -i, --input      <path>   Project definition JSON   (required)
  -o, --output     <path>   Output directory           (required)
  -t, --templates  <path>   Custom templates directory
  -v, --verbose             Verbose output
  -h, --help                Show help
```

---

## Roadmap

| Step | Status | Description |
|---|---|---|
| 1 | ✅ Done | JSON Schema (draft-07) for project definitions |
| 2 | ✅ Done | Base scaffolding for .NET 8 + Angular 19 |
| 3 | ✅ Done | Generator CLI with schema validation and parsing |
| 4 | ✅ Done | Scriban templates for all layers |
| 5 | 🔄 Planned | AI integration for complex business rule implementation |
| 6 | 📋 Planned | Automated tests for generator and generated code |

---

## License

MIT License
