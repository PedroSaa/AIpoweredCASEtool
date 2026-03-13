# AICASE — Herramienta CASE Potenciada por IA

**AICASE** (Artificial Intelligence Computer-Aided Software Engineering) es una herramienta de generación de código full-stack que combina un enfoque de **plantillas Scriban determinísticas** con la futura integración de **modelos de lenguaje de gran escala (LLMs)** para la generación de lógica de negocio compleja.

## ¿Qué es AICASE?

AICASE permite definir un proyecto de software completo en un único archivo JSON estructurado y, a partir de él, generar automáticamente:

- **Backend**: API REST en .NET 8 con Entity Framework Core, FluentValidation y AutoMapper
- **Frontend**: Aplicación Angular 17 con Angular Material
- **Base de datos**: Scripts SQL Server con tablas, índices y stored procedures CRUD

El enfoque **híbrido** combina:
1. **Generación determinística** (Paso 1–4): Código CRUD estándar mediante plantillas Scriban
2. **Generación con IA** (Paso 5, próximamente): Lógica de negocio compleja mediante LLMs (GPT-4, Claude, etc.)

---

## Estructura del Proyecto

```
AIpoweredCASEtool/
├── schema/
│   ├── aicase-schema.json          # Esquema JSON (draft-07) para validar proyectos
│   └── ejemplo-proyecto.json       # Ejemplo completo: SistemaVentas
│
├── docs/
│   ├── SCHEMA.md                   # Documentación del esquema (en español)
│   └── ARCHITECTURE.md             # Arquitectura del generador (en español)
│
├── generator/
│   └── AicaseCli/
│       ├── AicaseCli.csproj        # Proyecto .NET 8 Console App
│       ├── Program.cs              # CLI con System.CommandLine
│       ├── Core/
│       │   ├── SchemaValidator.cs  # Validación con NJsonSchema
│       │   ├── ProjectParser.cs    # Deserialización con Newtonsoft.Json
│       │   └── TemplateEngine.cs   # Motor Scriban
│       ├── Models/
│       │   ├── ProjectDefinition.cs
│       │   ├── EntityDefinition.cs
│       │   ├── FieldDefinition.cs
│       │   └── ...                 # Resto de modelos
│       └── Generators/
│           ├── BackendGenerator.cs
│           ├── FrontendGenerator.cs
│           └── DatabaseGenerator.cs
│
├── templates/
│   ├── backend/
│   │   ├── model.scriban
│   │   ├── dto.scriban
│   │   ├── repository-interface.scriban
│   │   ├── repository.scriban
│   │   ├── service-interface.scriban
│   │   ├── service.scriban
│   │   ├── controller.scriban
│   │   ├── validator.scriban
│   │   └── dbcontext-config.scriban
│   ├── frontend/
│   │   ├── model.scriban
│   │   ├── service.scriban
│   │   ├── list-component-ts.scriban
│   │   ├── list-component-html.scriban
│   │   ├── form-component-ts.scriban
│   │   ├── form-component-html.scriban
│   │   └── routing-module.scriban
│   └── database/
│       ├── create-table.scriban
│       └── stored-procedure-crud.scriban
│
├── scaffolding/
│   ├── backend/                    # Base del proyecto .NET 8
│   │   ├── BackendBase.csproj
│   │   ├── Program.cs
│   │   ├── appsettings.json
│   │   └── Data/ApplicationDbContext.cs
│   └── frontend/                   # Base del proyecto Angular 17
│       ├── package.json
│       ├── angular.json
│       └── src/
│           ├── app/
│           │   ├── services/api.service.ts
│           │   └── interceptors/auth.interceptor.ts
│           └── environments/
│               ├── environment.ts
│               └── environment.prod.ts
│
└── output/                         # Código generado (ignorado en Git)
```

---

## Requisitos Previos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- [Node.js 18+](https://nodejs.org/) (para el frontend generado)
- [Angular CLI 17](https://angular.io/cli) (opcional, para ejecutar el frontend)

---

## Compilar y Usar el CLI

### 1. Compilar el generador

```bash
cd generator/AicaseCli
dotnet restore
dotnet build -c Release
```

### 2. Opciones del CLI

```
AICASE CLI - Generador de código para proyectos full-stack

Uso: aicase [opciones]

Opciones:
  -i, --input <ruta>       Archivo JSON de definición del proyecto [REQUERIDO]
  -o, --output <dir>       Directorio de salida (por defecto: output/)
  -s, --schema <ruta>      Archivo de esquema JSON para validación
  -t, --templates <dir>    Directorio de plantillas Scriban
  --skip-validation        Omitir validación del esquema JSON
  -v, --verbose            Mostrar árbol de archivos generados
  --version                Mostrar versión
  -?, -h, --help           Mostrar ayuda
```

### 3. Ejemplo de uso con SistemaVentas

```bash
# Desde la raíz del repositorio
dotnet run --project generator/AicaseCli -- \
  --input schema/ejemplo-proyecto.json \
  --output output/SistemaVentas \
  --verbose
```

**Salida esperada:**
```
╔════════════════════════════════════════╗
║    AICASE - Generador de Código v1.0   ║
╚════════════════════════════════════════╝

[1/6] Leyendo archivo de entrada: schema/ejemplo-proyecto.json
[2/6] Validando contra el esquema AICASE...
  ✓ JSON válido según el esquema AICASE.
[3/6] Parseando definición del proyecto...
  ✓ Proyecto: SistemaVentas v1.0.0
  ✓ Entidades encontradas: 3
[4/6] Generando código Backend (.NET 8)...
  ✓ Backend generado en: output/SistemaVentas/backend
[5/6] Generando código Frontend (Angular 17)...
  ✓ Frontend generado en: output/SistemaVentas/frontend
[6/6] Generando scripts de Base de Datos (SQL Server)...
  ✓ Scripts SQL generados en: output/SistemaVentas/database

╔════════════════════════════════════════╗
║         ¡Generación completada!        ║
╚════════════════════════════════════════╝
Archivos generados en: /ruta/output/SistemaVentas
```

### 4. Usar el código generado

**Backend (.NET 8):**
```bash
cd output/SistemaVentas/backend
# 1. Copiar los archivos generados al proyecto scaffolding
# 2. Editar appsettings.json con la cadena de conexión real
dotnet ef migrations add InitialCreate
dotnet ef database update
dotnet run
# API disponible en http://localhost:5000
# Swagger UI en http://localhost:5000/swagger
```

**Frontend (Angular 17):**
```bash
cd output/SistemaVentas/frontend
npm install
ng serve
# Abrir http://localhost:4200
```

**Base de datos (SQL Server):**
```sql
-- En SQL Server Management Studio:
-- 1. Crear la base de datos SistemaVentasDB
-- 2. Ejecutar output/SistemaVentas/database/01_CreateTables.sql
-- 3. Ejecutar cada archivo en output/SistemaVentas/database/procedures/
-- 4. Ejecutar output/SistemaVentas/database/02_SeedData.sql
```

---

## Crear tu Propio Proyecto

### Paso 1: Definir el proyecto en JSON

Crea un archivo `mi-proyecto.json` basándote en `schema/ejemplo-proyecto.json`.
Consulta la documentación completa del esquema en `docs/SCHEMA.md`.

**Estructura mínima:**
```json
{
  "nombre": "MiProyecto",
  "version": "1.0.0",
  "tecnologias": {
    "backend": "dotnet8",
    "frontend": "angular17",
    "baseDatos": "sqlserver"
  },
  "entidades": [
    {
      "nombre": "MiEntidad",
      "tabla": "MisEntidades",
      "campos": [
        { "nombre": "Id", "tipo": "int", "esPK": true, "autoIncremento": true },
        { "nombre": "Nombre", "tipo": "string", "largo": 100, "requerido": true }
      ]
    }
  ]
}
```

### Paso 2: Generar el código

```bash
dotnet run --project generator/AicaseCli -- \
  --input mi-proyecto.json \
  --output output/MiProyecto
```

### Paso 3: Personalizar las plantillas (opcional)

Si necesita ajustar el código generado, edite los archivos `.scriban` en la carpeta `templates/`.
Los generadores primero buscan las plantillas en el directorio `--templates` y si no las encuentran, usan las plantillas embebidas.

---

## Tecnologías Utilizadas

| Componente      | Tecnología                            | Versión  |
|-----------------|---------------------------------------|----------|
| CLI Generator   | .NET 8 Console App                    | 8.0      |
| CLI Framework   | System.CommandLine                    | 2.0 beta |
| Templates       | Scriban                               | 5.9      |
| JSON Validation | NJsonSchema                           | 11.0     |
| JSON Parsing    | Newtonsoft.Json                       | 13.0     |
| Backend         | ASP.NET Core 8 Web API                | 8.0      |
| ORM             | Entity Framework Core (SQL Server)    | 8.0      |
| Validation      | FluentValidation                      | 11.3     |
| Mapping         | AutoMapper                            | 12.0     |
| API Docs        | Swashbuckle (Swagger)                 | 6.5      |
| Auth            | JWT Bearer                            | 8.0      |
| Frontend        | Angular                               | 17.0     |
| UI Components   | Angular Material                      | 17.0     |
| Base de datos   | SQL Server                            | 2019+    |

---

## Roadmap

### ✅ Paso 1: Diseño del esquema JSON
Esquema JSON draft-07 con soporte completo de entidades, campos, validaciones, relaciones, reglas de negocio, configuración de pantallas y API.

### ✅ Paso 2: Motor de plantillas Scriban
Plantillas para backend (.NET 8), frontend (Angular 17) y base de datos (SQL Server).

### ✅ Paso 3: Generador CLI funcional
CLI con System.CommandLine, validación de esquema con NJsonSchema, parseo y generación completa.

### ✅ Paso 4: Scaffolding base
Proyecto base .NET 8 (DbContext, Program.cs, JWT, Swagger) y Angular 17 (ApiService, AuthInterceptor).

### 🔄 Paso 5: Integración de IA para lógica compleja *(próximamente)*
- Integración con OpenAI GPT-4 / Azure OpenAI / Anthropic Claude para:
  - Implementar automáticamente reglas de negocio definidas en pseudocódigo
  - Generar casos de prueba unitarios e de integración
  - Sugerir mejoras en el diseño del esquema
  - Documentar automáticamente el código generado
  - Detectar inconsistencias y errores en el modelo de datos

### 📋 Paso 6: Refinamiento y pruebas
- Suite de pruebas para el generador
- Validación automática del código generado (compilación en CI/CD)
- Soporte para PostgreSQL y MySQL
- Soporte para React 18 y Vue 3
- Interfaz web visual para editar el JSON del proyecto

---

## Contribuir

1. Haz fork del repositorio
2. Crea una rama: `git checkout -b feature/mi-caracteristica`
3. Realiza tus cambios con pruebas
4. Envía un Pull Request con descripción detallada

---

## Licencia

MIT License — Ver archivo LICENSE para detalles.
