# Repositories

Generated repository interfaces and implementations are placed here by AICASE.

Pattern: `IEntityRepository` + `EntityRepository : IEntityRepository`

Repositories use EF Core and provide async CRUD methods. They are registered in `Program.cs` with `AddScoped`.
