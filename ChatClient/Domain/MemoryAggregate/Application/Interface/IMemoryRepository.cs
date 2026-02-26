using AgentTest.Agent.Domain.MemoryAggregate.Application.DTO;
using AgentTest.ChatClient.Domain.MemoryAggregate.Domain;
namespace AgentTest.ChatClient.Domain.MemoryAggregate.Application.Interface;

public interface IMemoryRepository
{
    void Create(Memory memory);
    void Delete(List<Guid> ids);
    List<MemorySimpleResponse> GetResponse(int maxMessages);
}