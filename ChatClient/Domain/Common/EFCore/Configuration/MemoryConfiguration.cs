
using AgentTest.ChatClient.Domain.MemoryAggregate.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace AgentTest.ChatClient.Domain.Common.EFCore.Configuration;

public class MemoryConfiguration : IEntityTypeConfiguration<Memory>
{
    public void Configure(EntityTypeBuilder<Memory> builder)
    {
        builder.ToTable(nameof(Memory).ToLower(), "ai").HasKey(x => x.MemoryID);
    }
}