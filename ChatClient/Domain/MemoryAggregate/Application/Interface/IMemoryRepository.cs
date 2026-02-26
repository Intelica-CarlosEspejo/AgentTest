using AgentTest.Agent.Domain.MemoryAggregate.Application.DTO;
using AgentTest.Agent.Domain.MemoryAggregate.Domain;

namespace AgentTest.Agent.Domain.MemoryAggregate.Application.Interface;
public interface IMemoryRepository
{
    void Create(Memory memory);
    List<MemorySimpleResponse> GetResponse();
}