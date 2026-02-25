using AgentTest.Agent.Domain.Common.EFCore;
using AgentTest.Agent.Domain.MemoryAggregate.Application.DTO;
using AgentTest.Agent.Domain.MemoryAggregate.Application.Interface;
using AgentTest.Agent.Domain.MemoryAggregate.Domain;
namespace AgentTest.Agent.Domain.MemoryAggregate.Infrastructure;
public class MemoryRepository(Context context) : IMemoryRepository
{
    public void Create(Memory memory)
    {
        context.Add(memory);
        context.SaveChangesAsync();
    }
    public List<MemorySimpleResponse> GetResponse()
    {
        var query = from memory in context.Memories select new MemorySimpleResponse(memory.MemoryID, memory.Message);
        return [.. query];
    }
}