# AICASE — Arquitectura del Sistema

Este documento describe la arquitectura interna del generador de código AICASE, los componentes principales, el flujo de datos y las decisiones tecnológicas.

---

## Diagrama de Componentes

```
┌─────────────────────────────────────────────────────────────────────┐
│                          AICASE CLI                                  │
│                     (generator/AicaseCli)                            │
│                                                                      │
│  ┌─────────────┐    ┌───────────────┐    ┌──────────────────────┐  │
│  │   Program   │───▶│ SchemaValidator│───▶│   ProjectParser      │  │
│  │  (CLI Entry)│    │ (NJsonSchema)  │    │ (Newtonsoft.Json)    │  │
│  └─────────────┘    └───────────────┘    └──────────┬───────────┘  │
│                                                      │               │
│                                          ProjectDefinition           │
│                                                      │               │
│                              ┌───────────────────────┼───────────┐  │
│                              │                       │           │  │
│                              ▼                       ▼           ▼  │
│                    ┌─────────────────┐  ┌──────────────┐  ┌─────┐  │
│                    │BackendGenerator │  │FrontendGen.  │  │ DB  │  │
│                    │  (.NET 8 C#)   │  │ (Angular 17) │  │ Gen │  │
│                    └────────┬────────┘  └──────┬───────┘  └──┬──┘  │
│                             │                  │              │     │
│                             └──────────────────┴──────────────┘     │
│                                          │                          │
│                              ┌───────────▼──────────┐              │
│                              │    TemplateEngine     │              │
│                              │       (Scriban)       │              │
│                              └───────────┬───────────┘              │
└──────────────────────────────────────────┼──────────────────────────┘
                                           │
              ┌────────────────────────────▼─────────────────────────┐
              │                   templates/                          │
              │  ┌────────────┐  ┌─────────────┐  ┌──────────────┐  │
              │  │  backend/  │  │  frontend/  │  │  database/   │  │
              │  │ *.scriban  │  │  *.scriban  │  │  *.scriban   │  │
              │  └────────────┘  └─────────────┘  └──────────────┘  │
              └──────────────────────────────────────────────────────┘
                                           │
              ┌────────────────────────────▼─────────────────────────┐
              │                    output/                            │
              │  ┌────────────┐  ┌─────────────┐  ┌──────────────┐  │
              │  │  backend/  │  │  frontend/  │  │  database/   │  │
              │  │   *.cs     │  │   *.ts      │  │   *.sql      │  │
              │  └────────────┘  └─────────────┘  └──────────────┘  │
              └──────────────────────────────────────────────────────┘
```

---

## Flujo de Datos: De JSON a Código Generado

```
Archivo JSON del Proyecto
         │
         ▼
  ① Lectura del archivo
         │
         ▼
  ② SchemaValidator.ValidateAsync()
     ├── Lee aicase-schema.json
     ├── Valida con NJsonSchema
     └── Retorna (IsValid, Errores[])
         │ (si es válido)
         ▼
  ③ ProjectParser.Parse()
     ├── Deserializa JSON → ProjectDefinition
     ├── Valida campos requeridos
     └── Normaliza relaciones de campos
         │
         ▼
  ④ Generadores (en paralelo conceptual)
     │
     ├── BackendGenerator.GenerateAsync()
     │   ├── Por cada EntityDefinition:
     │   │   ├── Modelo C# (model.scriban)
     │   │   ├── DTOs (dto.scriban)
     │   │   ├── Repositorio + Interfaz
     │   │   ├── Servicio + Interfaz
     │   │   ├── Controlador REST
     │   │   ├── Validador FluentValidation
     │   │   └── Configuración EF Core
     │   └── DbContext snippet
     │
     ├── FrontendGenerator.GenerateAsync()
     │   ├── Por cada EntityDefinition:
     │   │   ├── Interface TypeScript
     │   │   ├── Angular Service
     │   │   ├── Componente Lista (.ts + .html)
     │   │   └── Componente Formulario (.ts + .html)
     │   ├── AppRoutingModule
     │   └── AppModule
     │
     └── DatabaseGenerator.GenerateAsync()
         ├── 01_CreateTables.sql (todas las entidades)
         ├── Por cada EntityDefinition:
         │   └── {Entidad}_CRUD.sql (5 stored procedures)
         └── 02_SeedData.sql
         │
         ▼
  ⑤ TemplateEngine.RenderAsync() / RenderToFileAsync()
     ├── Busca plantilla .scriban en disco
     ├── Si no existe → usa plantilla embebida
     ├── Parsea la plantilla con Scriban
     ├── Registra funciones: csharp_type, typescript_type,
     │   sql_type, to_camel_case, to_plural, to_kebab_case
     ├── Renderiza el modelo con la plantilla
     └── Escribe el archivo de salida
         │
         ▼
  ⑥ Archivos de código generados
     output/
     ├── backend/
     │   ├── Models/        → {Entidad}.cs
     │   ├── DTOs/          → {Entidad}Dtos.cs
     │   ├── Repositories/  → I{Entidad}Repository.cs, {Entidad}Repository.cs
     │   ├── Services/      → I{Entidad}Service.cs, {Entidad}Service.cs
     │   ├── Controllers/   → {Entidad}Controller.cs
     │   ├── Validators/    → {Entidad}Validator.cs
     │   └── Data/
     │       ├── Configurations/ → {Entidad}Configuration.cs
     │       └── DbContextSnippet.txt
     ├── frontend/
     │   └── src/app/
     │       ├── models/    → {entidad}.model.ts
     │       ├── services/  → {entidad}.service.ts
     │       ├── components/{entidad}/
     │       │   ├── list/  → {entidad}-list.component.ts/.html
     │       │   └── form/  → {entidad}-form.component.ts/.html
     │       ├── app-routing.module.ts
     │       └── app.module.ts
     └── database/
         ├── 01_CreateTables.sql
         ├── 02_SeedData.sql
         └── procedures/
             └── {Entidad}_CRUD.sql
```

---

## Descripción de Componentes

### SchemaValidator

**Ubicación:** `generator/AicaseCli/Core/SchemaValidator.cs`

**Responsabilidad:** Valida que el JSON de entrada sea conforme al esquema `aicase-schema.json` antes de procesarlo.

**Tecnología:** [NJsonSchema](https://github.com/RicoSuter/NJsonSchema) - biblioteca .NET para validación y generación de esquemas JSON.

**Motivo de elección:** NJsonSchema soporta JSON Schema draft-07, es mantenida activamente, y devuelve errores detallados con ruta y tipo de error.

**Método principal:**
```csharp
Task<(bool IsValid, ICollection<string> Errors)> ValidateAsync(
    string jsonContent, string schemaPath)
```

---

### ProjectParser

**Ubicación:** `generator/AicaseCli/Core/ProjectParser.cs`

**Responsabilidad:** Deserializa el JSON validado en el modelo de objetos C# (`ProjectDefinition` y sus tipos relacionados).

**Tecnología:** [Newtonsoft.Json](https://www.newtonsoft.com/json) - biblioteca de serialización JSON más completa y flexible de .NET.

**Motivo de elección:** Newtonsoft.Json permite mapeo flexible con `[JsonProperty]`, maneja tipos complejos como `object?` para valores de validación, y tiene mejor soporte para casos edge que `System.Text.Json`.

**Normalización:** Consolida las relaciones definidas a nivel de campo en la lista de relaciones de la entidad para simplificar el procesamiento en los generadores.

---

### TemplateEngine

**Ubicación:** `generator/AicaseCli/Core/TemplateEngine.cs`

**Responsabilidad:** Renderiza plantillas Scriban con un modelo de datos para producir el código final.

**Tecnología:** [Scriban](https://github.com/scriban/scriban) - motor de plantillas .NET de alto rendimiento.

**Motivo de elección:** Scriban fue elegido sobre otras alternativas (Handlebars, Liquid, T4) por:
- Sintaxis limpia y expresiva: `{{ for campo in entidad.campos }}`
- Alto rendimiento (compilación de plantillas)
- Soporte para funciones personalizadas de transformación de tipos
- Manejo seguro de nulos
- Soporte para operadores de filtrado y transformación

**Funciones personalizadas registradas:**
| Función           | Descripción                                    |
|-------------------|------------------------------------------------|
| `csharp_type`     | Convierte tipo JSON a tipo C# (`string → string`, `int → int`, etc.) |
| `typescript_type` | Convierte tipo JSON a tipo TypeScript (`decimal → number`) |
| `sql_type`        | Convierte tipo JSON a tipo SQL Server (`string → NVARCHAR(n)`) |
| `to_camel_case`   | Convierte PascalCase a camelCase |
| `to_plural`       | Pluraliza un nombre (básico) |
| `to_kebab_case`   | Convierte PascalCase a kebab-case |

---

### BackendGenerator

**Ubicación:** `generator/AicaseCli/Generators/BackendGenerator.cs`

**Responsabilidad:** Genera todos los archivos C# del backend para cada entidad.

**Archivos generados por entidad:**

| Archivo | Plantilla | Descripción |
|---------|-----------|-------------|
| `Models/{Entidad}.cs` | `model.scriban` | Clase de dominio con DataAnnotations |
| `DTOs/{Entidad}Dtos.cs` | `dto.scriban` | CreateDto, UpdateDto, ResponseDto |
| `Repositories/I{Entidad}Repository.cs` | `repository-interface.scriban` | Interfaz del repositorio |
| `Repositories/{Entidad}Repository.cs` | `repository.scriban` | Implementación EF Core |
| `Services/I{Entidad}Service.cs` | `service-interface.scriban` | Interfaz del servicio |
| `Services/{Entidad}Service.cs` | `service.scriban` | Implementación con stubs de reglas de negocio |
| `Controllers/{Entidad}Controller.cs` | `controller.scriban` | Controlador REST con autorización |
| `Validators/{Entidad}Validator.cs` | `validator.scriban` | FluentValidation con reglas del JSON |
| `Data/Configurations/{Entidad}Configuration.cs` | `dbcontext-config.scriban` | Configuración Fluent API EF Core |

**Fallback de plantillas:** Si las plantillas `.scriban` externas no existen, el generador usa plantillas Scriban embebidas directamente en el código C# para garantizar la funcionalidad sin depender de archivos externos.

---

### FrontendGenerator

**Ubicación:** `generator/AicaseCli/Generators/FrontendGenerator.cs`

**Responsabilidad:** Genera todos los archivos TypeScript y HTML del frontend Angular 17.

**Archivos generados por entidad:**

| Archivo | Plantilla | Descripción |
|---------|-----------|-------------|
| `models/{entidad}.model.ts` | `model.scriban` | Interface TypeScript con tipos mapeados |
| `services/{entidad}.service.ts` | `service.scriban` | Servicio Angular con CRUD via HttpClient |
| `components/{entidad}/list/{entidad}-list.component.ts` | `list-component-ts.scriban` | Componente de listado con paginación |
| `components/{entidad}/list/{entidad}-list.component.html` | `list-component-html.scriban` | Template HTML con tabla Material |
| `components/{entidad}/form/{entidad}-form.component.ts` | `form-component-ts.scriban` | Componente de formulario con ReactiveForm |
| `components/{entidad}/form/{entidad}-form.component.html` | `form-component-html.scriban` | Template HTML con inputs Material |

**Archivos globales generados:**
- `app-routing.module.ts` — Rutas para todas las entidades
- `app.module.ts` — Módulo principal con declaraciones e imports

---

### DatabaseGenerator

**Ubicación:** `generator/AicaseCli/Generators/DatabaseGenerator.cs`

**Responsabilidad:** Genera scripts SQL Server para crear la estructura de base de datos y los stored procedures CRUD.

**Archivos generados:**

| Archivo | Plantilla | Descripción |
|---------|-----------|-------------|
| `01_CreateTables.sql` | `create-table.scriban` | CREATE TABLE con PK, FK, índices únicos |
| `procedures/{Entidad}_CRUD.sql` | `stored-procedure-crud.scriban` | 5 stored procedures por entidad |
| `02_SeedData.sql` | (generado dinámico) | Script de datos iniciales |

**Stored procedures generados por entidad:**
- `sp_{Entidad}_GetAll` — Listado con paginación y búsqueda
- `sp_{Entidad}_GetById` — Obtener por Id
- `sp_{Entidad}_Insert` — Insertar nuevo registro
- `sp_{Entidad}_Update` — Actualizar registro existente
- `sp_{Entidad}_Delete` — Eliminar registro

---

## Decisiones Tecnológicas

### ¿Por qué .NET 8 para el generador CLI?

- Soporte LTS hasta noviembre 2026
- Rendimiento superior en operaciones de I/O y procesamiento de texto
- Integración natural con las plantillas generadas (.NET 8 backend)
- Ecosistema maduro de librerías (NJsonSchema, Scriban, System.CommandLine)

### ¿Por qué Scriban para plantillas?

Comparativa de motores de plantillas:

| Criterio | Scriban | T4 | Handlebars | Liquid |
|----------|---------|-----|------------|--------|
| Rendimiento | ⭐⭐⭐ | ⭐⭐ | ⭐⭐ | ⭐⭐ |
| Sintaxis | ⭐⭐⭐ | ⭐ | ⭐⭐ | ⭐⭐ |
| Funciones custom | ⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐ | ⭐ |
| Seguridad (null) | ⭐⭐⭐ | ⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐ |
| Dependencias | ⭐⭐⭐ | ⭐ | ⭐⭐ | ⭐⭐⭐ |

### ¿Por qué NJsonSchema sobre Newtonsoft.Schema?

- Soporte nativo para JSON Schema draft-07
- Mensajes de error más detallados con ruta exacta del error
- Mantenimiento activo por Rico Suter (mismo autor de NSwag)
- API asíncrona nativa

### ¿Por qué System.CommandLine?

- Librería oficial de Microsoft para CLIs en .NET
- Soporte para opciones tipadas, validación y autocompletado
- Generación automática de `--help` estructurado
- Soporte para subcomandos (escalable para futuras versiones)

### ¿Por qué Angular Material para el frontend?

- Componentes de alto nivel listos (tabla, paginador, datepicker, select)
- Cohesión visual inmediata sin CSS adicional
- Integración nativa con Angular 17 (módulos, formularios reactivos)
- Amplia documentación y comunidad

---

## Extensibilidad

### Agregar soporte para nuevo motor de base de datos

1. Crear plantillas en `templates/database/{nuevo-motor}/`
2. Extender `FieldDefinition.ToSqlType()` con los tipos del nuevo motor
3. Agregar la opción en el esquema JSON (`tecnologias.baseDatos`)
4. Crear un nuevo generador o extender `DatabaseGenerator`

### Agregar soporte para nuevo framework frontend

1. Crear plantillas en `templates/frontend/{nuevo-framework}/`
2. Crear un nuevo generador que herede la lógica común
3. Registrar la opción en el esquema JSON (`tecnologias.frontend`)

### Agregar integración con IA (Paso 5)

La arquitectura está diseñada para agregar un `AiEnhancer` entre el `ProjectParser` y los generadores:

```
ProjectParser → AiEnhancer → [BackendGenerator, FrontendGenerator, DatabaseGenerator]
```

El `AiEnhancer` tomaría las `ReglasNegocio` en pseudocódigo y las convertiría en código real usando un LLM, reemplazando los stubs `// TODO:` generados actualmente.
