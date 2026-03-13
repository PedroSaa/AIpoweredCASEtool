# AICASE Schema - Documentación Completa

## Tabla de Contenidos

1. [Introducción](#introducción)
2. [Estructura Raíz](#estructura-raíz)
3. [Tecnologías](#tecnologías)
4. [Configuración Global](#configuración-global)
5. [Entidades](#entidades)
6. [Campos](#campos)
7. [Validaciones](#validaciones)
8. [Input UI](#input-ui)
9. [Relaciones](#relaciones)
10. [Reglas de Negocio](#reglas-de-negocio)
11. [Pantallas](#pantallas)
12. [Configuración de API](#configuración-de-api)
13. [Ejemplos](#ejemplos)

---

## Introducción

El schema AICASE define el "contrato único de verdad" para la generación automática de código. A partir de un archivo JSON que cumple con este schema, el motor AICASE es capaz de generar:

- Modelos y DTOs en C#
- Repositorios y servicios con lógica de negocio
- Controladores REST con autenticación
- Componentes Angular (listado y formulario)
- Scripts DDL para SQL Server
- Stored Procedures (opcional)

El schema sigue el estándar **JSON Schema Draft-07**.

---

## Estructura Raíz

| Propiedad | Tipo | Requerido | Descripción |
|-----------|------|-----------|-------------|
| `proyecto` | string | ✅ | Nombre del proyecto (usado para namespaces y títulos) |
| `version` | string | ✅ | Versión en formato semver (ej: `1.0.0`) |
| `descripcion` | string | ❌ | Descripción breve del proyecto |
| `tecnologias` | object | ✅ | Stack tecnológico objetivo |
| `configuracionGlobal` | object | ❌ | Configuración global de autenticación, roles y BD |
| `entidades` | array | ✅ | Lista de entidades del dominio (mínimo 1) |

### Ejemplo mínimo

```json
{
  "proyecto": "MiProyecto",
  "version": "1.0",
  "tecnologias": {
    "backend": "C# .NET 8 Web API",
    "frontend": "Angular 17",
    "database": "SQL Server"
  },
  "entidades": [ ... ]
}
```

---

## Tecnologías

Define el stack tecnológico del proyecto generado.

| Propiedad | Tipo | Requerido | Descripción |
|-----------|------|-----------|-------------|
| `backend` | string | ✅ | Tecnología backend (ej: `"C# .NET 8 Web API"`) |
| `frontend` | string | ✅ | Tecnología frontend (ej: `"Angular 17"`) |
| `database` | string | ✅ | Motor de base de datos (ej: `"SQL Server"`) |

---

## Configuración Global

Configuración transversal del sistema.

### Autenticación

| Propiedad | Tipo | Valores | Descripción |
|-----------|------|---------|-------------|
| `tipo` | string | `JWT`, `OAuth2`, `ApiKey`, `None` | Mecanismo de autenticación |
| `expiracionMinutos` | integer | ≥1 | Tiempo de vida del token en minutos |
| `refreshToken` | boolean | - | Si se habilita refresh token |

### Roles

Array de strings con los roles del sistema. Estos roles se usan en la propiedad `api.roles` de cada entidad.

```json
"roles": ["Admin", "Ventas", "Consulta"]
```

### Base de Datos

| Propiedad | Tipo | Descripción |
|-----------|------|-------------|
| `nombreBaseDatos` | string | Nombre de la base de datos SQL |
| `esquema` | string | Esquema SQL (default: `dbo`) |
| `usarStoredProcedures` | boolean | Si se generan stored procedures CRUD |

---

## Entidades

Cada entidad representa una tabla en la base de datos y el conjunto de artefactos asociados.

| Propiedad | Tipo | Requerido | Descripción |
|-----------|------|-----------|-------------|
| `nombre` | string | ✅ | Nombre en PascalCase (ej: `"Cliente"`) |
| `tabla` | string | ✅ | Nombre de la tabla SQL (ej: `"Clientes"`) |
| `descripcion` | string | ❌ | Descripción de la entidad |
| `campos` | array | ✅ | Lista de campos/columnas (mínimo 1) |
| `relaciones` | array | ❌ | Relaciones con otras entidades |
| `reglasNegocio` | array | ❌ | Reglas de negocio |
| `pantallas` | object | ❌ | Configuración de UI |
| `api` | object | ❌ | Configuración del endpoint REST |

> **Convención de nombres**: El `nombre` de la entidad se usa como base para todos los artefactos generados:
> - Modelo C#: `Cliente.cs`
> - Controlador: `ClientesController.cs`
> - Servicio Angular: `cliente.service.ts`
> - Componente listado: `cliente-list.component.ts`

---

## Campos

Cada campo define una columna en la tabla y su comportamiento en las distintas capas.

| Propiedad | Tipo | Requerido | Descripción |
|-----------|------|-----------|-------------|
| `nombre` | string | ✅ | Nombre en PascalCase (ej: `"NombreCliente"`) |
| `tipo` | string | ✅ | Tipo de dato (ver tabla de tipos) |
| `largo` | integer | ❌ | Longitud máxima (solo para `string`) |
| `precision` | integer | ❌ | Precisión total (solo para `decimal`) |
| `escala` | integer | ❌ | Dígitos decimales (solo para `decimal`) |
| `requerido` | boolean | ❌ | Si el campo es obligatorio (default: `false`) |
| `unico` | boolean | ❌ | Si tiene restricción UNIQUE (default: `false`) |
| `esPK` | boolean | ❌ | Si es clave primaria (default: `false`) |
| `autoIncremento` | boolean | ❌ | Si es IDENTITY en SQL (default: `false`) |
| `valorDefecto` | any | ❌ | Valor por defecto del campo |
| `esFK` | boolean | ❌ | Si es clave foránea (default: `false`) |
| `validaciones` | array | ❌ | Lista de validaciones |
| `input` | object | ❌ | Configuración del input en la UI |

### Tipos de Datos y Mapeos

| Tipo JSON | C# | TypeScript | SQL Server |
|-----------|-----|------------|-----------|
| `string` | `string` | `string` | `NVARCHAR(largo)` |
| `int` | `int` | `number` | `INT` |
| `decimal` | `decimal` | `number` | `DECIMAL(p,e)` |
| `double` | `double` | `number` | `FLOAT` |
| `float` | `float` | `number` | `REAL` |
| `long` | `long` | `number` | `BIGINT` |
| `date` | `DateOnly` | `Date` | `DATE` |
| `datetime` | `DateTime` | `Date` | `DATETIME2` |
| `bool` | `bool` | `boolean` | `BIT` |
| `guid` | `Guid` | `string` | `UNIQUEIDENTIFIER` |
| `byte[]` | `byte[]` | `string` (base64) | `VARBINARY(MAX)` |

---

## Validaciones

Las validaciones se aplican en múltiples capas: FluentValidation (backend), Reactive Forms (Angular frontend) y constraints SQL.

| Tipo | Aplica a | Propiedad `valor` | Descripción |
|------|----------|-------------------|-------------|
| `minLength` | string | integer | Longitud mínima |
| `maxLength` | string | integer | Longitud máxima |
| `min` | number, date | number/string | Valor mínimo |
| `max` | number, date | number/string | Valor máximo |
| `email` | string | - | Formato email válido |
| `regex` | string | - | Usa `expresionRegex` |
| `fechaMaxima` | date, datetime | string (`"hoy"` o fecha ISO) | Fecha máxima permitida |
| `fechaMinima` | date, datetime | string (`"hoy"` o fecha ISO) | Fecha mínima permitida |
| `personalizado` | cualquiera | - | Delegado a la IA para implementación |

### Propiedades de Validación

| Propiedad | Tipo | Descripción |
|-----------|------|-------------|
| `tipo` | string | Tipo de validación (requerido) |
| `valor` | any | Valor de referencia |
| `mensaje` | string | Mensaje de error personalizado |
| `expresionRegex` | string | Expresión regular (solo para tipo `regex`) |

### Ejemplo

```json
"validaciones": [
  { "tipo": "minLength", "valor": 3, "mensaje": "Mínimo 3 caracteres" },
  { "tipo": "email", "mensaje": "Email inválido" },
  { "tipo": "regex", "expresionRegex": "^[A-Z]+$", "mensaje": "Solo letras mayúsculas" }
]
```

---

## Input UI

Configura cómo se renderiza el campo en los formularios Angular.

| Propiedad | Tipo | Requerido | Descripción |
|-----------|------|-----------|-------------|
| `tipo` | string | ✅ | Tipo de control HTML |
| `label` | string | ✅ | Etiqueta visible |
| `placeholder` | string | ❌ | Texto placeholder |
| `fuenteDatos` | string | ❌ | Entidad o URL para dropdown/autocomplete |
| `opciones` | array | ❌ | Opciones estáticas para radio/dropdown |
| `orden` | integer | ❌ | Posición en el formulario |
| `ocultarEnListado` | boolean | ❌ | Si se oculta en la tabla (default: `false`) |

### Tipos de Input

| Tipo | Control HTML | Descripción |
|------|-------------|-------------|
| `text` | `<input type="text">` | Texto libre |
| `email` | `<input type="email">` | Email con validación |
| `number` | `<input type="number">` | Numérico |
| `datepicker` | Angular DatePicker | Selector de fecha |
| `dropdown` | `<mat-select>` | Lista desplegable |
| `checkbox` | `<mat-checkbox>` | Casilla de verificación |
| `textarea` | `<textarea>` | Texto largo |
| `radio` | `<mat-radio-group>` | Botones de radio |
| `autocomplete` | `<mat-autocomplete>` | Búsqueda con sugerencias |
| `password` | `<input type="password">` | Contraseña |
| `hidden` | No visible | Campo oculto (PK, etc.) |

---

## Relaciones

Define las relaciones entre entidades, que se traducen a navigation properties en C# y foreign keys en SQL.

| Propiedad | Tipo | Requerido | Descripción |
|-----------|------|-----------|-------------|
| `nombre` | string | ❌ | Nombre descriptivo |
| `tipo` | string | ✅ | Tipo de relación |
| `entidadDestino` | string | ✅ | Nombre de la entidad relacionada |
| `campoLocal` | string | ✅ | Campo FK en esta entidad |
| `campoDestino` | string | ✅ | Campo PK en la entidad destino |
| `cargaEager` | boolean | ❌ | Si se usa Include() en EF Core |
| `tablaPivote` | string | ❌ | Tabla intermedia para ManyToMany |

### Tipos de Relación

| Tipo | C# | EF Core | SQL |
|------|-----|---------|-----|
| `ManyToOne` | Navigation property `[Entidad]` | `HasOne` + `WithMany` | FOREIGN KEY |
| `OneToMany` | `ICollection<[Entidad]>` | `HasMany` + `WithOne` | FK en tabla hija |
| `OneToOne` | Navigation property | `HasOne` + `WithOne` | FK + UNIQUE |
| `ManyToMany` | `ICollection<[Entidad]>` | `HasMany` + `WithMany` | Tabla pivote |

---

## Reglas de Negocio

Las reglas de negocio se implementan en la capa de servicio C# y se documentan para que la IA genere la lógica específica.

| Propiedad | Tipo | Requerido | Descripción |
|-----------|------|-----------|-------------|
| `nombre` | string | ✅ | Identificador de la regla (PascalCase) |
| `descripcion` | string | ❌ | Descripción en lenguaje natural |
| `trigger` | string | ✅ | Evento que dispara la regla |
| `condicion` | string | ❌ | Condición (para que la IA genere el código) |
| `accion` | string | ✅ | Qué hacer cuando se cumple la condición |
| `mensaje` | string | ❌ | Mensaje al usuario |
| `codigoPersonalizado` | string | ❌ | Código C# personalizado (override de IA) |

### Triggers

| Trigger | Momento de ejecución |
|---------|---------------------|
| `antesDeInsertar` | Antes de INSERT (validación) |
| `despuesDeInsertar` | Después de INSERT (efectos) |
| `antesDeActualizar` | Antes de UPDATE (validación) |
| `despuesDeActualizar` | Después de UPDATE (efectos) |
| `antesDeEliminar` | Antes de DELETE (validación) |
| `despuesDeEliminar` | Después de DELETE (limpieza) |
| `alConsultar` | Al realizar consulta (filtrado) |

### Acciones

| Acción | Descripción |
|--------|-------------|
| `rechazar` | Lanza excepción y aborta la operación |
| `modificar` | Modifica el dato antes de guardar |
| `notificar` | Registra notificación sin abortar |
| `registrarLog` | Escribe en log de auditoría |
| `personalizado` | Usa `codigoPersonalizado` |

---

## Pantallas

Configuración de la capa de presentación Angular.

### Pantalla de Listado

| Propiedad | Tipo | Requerido | Descripción |
|-----------|------|-----------|-------------|
| `titulo` | string | ✅ | Título de la página |
| `columnas` | string[] | ✅ | Campos a mostrar en la tabla |
| `acciones` | string[] | ❌ | Botones de acción (`crear`, `editar`, `eliminar`, `ver`, `exportar`, `importar`) |
| `filtros` | string[] | ❌ | Campos con filtro habilitado |
| `paginacion` | object | ❌ | Configuración de paginación |
| `ordenamientoPorDefecto` | object | ❌ | Campo y dirección de ordenamiento inicial |

### Pantalla de Formulario

| Propiedad | Tipo | Requerido | Descripción |
|-----------|------|-----------|-------------|
| `titulo` | string | ✅ | Título del formulario |
| `layout` | string | ❌ | `1-columna`, `2-columnas`, `3-columnas`, `personalizado` |
| `campos` | string[] | ❌ | Campos en orden (si no se usan secciones) |
| `secciones` | array | ❌ | Agrupación de campos en secciones |

---

## Configuración de API

Define el endpoint REST que se generará en el controlador C#.

| Propiedad | Tipo | Requerido | Descripción |
|-----------|------|-----------|-------------|
| `ruta` | string | ✅ | Ruta base (ej: `/api/clientes`) |
| `operaciones` | string[] | ✅ | Verbos HTTP habilitados |
| `autenticacion` | boolean | ❌ | Si requiere `[Authorize]` |
| `roles` | string[] | ❌ | Roles con acceso (`[Authorize(Roles="...")]`) |
| `versionamiento` | string | ❌ | Versión de la API (ej: `v1`) |

### Operaciones HTTP

| Operación | Verbo HTTP | Endpoint generado |
|-----------|------------|-------------------|
| `GET` | GET | `GET /api/{recurso}` (listado paginado) |
| `GET_BY_ID` | GET | `GET /api/{recurso}/{id}` |
| `POST` | POST | `POST /api/{recurso}` |
| `PUT` | PUT | `PUT /api/{recurso}/{id}` |
| `PATCH` | PATCH | `PATCH /api/{recurso}/{id}` |
| `DELETE` | DELETE | `DELETE /api/{recurso}/{id}` |

---

## Ejemplos

### Campo string con validaciones

```json
{
  "nombre": "Email",
  "tipo": "string",
  "largo": 150,
  "requerido": true,
  "unico": true,
  "validaciones": [
    { "tipo": "email", "mensaje": "Email inválido" },
    { "tipo": "maxLength", "valor": 150 }
  ],
  "input": {
    "tipo": "email",
    "label": "Correo Electrónico",
    "placeholder": "usuario@dominio.com"
  }
}
```

### Campo con relación ManyToOne

```json
{
  "nombre": "TipoClienteId",
  "tipo": "int",
  "requerido": true,
  "esFK": true,
  "input": {
    "tipo": "dropdown",
    "label": "Tipo de Cliente",
    "fuenteDatos": "TipoCliente"
  }
}
```

La relación se declara en el array `relaciones` de la entidad:

```json
"relaciones": [
  {
    "tipo": "ManyToOne",
    "entidadDestino": "TipoCliente",
    "campoLocal": "TipoClienteId",
    "campoDestino": "Id",
    "cargaEager": true
  }
]
```

### Regla de negocio de unicidad

```json
{
  "nombre": "EmailUnico",
  "trigger": "antesDeInsertar",
  "condicion": "El email ya existe en la tabla",
  "accion": "rechazar",
  "mensaje": "Ya existe un registro con ese email"
}
```

### Configuración de API con roles

```json
"api": {
  "ruta": "/api/clientes",
  "operaciones": ["GET", "GET_BY_ID", "POST", "PUT", "DELETE"],
  "autenticacion": true,
  "roles": ["Admin", "Ventas"]
}
```
