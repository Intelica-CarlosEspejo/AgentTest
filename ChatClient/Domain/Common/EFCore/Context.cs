using AgentTest.ChatClient.Domain.Common.EFCore.Configuration;
using AgentTest.ChatClient.Domain.MemoryAggregate.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.NameTranslation;
namespace AgentTest.Agent.Domain.Common.EFCore;

public class Context(DbContextOptions<Context> options) : DbContext(options)
{
    public DbSet<Memory> Memories { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new MemoryConfiguration());
        var mapper = new NpgsqlSnakeCaseNameTranslator();
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            entity.SetTableName(mapper.TranslateTypeName(entity.GetTableName() ?? ""));
            foreach (var property in entity.GetProperties())
            {
                property.SetColumnName(mapper.TranslateMemberName(property.GetColumnName()).Replace("_", string.Empty));
            }
        }
        // Aplicar conversion global para DateTime
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var dateTimeProperties = entityType.ClrType.GetProperties().Where(p => p.PropertyType == typeof(DateTime));
            foreach (var property in dateTimeProperties)
            {
                modelBuilder.Entity(entityType.Name).Property(property.Name).HasConversion(
                    new ValueConverter<DateTime, DateTime>(v => v.ToUniversalTime(), v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
                );
            }
        }
    }
}