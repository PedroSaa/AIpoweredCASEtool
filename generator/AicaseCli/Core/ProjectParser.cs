using AicaseCli.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace AicaseCli.Core;

/// <summary>
/// Parsea el contenido JSON de un proyecto AICASE en el modelo de C# <see cref="ProjectDefinition"/>.
/// </summary>
public class ProjectParser
{
    private static readonly JsonSerializerSettings Settings = new()
    {
        ContractResolver = new DefaultContractResolver(),
        NullValueHandling = NullValueHandling.Ignore,
        MissingMemberHandling = MissingMemberHandling.Ignore,
        DateFormatHandling = DateFormatHandling.IsoDateFormat
    };

    /// <summary>
    /// Deserializa el JSON en un <see cref="ProjectDefinition"/>.
    /// </summary>
    /// <param name="jsonContent">Contenido JSON del archivo de proyecto.</param>
    /// <returns>Modelo de definición del proyecto.</returns>
    /// <exception cref="InvalidOperationException">Si el JSON no puede deserializarse.</exception>
    public ProjectDefinition Parse(string jsonContent)
    {
        if (string.IsNullOrWhiteSpace(jsonContent))
            throw new ArgumentException("El contenido JSON no puede estar vacío.", nameof(jsonContent));

        try
        {
            var project = JsonConvert.DeserializeObject<ProjectDefinition>(jsonContent, Settings);

            if (project == null)
                throw new InvalidOperationException("El JSON no pudo deserializarse en un ProjectDefinition.");

            ValidateRequiredFields(project);
            NormalizeProject(project);

            return project;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Error al parsear el JSON: {ex.Message}", ex);
        }
    }

    private static void ValidateRequiredFields(ProjectDefinition project)
    {
        if (string.IsNullOrWhiteSpace(project.Nombre))
            throw new InvalidOperationException("El proyecto debe tener un nombre.");

        if (project.Entidades == null || project.Entidades.Count == 0)
            throw new InvalidOperationException("El proyecto debe tener al menos una entidad.");

        foreach (var entity in project.Entidades)
        {
            if (string.IsNullOrWhiteSpace(entity.Nombre))
                throw new InvalidOperationException("Todas las entidades deben tener un nombre.");

            if (string.IsNullOrWhiteSpace(entity.Tabla))
                throw new InvalidOperationException($"La entidad '{entity.Nombre}' debe tener un nombre de tabla.");

            if (entity.Campos == null || entity.Campos.Count == 0)
                throw new InvalidOperationException($"La entidad '{entity.Nombre}' debe tener al menos un campo.");
        }
    }

    private static void NormalizeProject(ProjectDefinition project)
    {
        // Normalizar entidades
        foreach (var entity in project.Entidades)
        {
            entity.Relaciones ??= new();
            entity.ReglasNegocio ??= new();

            // Consolidar relaciones definidas a nivel de campo en la lista de relaciones de la entidad
            foreach (var campo in entity.Campos)
            {
                if (campo.Relacion != null && !entity.Relaciones.Any(r =>
                    r.Entidad == campo.Relacion.Entidad && r.Tipo == campo.Relacion.Tipo))
                {
                    entity.Relaciones.Add(campo.Relacion);
                }
            }
        }
    }
}
