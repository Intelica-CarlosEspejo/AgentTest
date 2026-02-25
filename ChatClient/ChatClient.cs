using System.ComponentModel;
using AgentTest.Agent.Domain.MemoryAggregate.Application.Interface;
using AgentTest.Agent.Domain.MemoryAggregate.Domain;
using Anthropic;
using Microsoft.Extensions.AI;
using OpenAI;
namespace AgentTest.ChatClient;

public class ChatClient
{
    public List<ChatMessage> Messages { get; private set; } = [];
    public ChatOptions ChatOptions { get; private set; } = new ChatOptions() { Tools = [] };
    private readonly IMemoryRepository chatClientRepository;
    private readonly IChatClient chatClient;
    public ChatClient(IMemoryRepository repository, ProviderType provider, ModelType model, string key, string importantDescription)
    {
        chatClientRepository = repository;
        //Definicion del proveedor y modelo a usar
        if (provider.name == "Anthropic") chatClient = new AnthropicClient(new() { ApiKey = key }).AsIChatClient(model.name).AsBuilder().UseFunctionInvocation().Build();
        else if (provider.name == "OpenAI") chatClient = new OpenAIClient(key).GetChatClient(model.name).AsIChatClient().AsBuilder().UseFunctionInvocation().Build();
        else throw new NotSupportedException($"El proveedor {provider.name} no es soportado.");
        //Tools para memoria y resumen
        AIFunction CapturingImportantInformation = AIFunctionFactory.Create(
        ([Description("ImportantMessage")] string? message) =>
        {
            if (message != null) chatClientRepository.Create(new Memory(message, false));
            return "Datos personales registrados correctamente.";
        },
        "CapturingImportantInformation",
        importantDescription);
        ChatOptions.Tools.Add(CapturingImportantInformation);
    }
    public async Task<string> SendMessageAsync(string message)
    {
        Messages.Add(new(ChatRole.User, message));
        var response = await chatClient.GetResponseAsync(Messages, ChatOptions);
        return response.Text;
    }
}