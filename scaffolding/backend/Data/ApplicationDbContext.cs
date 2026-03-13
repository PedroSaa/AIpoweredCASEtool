using Microsoft.EntityFrameworkCore;

namespace {{ProjectName}}.Data;

/// <summary>
/// DbContext principal de la aplicación.
/// Las configuraciones de entidades (Fluent API) son generadas por AICASE.
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // DbSets generados por AICASE:
    // public DbSet<Cliente> Clientes => Set<Cliente>();
    // public DbSet<TipoCliente> TiposCliente => Set<TipoCliente>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplicar todas las configuraciones Fluent API del ensamblado
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
