using AgentTest.ChatClient.Domain.MemoryAggregate.Application.Interface;
using AgentTest.ChatClient.Domain.MemoryAggregate.Domain;
using Anthropic;
using Microsoft.Extensions.AI;
using OpenAI;
using System.ComponentModel;
using static System.Net.Mime.MediaTypeNames;
namespace AgentTest.ChatClient;

public class ChatClient
{
    public List<ChatMessage> Messages { get; private set; } = [];
    //[
    //    new (ChatRole.System,
    //        "IMPORTANTE: Solo debes llamar a CapturingImportantInformation para mensajes del usuario " +
    //        "que comiencen con el prefijo [NEW]. Ignora completamente cualquier mensaje sin ese prefijo.")
    //];
    public ChatOptions ChatOptions { get; private set; } = new ChatOptions() { Tools = [] };
    private readonly IMemoryRepository chatClientRepository;
    private readonly IChatClient chatClient;
    private readonly int maxMessages;
    private readonly int maxNonResumenes;
    public ChatClient(IMemoryRepository repository, ProviderType provider, ModelType model, string key, string importantDescription, int maxMessages = 12, int maxNonResumenes = 6)
    {
        chatClientRepository = repository;
        this.maxMessages = maxMessages;
        this.maxNonResumenes = maxNonResumenes;
        //Definicion del proveedor y modelo a usar
        if (provider.name == "Anthropic") chatClient = new AnthropicClient(new() { ApiKey = key }).AsIChatClient(model.name).AsBuilder().UseFunctionInvocation().Build();
        else if (provider.name == "OpenAI") chatClient = new OpenAIClient(key).GetChatClient(model.name).AsIChatClient().AsBuilder().UseFunctionInvocation().Build();
        else throw new NotSupportedException($"El proveedor {provider.name} no es soportado.");
        //Tools para memoria y resumen
        AIFunction CapturingImportantInformation = AIFunctionFactory.Create(
        ([Description("Mensaje que uso el modelo para evaluar si cumple los criterios")] string? message
        , [Description("Indicame por llamaste a la funcion")] string? explicatio
        , [Description("Es importante?")] bool? isImportant
        ) =>
        {
            Console.WriteLine($"[x] Importante: {isImportant} - {explicatio}");
            if (message != null && isImportant == true) chatClientRepository.Create(new Memory(message.Replace("[New]", ""), false));
            return "Datos personales registrados correctamente.";
        },
        "CapturingImportantInformation",
        importantDescription);
        ChatOptions.Tools.Add(CapturingImportantInformation);
    }
    public async Task<string> SendMessageAsync(string message)
    {
        //Obtener contexto actual
        var importaMessages = chatClientRepository.GetResponse(maxMessages);
        //Resumir si hay mas de maxNonResumenes mensajes sin resumir
        var nonResumenes = importaMessages.Where(m => !m.IsResumen).ToList();
        if (nonResumenes.Count >= maxNonResumenes)
        {
            Console.WriteLine("[x] Resumiendo memoria...");
            var contenido = string.Join("\n", nonResumenes.Select(m => m.Message));
            List<ChatMessage> summaryMessages =
            [
                new(ChatRole.User, $"Resume los siguientes mensajes en un párrafo conciso conservando solo los datos más relevantes:\n{contenido}")
            ];
            var summaryResponse = await chatClient.GetResponseAsync(summaryMessages, new ChatOptions());
            chatClientRepository.Create(new Memory(summaryResponse.Text, true));
            chatClientRepository.Delete([.. nonResumenes.Select(m => m.MemoryID)]);
            //Refrescar contexto tras resumir
            importaMessages = chatClientRepository.GetResponse(maxMessages);
        }
        //Construir contexto y enviar mensaje
        Messages =
        [
            new(ChatRole.System,
                "Responde siempre de forma concisa y directa, sin explicar tu razonamiento ni mencionar las herramientas que usas internamente. " +
                "Si el mensaje es una pregunta, saludo o no declara datos personales explícitamente, NO llames a ninguna función.")
        ];
        //Messages = [];
        importaMessages.ForEach(x => Messages.Add(new(ChatRole.Tool, x.Message)));
        Messages.Add(new(ChatRole.User, $"[New] - {message}"));
        var response = await chatClient.GetResponseAsync(Messages, ChatOptions);
        return response.Text;
    }
}
