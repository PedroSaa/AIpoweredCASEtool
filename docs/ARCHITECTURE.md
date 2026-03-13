# AICASE - Arquitectura del Generador

## Tabla de Contenidos

1. [Visión General](#visión-general)
2. [Diagrama de Componentes](#diagrama-de-componentes)
3. [Flujo de Datos](#flujo-de-datos)
4. [Componentes del Generador](#componentes-del-generador)
5. [Motor de Plantillas](#motor-de-plantillas)
6. [Mapeo de Tipos](#mapeo-de-tipos)
7. [Extensibilidad](#extensibilidad)

---

## Visión General

AICASE implementa el **Enfoque Híbrido (C)** para la generación de código:

```
┌─────────────────────────────────────────────────────────┐
│                     AICASE CLI                          │
│                                                         │
│  JSON Maestro → Validación → Parseo → Generación        │
│                                                         │
│  ┌──────────┐   ┌──────────┐   ┌─────────────────────┐ │
│  │ Schema   │   │ Template │   │ AI Integration      │ │
│  │ Validator│   │ Engine   │   │ (Paso 5 - Futuro)   │ │
│  │(NJsonSch)│   │(Scriban) │   │                     │ │
│  └──────────┘   └──────────┘   └─────────────────────┘ │
└─────────────────────────────────────────────────────────┘
```

**Tecnologías del generador:**
- **Lenguaje**: C# .NET 8 (aplicación CLI)
- **Motor de plantillas**: [Scriban](https://github.com/scriban/scriban) v5.x
- **Validación de schema**: [NJsonSchema](https://github.com/RicoSuter/NJsonSchema)
- **CLI framework**: [System.CommandLine](https://github.com/dotnet/command-line-api)
- **JSON parsing**: Newtonsoft.Json

---

## Diagrama de Componentes

```
┌─────────────────────────────────────────────────────────────────┐
│                        AicaseCli                                │
│                                                                 │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │                     Program.cs (CLI)                    │   │
│  │   --input  --output  --schema                           │   │
│  └─────────────────┬───────────────────────────────────────┘   │
│                    │                                            │
│         ┌──────────▼──────────┐                                │
│         │   Core/             │                                 │
│         │ ┌─────────────────┐ │                                │
│         │ │ SchemaValidator │ │──── NJsonSchema                 │
│         │ └────────┬────────┘ │                                │
│         │ ┌────────▼────────┐ │                                │
│         │ │ ProjectParser   │ │──── Newtonsoft.Json             │
│         │ └────────┬────────┘ │                                │
│         │ ┌────────▼────────┐ │                                │
│         │ │ TemplateEngine  │ │──── Scriban                     │
│         │ └─────────────────┘ │                                │
│         └──────────┬──────────┘                                │
│                    │                                            │
│         ┌──────────▼──────────┐                                │
│         │   Generators/       │                                 │
│         │ ┌─────────────────┐ │                                │
│         │ │BackendGenerator │ │──── templates/backend/*.scriban  │
│         │ ├─────────────────┤ │                                │
│         │ │FrontendGenerator│ │──── templates/frontend/*.scriban │
│         │ ├─────────────────┤ │                                │
│         │ │DatabaseGenerator│ │──── templates/database/*.scriban │
│         │ └─────────────────┘ │                                │
│         └──────────┬──────────┘                                │
│                    │                                            │
│         ┌──────────▼──────────┐                                │
│         │   Models/           │                                 │
│         │  ProjectDefinition  │                                 │
│         │  EntityDefinition   │                                 │
│         │  FieldDefinition    │                                 │
│         │  TypeMapper         │                                 │
│         └─────────────────────┘                                │
└─────────────────────────────────────────────────────────────────┘
```

---

## Flujo de Datos

```
                      AICASE - Flujo de Datos
                      ════════════════════════

 Usuario
    │
    │  aicase --input mi-proyecto.json --output ./salida
    ▼
┌─────────────────────────────────┐
│         Program.cs              │
│  Recibe argumentos CLI          │
│  Orquesta el proceso            │
└────────────────┬────────────────┘
                 │
                 │ 1. Leer archivo JSON
                 ▼
┌─────────────────────────────────┐
│       SchemaValidator           │
│  Valida JSON contra             │
│  aicase-schema.json (Draft-07)  │
│                                 │
│  ✓ JSON válido                  │
│  ✗ Errores con ruta y tipo      │
└────────────────┬────────────────┘
                 │
                 │ 2. Parsear JSON
                 ▼
┌─────────────────────────────────┐
│        ProjectParser            │
│  JObject → ProjectDefinition    │
│                                 │
│  • Metadatos del proyecto       │
│  • Lista de EntityDefinition    │
│    - FieldDefinition[]          │
│    - RelationDefinition[]       │
│    - BusinessRule[]             │
│    - ScreenConfig               │
│    - ApiConfig                  │
└────────────────┬────────────────┘
                 │
                 │ 3. Generar código
                 ▼
┌─────────────────────────────────────────────────────────┐
│                    Generators                           │
│                                                         │
│  BackendGenerator                                       │
│  ┌─────────────────────────────────────────────────┐   │
│  │ Por cada entidad:                               │   │
│  │  • Model.cs         (model.scriban)             │   │
│  │  • {E}Dto.cs        (dto.scriban)               │   │
│  │  • I{E}Repository   (repository-interface.scriban)│  │
│  │  • {E}Repository    (repository.scriban)        │   │
│  │  • I{E}Service      (service-interface.scriban) │   │
│  │  • {E}Service       (service.scriban)           │   │
│  │  • {E}sController   (controller.scriban)        │   │
│  │  • {E}Validator     (validator.scriban)         │   │
│  │  • {E}Configuration (dbcontext-config.scriban)  │   │
│  │ Globales:                                       │   │
│  │  • ApplicationDbContext (dbcontext.scriban)     │   │
│  │  • Program.cs       (program.scriban)           │   │
│  │  • {P}.csproj       (csproj.scriban)            │   │
│  │  • appsettings.json (appsettings.scriban)       │   │
│  └─────────────────────────────────────────────────┘   │
│                                                         │
│  FrontendGenerator                                      │
│  ┌─────────────────────────────────────────────────┐   │
│  │ Por cada entidad:                               │   │
│  │  • {e}.model.ts     (model.scriban)             │   │
│  │  • {e}.service.ts   (service.scriban)           │   │
│  │  • {e}-list.ts      (list-component-ts.scriban) │   │
│  │  • {e}-list.html    (list-component-html.scriban)│  │
│  │  • {e}-form.ts      (form-component-ts.scriban) │   │
│  │  • {e}-form.html    (form-component-html.scriban)│  │
│  │  • {e}-routing.ts   (routing-module.scriban)    │   │
│  │ Global:                                         │   │
│  │  • app-routing.ts   (app-routing.scriban)       │   │
│  └─────────────────────────────────────────────────┘   │
│                                                         │
│  DatabaseGenerator                                      │
│  ┌─────────────────────────────────────────────────┐   │
│  │ Por cada entidad:                               │   │
│  │  • Create_{tabla}.sql  (create-table.scriban)   │   │
│  │  • SP_{tabla}_CRUD.sql (stored-procedure-crud)  │   │
│  │ Global:                                         │   │
│  │  • 00_CreateDatabase.sql (generado en código)   │   │
│  └─────────────────────────────────────────────────┘   │
└────────────────────────────────────────────────────────┬┘
                                                         │
                                                         ▼
                                              output/
                                              ├── backend/
                                              ├── frontend/
                                              └── database/
```

---

## Componentes del Generador

### SchemaValidator

Utiliza **NJsonSchema** para validar el JSON de entrada contra el schema formal `aicase-schema.json`.

```csharp
var schema = await JsonSchema.FromJsonAsync(schemaContent);
var errors = schema.Validate(jsonContent);
```

Reporta errores con la ruta JSON donde ocurrió el problema (ej: `entidades[0].campos[1].tipo`).

### ProjectParser

Transforma el JSON (via `Newtonsoft.Json.Linq.JObject`) en el modelo de dominio interno del generador:

```
JSON                    → Modelos Internos
───────────────────────────────────────────
"proyecto"              → ProjectDefinition.Name
"entidades[].campos[]"  → EntityDefinition.Fields[FieldDefinition]
"validaciones[]"        → FieldDefinition.Validations[ValidationRule]
"relaciones[]"          → EntityDefinition.Relations[RelationDefinition]
"reglasNegocio[]"       → EntityDefinition.BusinessRules[BusinessRule]
```

### TemplateEngine

Wrapper sobre **Scriban** que:
1. Carga archivos `.scriban` desde la carpeta `templates/`
2. Expone funciones helper: `to_camel_case`, `to_pascal_case`, `to_kebab_case`, `pluralize`
3. Mapea las propiedades del modelo al contexto de la plantilla (nombres en minúscula)

### Generadores

Cada generador recibe un `ProjectDefinition` y:
1. Crea la estructura de carpetas en `output/`
2. Por cada entidad, renderiza las plantillas correspondientes
3. Escribe los archivos generados
4. Retorna el conteo de archivos generados

---

## Motor de Plantillas

Las plantillas usan la sintaxis **Scriban**:

```scriban
{{- for field in entity.fields }}
public {{ field.csharptype }} {{ field.name }} { get; set; }
{{- end }}
```

### Variables disponibles en las plantillas

| Variable | Tipo | Descripción |
|----------|------|-------------|
| `project` | `ProjectDefinition` | Metadatos del proyecto |
| `entity` | `EntityDefinition` | Entidad actual |
| `project.entities` | `List<EntityDefinition>` | Todas las entidades |

### Funciones helper

| Función | Ejemplo | Resultado |
|---------|---------|-----------|
| `to_camel_case` | `"NombreCliente"` | `"nombreCliente"` |
| `to_pascal_case` | `"nombreCliente"` | `"NombreCliente"` |
| `to_kebab_case` | `"TipoCliente"` | `"tipo-cliente"` |
| `to_snake_case` | `"TipoCliente"` | `"tipo_cliente"` |
| `pluralize` | `"Cliente"` | `"Clientes"` |

---

## Mapeo de Tipos

El `TypeMapper` centraliza el mapeo entre el tipo JSON y las capas de la aplicación:

| JSON | C# (requerido) | C# (opcional) | TypeScript | SQL Server |
|------|----------------|---------------|------------|-----------|
| `string` | `string` | `string?` | `string` | `NVARCHAR(n)` |
| `int` | `int` | `int?` | `number` | `INT` |
| `decimal` | `decimal` | `decimal?` | `number` | `DECIMAL(p,s)` |
| `double` | `double` | `double?` | `number` | `FLOAT` |
| `float` | `float` | `float?` | `number` | `REAL` |
| `long` | `long` | `long?` | `number` | `BIGINT` |
| `date` | `DateOnly` | `DateOnly?` | `Date` | `DATE` |
| `datetime` | `DateTime` | `DateTime?` | `Date` | `DATETIME2` |
| `bool` | `bool` | `bool?` | `boolean` | `BIT` |
| `guid` | `Guid` | `Guid?` | `string` | `UNIQUEIDENTIFIER` |
| `byte[]` | `byte[]` | `byte[]` | `string` | `VARBINARY(MAX)` |

---

## Extensibilidad

Para agregar soporte a una nueva tecnología (ej: Python/FastAPI):

1. Crear carpeta `templates/python/` con las plantillas `.scriban`
2. Agregar clase `PythonGenerator` en `generator/AicaseCli/Generators/`
3. Registrar el generador en `Program.cs`
4. Actualizar el schema para incluir `"python"` en los valores permitidos de `tecnologias.backend`

Para agregar integración con IA (Paso 5):

1. Detectar reglas de negocio con `accion: "personalizado"` o sin `codigoPersonalizado`
2. Construir prompt con el contexto de la entidad y la regla
3. Llamar a la API de OpenAI/Claude/etc.
4. Insertar el código generado en el archivo de servicio correspondiente
