using AicaseCli.Core;
using AicaseCli.Models;

namespace AicaseCli.Generators;

/// <summary>
/// Genera todo el código backend .NET 8 para cada entidad del proyecto.
/// </summary>
public class BackendGenerator
{
    private readonly TemplateEngine _engine;
    private readonly string _templatesPath;

    public BackendGenerator(TemplateEngine engine, string templatesPath)
    {
        _engine = engine;
        _templatesPath = templatesPath;
    }

    public async Task GenerateAsync(ProjectDefinition project, string outputBasePath)
    {
        var backendPath = Path.Combine(outputBasePath, "backend");

        foreach (var entity in project.Entidades)
        {
            var model = BuildModel(project, entity);

            await GenerateModelAsync(model, backendPath);
            await GenerateDtoAsync(model, backendPath);
            await GenerateRepositoryInterfaceAsync(model, backendPath);
            await GenerateRepositoryAsync(model, backendPath);
            await GenerateServiceInterfaceAsync(model, backendPath);
            await GenerateServiceAsync(model, backendPath);
            await GenerateControllerAsync(model, backendPath);
            await GenerateValidatorAsync(model, backendPath);
            await GenerateDbContextConfigAsync(model, backendPath);
        }

        // Generar archivo con todos los DbSet para el ApplicationDbContext
        await GenerateDbContextSnippetAsync(project, backendPath);
        Console.WriteLine($"  → {project.Entidades.Count} entidades backend generadas.");
    }

    private static object BuildModel(ProjectDefinition project, EntityDefinition entity) => new
    {
        Project = new
        {
            project.Nombre,
            project.Version,
            Namespace = project.Namespace,
            Roles = project.Configuracion?.Roles ?? new List<string>()
        },
        Entity = entity
    };

    private async Task GenerateModelAsync(object model, string backendPath)
    {
        var entity = ((dynamic)model).Entity as EntityDefinition;
        var ns = ((dynamic)model).Project.Namespace as string;
        var outputPath = Path.Combine(backendPath, "Models", $"{entity!.Nombre}.cs");

        var templatePath = Path.Combine(_templatesPath, "backend", "model.scriban");
        if (File.Exists(templatePath))
        {
            await _engine.RenderToFileAsync(templatePath, model, outputPath);
        }
        else
        {
            await _engine.RenderStringToFileAsync(GetEmbeddedModelTemplate(), model, outputPath);
        }
    }

    private async Task GenerateDtoAsync(object model, string backendPath)
    {
        var entity = ((dynamic)model).Entity as EntityDefinition;
        var outputPath = Path.Combine(backendPath, "DTOs", $"{entity!.Nombre}Dtos.cs");

        var templatePath = Path.Combine(_templatesPath, "backend", "dto.scriban");
        if (File.Exists(templatePath))
            await _engine.RenderToFileAsync(templatePath, model, outputPath);
        else
            await _engine.RenderStringToFileAsync(GetEmbeddedDtoTemplate(), model, outputPath);
    }

    private async Task GenerateRepositoryInterfaceAsync(object model, string backendPath)
    {
        var entity = ((dynamic)model).Entity as EntityDefinition;
        var outputPath = Path.Combine(backendPath, "Repositories", $"I{entity!.Nombre}Repository.cs");

        var templatePath = Path.Combine(_templatesPath, "backend", "repository-interface.scriban");
        if (File.Exists(templatePath))
            await _engine.RenderToFileAsync(templatePath, model, outputPath);
        else
            await _engine.RenderStringToFileAsync(GetEmbeddedRepositoryInterfaceTemplate(), model, outputPath);
    }

    private async Task GenerateRepositoryAsync(object model, string backendPath)
    {
        var entity = ((dynamic)model).Entity as EntityDefinition;
        var outputPath = Path.Combine(backendPath, "Repositories", $"{entity!.Nombre}Repository.cs");

        var templatePath = Path.Combine(_templatesPath, "backend", "repository.scriban");
        if (File.Exists(templatePath))
            await _engine.RenderToFileAsync(templatePath, model, outputPath);
        else
            await _engine.RenderStringToFileAsync(GetEmbeddedRepositoryTemplate(), model, outputPath);
    }

    private async Task GenerateServiceInterfaceAsync(object model, string backendPath)
    {
        var entity = ((dynamic)model).Entity as EntityDefinition;
        var outputPath = Path.Combine(backendPath, "Services", $"I{entity!.Nombre}Service.cs");

        var templatePath = Path.Combine(_templatesPath, "backend", "service-interface.scriban");
        if (File.Exists(templatePath))
            await _engine.RenderToFileAsync(templatePath, model, outputPath);
        else
            await _engine.RenderStringToFileAsync(GetEmbeddedServiceInterfaceTemplate(), model, outputPath);
    }

    private async Task GenerateServiceAsync(object model, string backendPath)
    {
        var entity = ((dynamic)model).Entity as EntityDefinition;
        var outputPath = Path.Combine(backendPath, "Services", $"{entity!.Nombre}Service.cs");

        var templatePath = Path.Combine(_templatesPath, "backend", "service.scriban");
        if (File.Exists(templatePath))
            await _engine.RenderToFileAsync(templatePath, model, outputPath);
        else
            await _engine.RenderStringToFileAsync(GetEmbeddedServiceTemplate(), model, outputPath);
    }

    private async Task GenerateControllerAsync(object model, string backendPath)
    {
        var entity = ((dynamic)model).Entity as EntityDefinition;
        var outputPath = Path.Combine(backendPath, "Controllers", $"{entity!.Nombre}Controller.cs");

        var templatePath = Path.Combine(_templatesPath, "backend", "controller.scriban");
        if (File.Exists(templatePath))
            await _engine.RenderToFileAsync(templatePath, model, outputPath);
        else
            await _engine.RenderStringToFileAsync(GetEmbeddedControllerTemplate(), model, outputPath);
    }

    private async Task GenerateValidatorAsync(object model, string backendPath)
    {
        var entity = ((dynamic)model).Entity as EntityDefinition;
        var outputPath = Path.Combine(backendPath, "Validators", $"{entity!.Nombre}Validator.cs");

        var templatePath = Path.Combine(_templatesPath, "backend", "validator.scriban");
        if (File.Exists(templatePath))
            await _engine.RenderToFileAsync(templatePath, model, outputPath);
        else
            await _engine.RenderStringToFileAsync(GetEmbeddedValidatorTemplate(), model, outputPath);
    }

    private async Task GenerateDbContextConfigAsync(object model, string backendPath)
    {
        var entity = ((dynamic)model).Entity as EntityDefinition;
        var outputPath = Path.Combine(backendPath, "Data", "Configurations", $"{entity!.Nombre}Configuration.cs");

        var templatePath = Path.Combine(_templatesPath, "backend", "dbcontext-config.scriban");
        if (File.Exists(templatePath))
            await _engine.RenderToFileAsync(templatePath, model, outputPath);
        else
            await _engine.RenderStringToFileAsync(GetEmbeddedDbContextConfigTemplate(), model, outputPath);
    }

    private async Task GenerateDbContextSnippetAsync(ProjectDefinition project, string backendPath)
    {
        var lines = new List<string>
        {
            $"// DbSets generados por AICASE para {project.Nombre}",
            $"// Copie estos DbSet en su ApplicationDbContext:",
            ""
        };

        foreach (var entity in project.Entidades)
            lines.Add($"public DbSet<{entity.Nombre}> {entity.Tabla} => Set<{entity.Nombre}>();");

        var outputPath = Path.Combine(backendPath, "Data", "DbContextSnippet.txt");
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
        await File.WriteAllLinesAsync(outputPath, lines);
    }

    // ── Plantillas embebidas (fallback cuando no existen archivos .scriban) ──

    private static string GetEmbeddedModelTemplate() => """
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace {{ project.namespace }}.Models;

/// <summary>{{ entity.descripcion }}</summary>
[Table("{{ entity.tabla }}")]
public class {{ entity.nombre }}
{
    {{- for campo in entity.campos }}
    {{- if campo.es_pk }}
    [Key]
    {{- if campo.auto_incremento }}
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    {{- end }}
    {{- end }}
    {{- if campo.requerido && !campo.es_pk }}
    [Required]
    {{- end }}
    {{- if campo.largo && campo.largo > 0 }}
    [MaxLength({{ campo.largo }})]
    {{- end }}
    public {{ campo.tipo | csharp_type }}{{ if !campo.requerido && !campo.es_pk }}?{{ end }} {{ campo.nombre }} { get; set; }

    {{- end }}
    {{- for rel in entity.relaciones }}
    {{- if rel.tipo == "ManyToOne" }}
    [ForeignKey("{{ rel.campo ?? rel.entidad + "Id" }}")]
    public virtual {{ rel.entidad }}? {{ rel.nombre_propiedad ?? rel.entidad }} { get; set; }
    {{- end }}
    {{- if rel.tipo == "OneToMany" }}
    public virtual ICollection<{{ rel.entidad }}> {{ rel.nombre_propiedad ?? rel.entidad + "s" }} { get; set; } = new List<{{ rel.entidad }}>();
    {{- end }}
    {{- end }}
}
""";

    private static string GetEmbeddedDtoTemplate() => """
using System.ComponentModel.DataAnnotations;

namespace {{ project.namespace }}.DTOs;

public class {{ entity.nombre }}CreateDto
{
    {{- for campo in entity.campos }}
    {{- if !campo.es_pk }}
    {{- if campo.requerido }}
    [Required]
    {{- end }}
    {{- if campo.largo && campo.largo > 0 }}
    [MaxLength({{ campo.largo }})]
    {{- end }}
    public {{ campo.tipo | csharp_type }}{{ if !campo.requerido }}?{{ end }} {{ campo.nombre }} { get; set; }

    {{- end }}
    {{- end }}
}

public class {{ entity.nombre }}UpdateDto : {{ entity.nombre }}CreateDto
{
    [Required]
    public int Id { get; set; }
}

public class {{ entity.nombre }}ResponseDto
{
    {{- for campo in entity.campos }}
    public {{ campo.tipo | csharp_type }}{{ if !campo.requerido && !campo.es_pk }}?{{ end }} {{ campo.nombre }} { get; set; }
    {{- end }}
}
""";

    private static string GetEmbeddedRepositoryInterfaceTemplate() => """
using {{ project.namespace }}.Models;

namespace {{ project.namespace }}.Repositories;

public interface I{{ entity.nombre }}Repository
{
    Task<(IEnumerable<{{ entity.nombre }}> Items, int TotalCount)> GetAllAsync(int page = 1, int pageSize = 10, string? search = null);
    Task<{{ entity.nombre }}?> GetByIdAsync(int id);
    Task<{{ entity.nombre }}> CreateAsync({{ entity.nombre }} entity);
    Task<{{ entity.nombre }}> UpdateAsync({{ entity.nombre }} entity);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}
""";

    private static string GetEmbeddedRepositoryTemplate() => """
using Microsoft.EntityFrameworkCore;
using {{ project.namespace }}.Data;
using {{ project.namespace }}.Models;

namespace {{ project.namespace }}.Repositories;

public class {{ entity.nombre }}Repository : I{{ entity.nombre }}Repository
{
    private readonly ApplicationDbContext _context;

    public {{ entity.nombre }}Repository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<{{ entity.nombre }}> Items, int TotalCount)> GetAllAsync(
        int page = 1, int pageSize = 10, string? search = null)
    {
        var query = _context.Set<{{ entity.nombre }}>().AsQueryable();

        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<{{ entity.nombre }}?> GetByIdAsync(int id)
        => await _context.Set<{{ entity.nombre }}>().FindAsync(id);

    public async Task<{{ entity.nombre }}> CreateAsync({{ entity.nombre }} entity)
    {
        _context.Set<{{ entity.nombre }}>().Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<{{ entity.nombre }}> UpdateAsync({{ entity.nombre }} entity)
    {
        _context.Set<{{ entity.nombre }}>().Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity == null) return false;
        _context.Set<{{ entity.nombre }}>().Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
        => await _context.Set<{{ entity.nombre }}>().AnyAsync(e => EF.Property<int>(e, "Id") == id);
}
""";

    private static string GetEmbeddedServiceInterfaceTemplate() => """
using {{ project.namespace }}.DTOs;

namespace {{ project.namespace }}.Services;

public interface I{{ entity.nombre }}Service
{
    Task<(IEnumerable<{{ entity.nombre }}ResponseDto> Items, int TotalCount)> GetAllAsync(int page = 1, int pageSize = 10, string? search = null);
    Task<{{ entity.nombre }}ResponseDto?> GetByIdAsync(int id);
    Task<{{ entity.nombre }}ResponseDto> CreateAsync({{ entity.nombre }}CreateDto dto);
    Task<{{ entity.nombre }}ResponseDto?> UpdateAsync(int id, {{ entity.nombre }}UpdateDto dto);
    Task<bool> DeleteAsync(int id);
}
""";

    private static string GetEmbeddedServiceTemplate() => """
using AutoMapper;
using {{ project.namespace }}.DTOs;
using {{ project.namespace }}.Models;
using {{ project.namespace }}.Repositories;

namespace {{ project.namespace }}.Services;

public class {{ entity.nombre }}Service : I{{ entity.nombre }}Service
{
    private readonly I{{ entity.nombre }}Repository _repository;
    private readonly IMapper _mapper;

    public {{ entity.nombre }}Service(I{{ entity.nombre }}Repository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<(IEnumerable<{{ entity.nombre }}ResponseDto> Items, int TotalCount)> GetAllAsync(
        int page = 1, int pageSize = 10, string? search = null)
    {
        var (items, total) = await _repository.GetAllAsync(page, pageSize, search);
        return (_mapper.Map<IEnumerable<{{ entity.nombre }}ResponseDto>>(items), total);
    }

    public async Task<{{ entity.nombre }}ResponseDto?> GetByIdAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity == null ? null : _mapper.Map<{{ entity.nombre }}ResponseDto>(entity);
    }

    public async Task<{{ entity.nombre }}ResponseDto> CreateAsync({{ entity.nombre }}CreateDto dto)
    {
        {{- for regla in entity.reglas_negocio }}
        {{- if regla.trigger == "antesDeInsertar" && regla.activa }}
        // TODO: Regla de negocio - {{ regla.nombre }}
        // Condición: {{ regla.condicion }}
        // Acción: {{ regla.accion }}
        // Mensaje: {{ regla.mensaje }}
        {{- end }}
        {{- end }}

        var entity = _mapper.Map<{{ entity.nombre }}>(dto);
        var created = await _repository.CreateAsync(entity);
        return _mapper.Map<{{ entity.nombre }}ResponseDto>(created);
    }

    public async Task<{{ entity.nombre }}ResponseDto?> UpdateAsync(int id, {{ entity.nombre }}UpdateDto dto)
    {
        if (!await _repository.ExistsAsync(id)) return null;

        {{- for regla in entity.reglas_negocio }}
        {{- if regla.trigger == "antesDeActualizar" && regla.activa }}
        // TODO: Regla de negocio - {{ regla.nombre }}
        // Condición: {{ regla.condicion }}
        {{- end }}
        {{- end }}

        var entity = _mapper.Map<{{ entity.nombre }}>(dto);
        var updated = await _repository.UpdateAsync(entity);
        return _mapper.Map<{{ entity.nombre }}ResponseDto>(updated);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        {{- for regla in entity.reglas_negocio }}
        {{- if regla.trigger == "antesDeEliminar" && regla.activa }}
        // TODO: Regla de negocio - {{ regla.nombre }}
        // Condición: {{ regla.condicion }}
        {{- end }}
        {{- end }}

        return await _repository.DeleteAsync(id);
    }
}
""";

    private static string GetEmbeddedControllerTemplate() => """
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using {{ project.namespace }}.DTOs;
using {{ project.namespace }}.Services;

namespace {{ project.namespace }}.Controllers;

[ApiController]
[Route("{{ entity.api.ruta ?? "/api/" + (entity.nombre | to_kebab_case) + "s" }}")]
{{- if entity.api.autenticacion }}
[Authorize{{- if entity.api.roles && entity.api.roles.size > 0 }}(Roles = "{{ array.join entity.api.roles ", " }}"){{- end }}]
{{- end }}
public class {{ entity.nombre }}Controller : ControllerBase
{
    private readonly I{{ entity.nombre }}Service _service;

    public {{ entity.nombre }}Controller(I{{ entity.nombre }}Service service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
    {
        var (items, total) = await _service.GetAllAsync(page, pageSize, search);
        return Ok(new { items, totalCount = total, page, pageSize });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _service.GetByIdAsync(id);
        return item == null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] {{ entity.nombre }}CreateDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] {{ entity.nombre }}UpdateDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var updated = await _service.UpdateAsync(id, dto);
        return updated == null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
""";

    private static string GetEmbeddedValidatorTemplate() => """
using FluentValidation;
using {{ project.namespace }}.DTOs;

namespace {{ project.namespace }}.Validators;

public class {{ entity.nombre }}CreateValidator : AbstractValidator<{{ entity.nombre }}CreateDto>
{
    public {{ entity.nombre }}CreateValidator()
    {
        {{- for campo in entity.campos }}
        {{- if !campo.es_pk }}
        {{- if campo.requerido }}
        RuleFor(x => x.{{ campo.nombre }}).NotEmpty().WithMessage("{{ campo.nombre }} es requerido.");
        {{- end }}
        {{- for validacion in campo.validaciones }}
        {{- if validacion.tipo == "minLength" }}
        RuleFor(x => x.{{ campo.nombre }}).MinimumLength({{ validacion.valor }}).WithMessage("{{ validacion.mensaje ?? campo.nombre + " mínimo " + validacion.valor + " caracteres." }}");
        {{- end }}
        {{- if validacion.tipo == "maxLength" }}
        RuleFor(x => x.{{ campo.nombre }}).MaximumLength({{ validacion.valor }}).WithMessage("{{ validacion.mensaje ?? campo.nombre + " máximo " + validacion.valor + " caracteres." }}");
        {{- end }}
        {{- if validacion.tipo == "email" }}
        RuleFor(x => x.{{ campo.nombre }}).EmailAddress().WithMessage("{{ validacion.mensaje ?? "El formato del email no es válido." }}");
        {{- end }}
        {{- if validacion.tipo == "min" }}
        RuleFor(x => x.{{ campo.nombre }}).GreaterThanOrEqualTo({{ validacion.valor }}).WithMessage("{{ validacion.mensaje ?? campo.nombre + " debe ser mayor o igual a " + validacion.valor }}");
        {{- end }}
        {{- if validacion.tipo == "max" }}
        RuleFor(x => x.{{ campo.nombre }}).LessThanOrEqualTo({{ validacion.valor }}).WithMessage("{{ validacion.mensaje ?? campo.nombre + " debe ser menor o igual a " + validacion.valor }}");
        {{- end }}
        {{- if validacion.tipo == "regex" }}
        RuleFor(x => x.{{ campo.nombre }}).Matches(@"{{ validacion.valor }}").WithMessage("{{ validacion.mensaje ?? "El formato de " + campo.nombre + " no es válido." }}");
        {{- end }}
        {{- end }}
        {{- end }}
        {{- end }}
    }
}

public class {{ entity.nombre }}UpdateValidator : {{ entity.nombre }}CreateValidator
{
    public {{ entity.nombre }}UpdateValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("Id debe ser mayor a 0.");
    }
}
""";

    private static string GetEmbeddedDbContextConfigTemplate() => """
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using {{ project.namespace }}.Models;

namespace {{ project.namespace }}.Data.Configurations;

public class {{ entity.nombre }}Configuration : IEntityTypeConfiguration<{{ entity.nombre }}>
{
    public void Configure(EntityTypeBuilder<{{ entity.nombre }}> builder)
    {
        builder.ToTable("{{ entity.tabla }}");

        {{- for campo in entity.campos }}
        {{- if campo.es_pk }}
        builder.HasKey(e => e.{{ campo.nombre }});
        {{- if campo.auto_incremento }}
        builder.Property(e => e.{{ campo.nombre }}).ValueGeneratedOnAdd();
        {{- end }}
        {{- else }}
        builder.Property(e => e.{{ campo.nombre }})
            {{- if campo.requerido }}
            .IsRequired()
            {{- end }}
            {{- if campo.largo && campo.largo > 0 }}
            .HasMaxLength({{ campo.largo }})
            {{- end }}
            {{- if campo.tipo == "decimal" }}
            .HasPrecision({{ campo.precision ?? 18 }}, {{ campo.escala ?? 2 }})
            {{- end }}
            ;
        {{- if campo.unico }}
        builder.HasIndex(e => e.{{ campo.nombre }}).IsUnique();
        {{- end }}
        {{- end }}
        {{- end }}

        {{- for rel in entity.relaciones }}
        {{- if rel.tipo == "ManyToOne" }}
        builder.HasOne(e => e.{{ rel.nombre_propiedad ?? rel.entidad }})
               .WithMany()
               .HasForeignKey("{{ rel.entidad }}Id")
               .OnDelete({{ if rel.cascada }}DeleteBehavior.Cascade{{ else }}DeleteBehavior.Restrict{{ end }});
        {{- end }}
        {{- end }}
    }
}
""";
}
