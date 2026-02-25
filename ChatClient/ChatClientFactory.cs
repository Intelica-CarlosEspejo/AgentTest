using AgentTest.Agent.Domain.MemoryAggregate.Application.Interface;
namespace AgentTest.ChatClient;

public class ChatClientFactory(IMemoryRepository memoryRepository)
{
    public ChatClient Create(ProviderType provider, ModelType model, string key)
    {
        string imnportantDescription = "Llama a esta función cuando el usuario mencione información importante para que sea almacenada en memoria.";
        return new ChatClient(memoryRepository, provider, model, key, imnportantDescription);
    }
}
