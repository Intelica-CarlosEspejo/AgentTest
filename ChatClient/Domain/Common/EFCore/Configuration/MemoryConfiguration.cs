using AgentTest.Agent.Domain.MemoryAggregate.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace AgentTest.Agent.Domain.Common.EFCore.Configuration;
public class MemoryConfiguration : IEntityTypeConfiguration<Memory>
{
    public void Configure(EntityTypeBuilder<Memory> builder)
    {
        builder.ToTable(nameof(Memory).ToLower()).HasKey(x => x.MemoryID);
    }
}