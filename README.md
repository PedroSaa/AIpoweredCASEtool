# AICASE - Artificial Intelligence Computer-Aided Software Engineering

> **A**rtificial **I**ntelligence **C**omputer-**A**ided **S**oftware **E**ngineering

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Angular](https://img.shields.io/badge/Angular-17-DD0031?logo=angular)](https://angular.io/)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-2022-CC2927?logo=microsoftsqlserver)](https://www.microsoft.com/sql-server)

---

## ¿Qué es AICASE?

**AICASE** es una herramienta **AI-powered CASE** (Computer-Aided Software Engineering) que genera código de aplicación completo a partir de un único archivo JSON maestro. Usando un **Enfoque Híbrido (Enfoque C)**:

- 🧩 **Plantillas Scriban** para código estructural y repetitivo (modelos, DTOs, repositorios, componentes Angular, scripts SQL)
- 🤖 **IA** para lógica de negocio compleja, validaciones personalizadas y casos especiales

Dado un JSON con la definición completa del proyecto (entidades, campos, validaciones, relaciones, reglas de negocio, configuración de UI y API), AICASE genera automáticamente:

| Capa | Tecnología | Artefactos |
|------|-----------|------------|
| Backend | C# .NET 8 Web API | Modelos, DTOs, Repositorios, Servicios, Controladores, Validadores (FluentValidation), DbContext |
| Frontend | Angular 19 | Interfaces TypeScript, Services, Componentes Listado y Formulario, Routing |
| Base de Datos | SQL Server | CREATE TABLE, Stored Procedures CRUD (opcional) |

---

## Estructura del Proyecto

```
AIpoweredCASEtool/
│
├── 📂 schema/                          # Paso 1: Definición del contrato JSON
│   ├── aicase-schema.json              # JSON Schema formal (Draft-07)
│   └── ejemplo-proyecto.json          # Ejemplo completo (SistemaVentas)
│
├── 📂 docs/                            # Documentación
│   ├── SCHEMA.md                       # Documentación del schema
│   └── ARCHITECTURE.md                 # Arquitectura del generador
│
├── 📂 scaffolding/                     # Paso 2: Proyectos base
│   ├── backend/                        # C# .NET 8 Web API base
│   │   ├── Program.cs
│   │   ├── appsettings.json
│   │   ├── Backend.csproj
│   │   ├── Data/ApplicationDbContext.cs
│   │   ├── Models/         (README)
│   │   ├── DTOs/           (README)
│   │   ├── Repositories/   (README)
│   │   ├── Services/        (README)
│   │   ├── Controllers/    (README)
│   │   └── Validators/     (README)
│   └── frontend/                       # Angular 19 base
│       ├── angular.json
│       ├── package.json
│       └── src/
│           ├── app/
│           │   ├── services/api.service.ts
│           │   ├── interceptors/auth.interceptor.ts
│           │   ├── models/
│           │   ├── components/
│           │   └── guards/
│           └── environments/
│
├── 📂 generator/                       # Paso 3: Motor generador CLI
│   └── AicaseCli/
│       ├── AicaseCli.csproj            # Dependencias: Scriban, NJsonSchema, System.CommandLine
│       ├── Program.cs                  # Punto de entrada CLI
│       ├── Core/
│       │   ├── SchemaValidator.cs      # Validación JSON contra schema
│       │   ├── ProjectParser.cs        # Parseo del JSON a modelos internos
│       │   └── TemplateEngine.cs       # Motor Scriban
│       ├── Generators/
│       │   ├── BackendGenerator.cs     # Genera código C#
│       │   ├── FrontendGenerator.cs    # Genera código Angular
│       │   └── DatabaseGenerator.cs   # Genera scripts SQL
│       └── Models/
│           ├── ProjectDefinition.cs    # Modelos internos
│           ├── EntityDefinition.cs     # Entidad, Campo, Relación, Regla, etc.
│           └── TypeMapper.cs          # Mapeo de tipos entre capas
│
└── 📂 templates/                       # Paso 4: Plantillas Scriban
    ├── backend/
    │   ├── model.scriban               # Clase C# con DataAnnotations
    │   ├── dto.scriban                 # DTOs Create/Update/Response
    │   ├── repository-interface.scriban
    │   ├── repository.scriban          # Implementación EF Core
    │   ├── service-interface.scriban
    │   ├── service.scriban             # Lógica de negocio
    │   ├── controller.scriban          # API REST con [Authorize]
    │   ├── validator.scriban           # FluentValidation
    │   ├── dbcontext-config.scriban    # Fluent API configuration
    │   ├── dbcontext.scriban           # ApplicationDbContext
    │   ├── program.scriban             # Program.cs
    │   ├── csproj.scriban              # .csproj
    │   └── appsettings.scriban         # appsettings.json
    ├── frontend/
    │   ├── model.scriban               # Interface TypeScript
    │   ├── service.scriban             # Angular Service
    │   ├── list-component-ts.scriban   # Componente listado .ts
    │   ├── list-component-html.scriban # Template HTML listado
    │   ├── form-component-ts.scriban   # Componente formulario .ts
    │   ├── form-component-html.scriban # Template HTML formulario
    │   ├── routing-module.scriban      # Routing por entidad
    │   └── app-routing.scriban         # App routing principal
    └── database/
        ├── create-table.scriban        # CREATE TABLE SQL Server
        └── stored-procedure-crud.scriban # SPs CRUD
```

---

## Compilar el CLI

### Prerrequisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- [Node.js 18+](https://nodejs.org/) (para el frontend generado)
- [SQL Server](https://www.microsoft.com/sql-server) (para la BD generada)

### Compilar

```bash
cd generator/AicaseCli
dotnet restore
dotnet build
```

### Publicar como ejecutable independiente

```bash
dotnet publish -c Release -r win-x64 --self-contained true -o ./publish/win
dotnet publish -c Release -r linux-x64 --self-contained true -o ./publish/linux
dotnet publish -c Release -r osx-x64 --self-contained true -o ./publish/macos
```

---

## Cómo Usar AICASE

### 1. Preparar el JSON del proyecto

Crea tu archivo `mi-proyecto.json` siguiendo el [schema documentado](docs/SCHEMA.md).
Puedes partir del [ejemplo completo](schema/ejemplo-proyecto.json).

### 2. Ejecutar el generador

```bash
# Forma básica (usa el schema embebido)
dotnet run --project generator/AicaseCli -- --input schema/ejemplo-proyecto.json

# Especificando carpeta de salida
dotnet run --project generator/AicaseCli -- \
  --input schema/ejemplo-proyecto.json \
  --output ./mi-proyecto-generado

# Especificando schema personalizado
dotnet run --project generator/AicaseCli -- \
  --input mi-proyecto.json \
  --output ./output \
  --schema schema/aicase-schema.json
```

### 3. Revisar la salida generada

```
output/
├── backend/          ← Proyecto C# .NET 8
├── frontend/         ← Proyecto Angular 19
└── database/         ← Scripts SQL Server
```

### 4. Compilar y ejecutar el backend generado

```bash
cd output/backend
dotnet restore
dotnet ef database update   # Si usas EF Migrations
dotnet run
```

### 5. Instalar y ejecutar el frontend generado

```bash
cd output/frontend
npm install
ng serve
```

### 6. Ejecutar scripts de BD

```bash
# En SQL Server Management Studio o sqlcmd:
sqlcmd -S localhost -i output/database/00_CreateDatabase.sql
sqlcmd -S localhost -d MiBaseDatos -i output/database/Create_Clientes.sql
```

---

## Ejemplo Paso a Paso

El archivo [`schema/ejemplo-proyecto.json`](schema/ejemplo-proyecto.json) define el **SistemaVentas** con 3 entidades:

| Entidad | Tabla | Descripción |
|---------|-------|-------------|
| `TipoCliente` | `TiposCliente` | Clasificación de clientes (Regular, VIP, etc.) |
| `Cliente` | `Clientes` | Clientes con relación a TipoCliente |
| `Producto` | `Productos` | Catálogo de productos con stock |

Ejecutar:

```bash
dotnet run --project generator/AicaseCli -- \
  --input schema/ejemplo-proyecto.json \
  --output ./output/SistemaVentas
```

Genera automáticamente **+40 archivos** de código listo para usar.

---

## Roadmap

| Paso | Estado | Descripción |
|------|--------|-------------|
| **1** | ✅ Completado | Schema JSON formal y ejemplo completo |
| **2** | ✅ Completado | Scaffolding base C# + Angular |
| **3** | ✅ Completado | Motor generador CLI (C#, Scriban, NJsonSchema) |
| **4** | ✅ Completado | Plantillas para todos los artefactos |
| **5** | 🔲 Pendiente | Integración con IA (OpenAI/Claude) para lógica compleja |
| **6** | 🔲 Pendiente | Testing automático del código generado |
| **7** | 🔲 Pendiente | Interfaz web (UI) para el generador |
| **8** | 🔲 Pendiente | Soporte para otros stacks (Java/Spring, Python/FastAPI) |

---

## Contribuir

1. Fork del repositorio
2. Crear rama feature: `git checkout -b feature/nueva-funcionalidad`
3. Commit: `git commit -m "feat: descripción"`
4. Push: `git push origin feature/nueva-funcionalidad`
5. Pull Request

---

## Licencia

MIT - Ver [LICENSE](LICENSE) para detalles.