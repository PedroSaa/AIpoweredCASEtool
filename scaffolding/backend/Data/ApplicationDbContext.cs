using Microsoft.EntityFrameworkCore;

namespace BackendBase.Data;

/// <summary>
/// DbContext base del proyecto. Los modelos generados por AICASE
/// se registran aquí mediante OnModelCreating.
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Los DbSet<T> generados por AICASE se añaden aquí.
    // Ejemplo: public DbSet<Cliente> Clientes => Set<Cliente>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplicar todas las configuraciones de entidad del ensamblado actual
        // (las clases IEntityTypeConfiguration<T> generadas por AICASE)
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    /// <summary>
    /// Sobreescritura de SaveChangesAsync para auditoría automática (opcional).
    /// </summary>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Aquí se puede agregar lógica de auditoría (CreatedAt, UpdatedAt)
        return base.SaveChangesAsync(cancellationToken);
    }
}
