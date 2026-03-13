# AICASE – Documentación del Esquema JSON

Este documento describe en detalle cada propiedad del esquema JSON de AICASE (`aicase-schema.json`), incluyendo los tipos permitidos, restricciones y ejemplos de uso.

---

## Índice

1. [Metadatos del Proyecto](#1-metadatos-del-proyecto)
2. [Configuración de Tecnologías](#2-configuración-de-tecnologías)
3. [Entidades](#3-entidades)
4. [Campos](#4-campos)
5. [Validaciones](#5-validaciones)
6. [Inputs de UI](#6-inputs-de-ui)
7. [Relaciones](#7-relaciones)
8. [Reglas de Negocio](#8-reglas-de-negocio)
9. [Configuración de Pantallas](#9-configuración-de-pantallas)
10. [Configuración de API](#10-configuración-de-api)
11. [Configuración Global](#11-configuración-global)

---

## 1. Metadatos del Proyecto

Propiedades raíz del archivo JSON de definición del proyecto.

| Propiedad      | Tipo     | Requerido | Descripción                                              |
|----------------|----------|-----------|----------------------------------------------------------|
| `nombre`       | string   | ✅ Sí     | Nombre del proyecto. Min: 1, Max: 100 caracteres.        |
| `version`      | string   | ✅ Sí     | Versión en formato semántico (ej: `"1.0.0"`).            |
| `descripcion`  | string   | No        | Descripción textual del proyecto. Max: 500 caracteres.   |
| `tecnologias`  | objeto   | ✅ Sí     | Tecnologías seleccionadas (ver sección 2).               |
| `entidades`    | arreglo  | ✅ Sí     | Lista de entidades del dominio (mínimo 1).               |
| `configuracion`| objeto   | No        | Configuración global del sistema (ver sección 11).       |

### Ejemplo

```json
{
  "nombre": "SistemaVentas",
  "version": "1.0.0",
  "descripcion": "Sistema de gestión de ventas",
  "tecnologias": { ... },
  "entidades": [ ... ],
  "configuracion": { ... }
}
```

---

## 2. Configuración de Tecnologías

Objeto `tecnologias` que especifica el stack tecnológico del proyecto generado.

| Propiedad    | Tipo   | Valores permitidos                              | Descripción                   |
|--------------|--------|-------------------------------------------------|-------------------------------|
| `backend`    | string | `dotnet8`, `dotnet6`, `java-spring`, `node-express` | Tecnología del backend    |
| `frontend`   | string | `angular17`, `angular16`, `react18`, `vue3`     | Framework del frontend        |
| `baseDatos`  | string | `sqlserver`, `postgresql`, `mysql`, `sqlite`    | Motor de base de datos        |

### Ejemplo

```json
"tecnologias": {
  "backend": "dotnet8",
  "frontend": "angular17",
  "baseDatos": "sqlserver"
}
```

---

## 3. Entidades

Cada elemento del arreglo `entidades` define una entidad del dominio (tabla + modelo + API + pantallas).

| Propiedad       | Tipo    | Requerido | Descripción                                                    |
|-----------------|---------|-----------|----------------------------------------------------------------|
| `nombre`        | string  | ✅ Sí     | Nombre en PascalCase. Patrón: `^[A-Z][a-zA-Z0-9]*$`           |
| `tabla`         | string  | ✅ Sí     | Nombre de la tabla en la base de datos.                        |
| `descripcion`   | string  | No        | Descripción de la entidad.                                     |
| `campos`        | arreglo | ✅ Sí     | Campos/columnas de la entidad (mínimo 1). Ver sección 4.      |
| `relaciones`    | arreglo | No        | Relaciones con otras entidades. Ver sección 7.                |
| `reglasNegocio` | arreglo | No        | Reglas de negocio. Ver sección 8.                             |
| `pantallas`     | objeto  | No        | Configuración de pantallas UI. Ver sección 9.                 |
| `api`           | objeto  | No        | Configuración de la API REST. Ver sección 10.                 |

### Ejemplo

```json
{
  "nombre": "Cliente",
  "tabla": "Clientes",
  "descripcion": "Entidad de clientes del sistema",
  "campos": [ ... ],
  "relaciones": [ ... ],
  "reglasNegocio": [ ... ],
  "pantallas": { ... },
  "api": { ... }
}
```

---

## 4. Campos

Cada elemento del arreglo `campos` dentro de una entidad define una columna/propiedad.

| Propiedad         | Tipo     | Requerido | Descripción                                                      |
|-------------------|----------|-----------|------------------------------------------------------------------|
| `nombre`          | string   | ✅ Sí     | Nombre en PascalCase. Patrón: `^[A-Z][a-zA-Z0-9]*$`             |
| `tipo`            | string   | ✅ Sí     | Tipo de dato (ver tabla de tipos más abajo).                     |
| `largo`           | integer  | No        | Longitud máxima para campos `string` (1-8000).                  |
| `precision`       | integer  | No        | Precisión total de dígitos para `decimal` (1-38).               |
| `escala`          | integer  | No        | Dígitos decimales para `decimal` (0-38).                        |
| `requerido`       | boolean  | No        | Si el campo es obligatorio. Por defecto: `false`.               |
| `unico`           | boolean  | No        | Si el valor debe ser único en la tabla. Por defecto: `false`.   |
| `esPK`            | boolean  | No        | Si es clave primaria. Por defecto: `false`.                     |
| `autoIncremento`  | boolean  | No        | Si se incrementa automáticamente. Por defecto: `false`.         |
| `valorPorDefecto` | any      | No        | Valor por defecto del campo.                                    |
| `validaciones`    | arreglo  | No        | Reglas de validación. Ver sección 5.                            |
| `input`           | objeto   | No        | Configuración de UI. Ver sección 6.                             |
| `relacion`        | objeto   | No        | Relación de este campo con otra entidad. Ver sección 7.         |

### Tipos de Datos Permitidos

| Valor en JSON | Tipo C#         | Tipo SQL Server      | Tipo TypeScript |
|---------------|-----------------|----------------------|-----------------|
| `string`      | `string`        | `NVARCHAR`           | `string`        |
| `int`         | `int`           | `INT`                | `number`        |
| `long`        | `long`          | `BIGINT`             | `number`        |
| `decimal`     | `decimal`       | `DECIMAL(p,s)`       | `number`        |
| `double`      | `double`        | `FLOAT`              | `number`        |
| `float`       | `float`         | `REAL`               | `number`        |
| `date`        | `DateTime`      | `DATE`               | `Date`          |
| `datetime`    | `DateTime`      | `DATETIME2`          | `Date`          |
| `bool`        | `bool`          | `BIT`                | `boolean`       |
| `guid`        | `Guid`          | `UNIQUEIDENTIFIER`   | `string`        |
| `byte[]`      | `byte[]`        | `VARBINARY(MAX)`     | `string`        |

### Ejemplo

```json
{
  "nombre": "Email",
  "tipo": "string",
  "largo": 150,
  "requerido": true,
  "unico": true,
  "validaciones": [
    { "tipo": "email", "mensaje": "El email no tiene un formato válido" }
  ],
  "input": {
    "tipo": "email",
    "label": "Correo Electrónico",
    "placeholder": "ejemplo@correo.com"
  }
}
```

---

## 5. Validaciones

Las validaciones se definen dentro del arreglo `validaciones` de un campo.

| Propiedad  | Tipo   | Requerido | Descripción                             |
|------------|--------|-----------|-----------------------------------------|
| `tipo`     | string | ✅ Sí     | Tipo de regla de validación.            |
| `valor`    | any    | No        | Valor de referencia de la validación.  |
| `mensaje`  | string | No        | Mensaje de error personalizado.         |

### Tipos de Validación

| Tipo           | Valor esperado          | Descripción                                                     |
|----------------|-------------------------|-----------------------------------------------------------------|
| `minLength`    | entero                  | Longitud mínima de una cadena.                                  |
| `maxLength`    | entero                  | Longitud máxima de una cadena.                                  |
| `min`          | número                  | Valor mínimo numérico.                                          |
| `max`          | número                  | Valor máximo numérico.                                          |
| `email`        | (sin valor)             | Valida formato de correo electrónico.                           |
| `regex`        | cadena (expresión)      | Valida con expresión regular personalizada.                     |
| `fechaMaxima`  | fecha ISO o `"hoy"`     | Fecha máxima permitida. Usar `"hoy"` para la fecha actual.     |
| `fechaMinima`  | fecha ISO o `"hoy"`     | Fecha mínima permitida.                                         |
| `rango`        | `{ min, max }`          | Rango numérico o de fechas.                                     |
| `requerido`    | (sin valor)             | Campo obligatorio (alternativo al atributo `requerido`).        |
| `personalizado`| cadena (pseudocódigo)   | Regla de validación personalizada implementada manualmente.     |

### Ejemplo

```json
"validaciones": [
  { "tipo": "minLength", "valor": 3, "mensaje": "Mínimo 3 caracteres" },
  { "tipo": "email", "mensaje": "El email no es válido" },
  { "tipo": "fechaMaxima", "valor": "hoy", "mensaje": "La fecha no puede ser futura" },
  { "tipo": "regex", "valor": "^[A-Z0-9]+$", "mensaje": "Solo letras y números" }
]
```

---

## 6. Inputs de UI

La propiedad `input` define cómo se renderiza el campo en la interfaz de usuario (Angular).

| Propiedad            | Tipo    | Requerido | Descripción                                           |
|----------------------|---------|-----------|-------------------------------------------------------|
| `tipo`               | string  | ✅ Sí     | Tipo de control UI (ver tabla más abajo).             |
| `label`              | string  | No        | Etiqueta visible del campo.                           |
| `placeholder`        | string  | No        | Texto de marcador de posición.                        |
| `fuenteDatos`        | string  | No        | Nombre de entidad para dropdowns/autocomplete.        |
| `campoValor`         | string  | No        | Campo a usar como valor en el dropdown.               |
| `campoTexto`         | string  | No        | Campo a usar como texto visible en el dropdown.       |
| `soloLectura`        | boolean | No        | Si el campo es solo lectura. Por defecto: `false`.    |
| `ocultarEnFormulario`| boolean | No        | Ocultar en formulario. Por defecto: `false`.          |
| `ocultarEnListado`   | boolean | No        | Ocultar en listado. Por defecto: `false`.             |

### Tipos de Control UI

| Valor          | Componente Angular Material             | Descripción                              |
|----------------|-----------------------------------------|------------------------------------------|
| `text`         | `mat-form-field` + `input[type=text]`   | Campo de texto simple.                   |
| `email`        | `mat-form-field` + `input[type=email]`  | Campo de correo electrónico.             |
| `number`       | `mat-form-field` + `input[type=number]` | Campo numérico.                          |
| `datepicker`   | `mat-datepicker`                        | Selector de fecha.                       |
| `dropdown`     | `mat-select`                            | Lista desplegable.                       |
| `checkbox`     | `mat-checkbox`                          | Casilla de verificación.                 |
| `textarea`     | `mat-form-field` + `textarea`           | Área de texto multilínea.               |
| `radio`        | `mat-radio-group`                       | Grupo de botones de radio.               |
| `autocomplete` | `mat-autocomplete`                      | Campo con sugerencias automáticas.       |
| `password`     | `mat-form-field` + `input[type=password]`| Campo de contraseña.                    |
| `file`         | `input[type=file]`                      | Cargador de archivos.                    |
| `colorpicker`  | Componente de color personalizado.      | Selector de color.                       |
| `timepicker`   | `mat-timepicker`                        | Selector de hora.                        |

### Ejemplo

```json
"input": {
  "tipo": "dropdown",
  "label": "Tipo de Cliente",
  "fuenteDatos": "TipoCliente",
  "campoValor": "Id",
  "campoTexto": "Nombre",
  "placeholder": "Seleccione un tipo de cliente"
}
```

---

## 7. Relaciones

Las relaciones se pueden definir a nivel de campo (FK) o a nivel de entidad (propiedades de navegación).

| Propiedad        | Tipo    | Requerido | Descripción                                        |
|------------------|---------|-----------|----------------------------------------------------|
| `tipo`           | string  | ✅ Sí     | Tipo de relación (ver tabla más abajo).            |
| `entidad`        | string  | ✅ Sí     | Nombre de la entidad destino (PascalCase).         |
| `campo`          | string  | No        | Campo de la entidad destino que se referencia.     |
| `nombrePropiedad`| string  | No        | Nombre de la propiedad de navegación generada.     |
| `cascada`        | boolean | No        | Si aplica eliminación en cascada. Por defecto: `false`. |

### Tipos de Relación

| Tipo          | Descripción                                                    | Ejemplo                             |
|---------------|----------------------------------------------------------------|-------------------------------------|
| `ManyToOne`   | Muchos-a-uno. FK en la entidad actual.                        | `Clientes → TipoCliente`            |
| `OneToMany`   | Uno-a-muchos. Colección en la entidad actual.                 | `TipoCliente → Clientes`            |
| `ManyToMany`  | Muchos-a-muchos. Tabla intermedia generada.                   | `Productos ↔ Categorías`            |
| `OneToOne`    | Uno-a-uno. FK única en la entidad actual.                     | `Usuario → Perfil`                  |

### Ejemplo

```json
"relaciones": [
  {
    "tipo": "ManyToOne",
    "entidad": "TipoCliente",
    "campo": "Id",
    "nombrePropiedad": "TipoCliente",
    "cascada": false
  }
]
```

---

## 8. Reglas de Negocio

Las reglas de negocio se expresan en pseudocódigo y son transformadas en stubs de código por el generador.

| Propiedad   | Tipo    | Requerido | Descripción                                               |
|-------------|---------|-----------|-----------------------------------------------------------|
| `nombre`    | string  | ✅ Sí     | Identificador único de la regla.                          |
| `trigger`   | string  | ✅ Sí     | Momento de ejecución (ver tabla más abajo).               |
| `condicion` | string  | No        | Condición en pseudocódigo para disparar la regla.        |
| `accion`    | string  | No        | Acción en pseudocódigo a ejecutar si la condición es cierta. |
| `mensaje`   | string  | No        | Mensaje de error si la regla falla.                       |
| `activa`    | boolean | No        | Si la regla está activa. Por defecto: `true`.             |

### Triggers Disponibles

| Valor                 | Descripción                                              |
|-----------------------|----------------------------------------------------------|
| `antesDeInsertar`     | Se ejecuta antes de insertar un nuevo registro.          |
| `antesDeActualizar`   | Se ejecuta antes de actualizar un registro.              |
| `antesDeEliminar`     | Se ejecuta antes de eliminar un registro.                |
| `despuesDeInsertar`   | Se ejecuta después de insertar un nuevo registro.        |
| `despuesDeActualizar` | Se ejecuta después de actualizar un registro.            |
| `despuesDeEliminar`   | Se ejecuta después de eliminar un registro.              |
| `antesDeGuardar`      | Se ejecuta antes de insertar O actualizar.               |
| `despuesDeGuardar`    | Se ejecuta después de insertar O actualizar.             |
| `alValidar`           | Se ejecuta durante la validación del modelo.             |

### Ejemplo

```json
"reglasNegocio": [
  {
    "nombre": "ValidarClienteDuplicado",
    "trigger": "antesDeInsertar",
    "condicion": "ExisteCliente(Email) == true",
    "accion": "LanzarError",
    "mensaje": "Ya existe un cliente con este correo electrónico",
    "activa": true
  }
]
```

---

## 9. Configuración de Pantallas

La propiedad `pantallas` configura cómo se genera la UI de listado y formulario para la entidad.

### 9.1 Pantalla de Listado (`pantallas.listado`)

| Propiedad        | Tipo    | Descripción                                              |
|------------------|---------|----------------------------------------------------------|
| `titulo`         | string  | Título de la pantalla de listado.                        |
| `columnas`       | arreglo | Campos a mostrar como columnas (soporta dot notation: `TipoCliente.Nombre`). |
| `acciones`       | arreglo | Botones de acción: `ver`, `editar`, `eliminar`, `exportar`, `imprimir`, `personalizado`. |
| `filtros`        | arreglo | Campos por los que se puede filtrar.                     |
| `paginacion`     | objeto  | Configuración de paginación.                             |
| `ordenamiento`   | string  | Campo por defecto para ordenar.                          |
| `ordenAscendente`| boolean | Orden ascendente por defecto. Por defecto: `true`.       |

#### Paginación (`paginacion`)

| Propiedad            | Tipo    | Descripción                                            |
|----------------------|---------|--------------------------------------------------------|
| `registrosPorPagina` | entero  | Registros por página (5-200). Por defecto: `10`.      |
| `mostrarTodos`       | boolean | Opción para mostrar todos. Por defecto: `false`.       |
| `opcionesTamano`     | arreglo | Opciones de tamaño. Ej: `[10, 25, 50, 100]`.          |

### 9.2 Pantalla de Formulario (`pantallas.formulario`)

| Propiedad   | Tipo    | Descripción                                                  |
|-------------|---------|--------------------------------------------------------------|
| `titulo`    | string  | Título del formulario.                                       |
| `layout`    | string  | Disposición: `simple`, `tabs`, `steps`, `accordion`, `two-columns`. |
| `campos`    | arreglo | Campos a incluir en el formulario.                          |
| `secciones` | arreglo | Secciones del formulario.                                   |

#### Secciones (`secciones`)

| Propiedad   | Tipo    | Descripción                                          |
|-------------|---------|------------------------------------------------------|
| `titulo`    | string  | Título de la sección. Requerido.                    |
| `campos`    | arreglo | Nombres de campos incluidos en la sección.          |
| `columnas`  | entero  | Número de columnas (1-4). Por defecto: `2`.         |
| `colapsable`| boolean | Si la sección es colapsable. Por defecto: `false`.  |

### Ejemplo

```json
"pantallas": {
  "listado": {
    "titulo": "Clientes",
    "columnas": ["Nombre", "Apellido", "Email", "TipoCliente.Nombre"],
    "acciones": ["ver", "editar", "eliminar"],
    "filtros": ["Nombre", "Email"],
    "paginacion": { "registrosPorPagina": 15, "mostrarTodos": false },
    "ordenamiento": "Apellido"
  },
  "formulario": {
    "titulo": "Cliente",
    "layout": "two-columns",
    "secciones": [
      { "titulo": "Datos Personales", "campos": ["Nombre", "Apellido"], "columnas": 2 }
    ]
  }
}
```

---

## 10. Configuración de API

La propiedad `api` configura la capa REST generada para la entidad.

| Propiedad      | Tipo    | Descripción                                                       |
|----------------|---------|-------------------------------------------------------------------|
| `ruta`         | string  | Ruta base de la API. Ej: `"/api/clientes"`.                      |
| `operaciones`  | arreglo | Operaciones habilitadas: `getAll`, `getById`, `create`, `update`, `delete`, `search`, `export`. |
| `autenticacion`| boolean | Si requiere autenticación JWT. Por defecto: `true`.               |
| `roles`        | arreglo | Roles con acceso. Deben estar definidos en `configuracion.roles`. |
| `versionApi`   | string  | Versión de la API. Ej: `"v1"`.                                   |

### Ejemplo

```json
"api": {
  "ruta": "/api/clientes",
  "operaciones": ["getAll", "getById", "create", "update", "delete"],
  "autenticacion": true,
  "roles": ["Admin", "Ventas"]
}
```

---

## 11. Configuración Global

La propiedad `configuracion` define parámetros globales del proyecto.

| Propiedad         | Tipo    | Descripción                                             |
|-------------------|---------|---------------------------------------------------------|
| `namespaceBackend`| string  | Namespace base del proyecto .NET. Ej: `"SistemaVentas.Api"`. |
| `nombreFrontend`  | string  | Nombre del proyecto Angular. Ej: `"sistema-ventas"`.   |
| `autenticacion`   | objeto  | Configuración de autenticación JWT.                    |
| `roles`           | arreglo | Roles del sistema. Ej: `["Admin", "Ventas"]`.          |
| `baseDatos`       | objeto  | Configuración de conexión a la base de datos.           |
| `cors`            | arreglo | Orígenes CORS permitidos.                              |

### 11.1 Autenticación (`configuracion.autenticacion`)

| Propiedad       | Tipo    | Descripción                                   |
|-----------------|---------|-----------------------------------------------|
| `tipo`          | string  | Tipo: `jwt`, `oauth2`, `apikey`, `none`.       |
| `key`           | string  | Clave secreta JWT.                            |
| `issuer`        | string  | Emisor del token.                             |
| `audience`      | string  | Audiencia del token.                          |
| `expireMinutes` | entero  | Minutos de expiración del token (mín: 1).     |

### 11.2 Base de Datos (`configuracion.baseDatos`)

| Propiedad         | Tipo    | Descripción                                  |
|-------------------|---------|----------------------------------------------|
| `servidor`        | string  | Servidor de base de datos.                   |
| `nombreBaseDatos` | string  | Nombre de la base de datos.                  |
| `usuario`         | string  | Usuario de conexión.                         |
| `contrasena`      | string  | Contraseña de conexión.                      |
| `puerto`          | entero  | Puerto de conexión.                          |
| `esquema`         | string  | Esquema por defecto. Ej: `"dbo"`.            |

### Ejemplo

```json
"configuracion": {
  "namespaceBackend": "SistemaVentas.Api",
  "nombreFrontend": "sistema-ventas",
  "autenticacion": {
    "tipo": "jwt",
    "key": "SuperSecretKey_MustBe32CharsMinimum!",
    "issuer": "SistemaVentas.Api",
    "audience": "SistemaVentas.App",
    "expireMinutes": 480
  },
  "roles": ["Admin", "Ventas", "Bodega"],
  "baseDatos": {
    "servidor": "localhost",
    "nombreBaseDatos": "SistemaVentasDB",
    "usuario": "sa",
    "contrasena": "TuContraseñaSegura!",
    "puerto": 1433,
    "esquema": "dbo"
  },
  "cors": ["http://localhost:4200"]
}
```

---

## Notas Importantes

1. **Nombres en PascalCase**: Los nombres de entidades y campos deben ser PascalCase (`Cliente`, no `cliente` ni `CLIENTE`).
2. **Tipos consistentes**: El tipo del campo en JSON debe coincidir con los valores permitidos exactamente (ej: `"string"`, no `"String"`).
3. **Relaciones bidireccionales**: Se recomienda definir la relación en ambas entidades para generar las propiedades de navegación correctas.
4. **Reglas de negocio**: El código pseudocódigo es una guía para el desarrollador; el generador crea stubs con comentarios explicativos.
5. **Roles**: Los roles en `api.roles` deben estar definidos en `configuracion.roles`.
