using AgentTest.ChatClient.Domain.MemoryAggregate.Application.Interface;
namespace AgentTest.ChatClient;

public class ChatClientFactory(IMemoryRepository memoryRepository)
{
    public ChatClient Create(ProviderType provider, ModelType model, string key)
    {
        string imnportantDescription =
            "Llama a esta función solo si cuando el mensaje del usuario comience con [New] y contenga datos personales declarativos: nombre, edad, dirección o gustos personales.";
        string imnportantDescriptionInvoicing =
            "LLama a esta funcion cuando el ultimo mensaje del usuario comience con [New]. " +            
            "LLama a esta funcion cuando el ultimo mensaje del usuario proporcione informacion sobre facturacion, facturas, boleta documentos contabes. ";
        return new ChatClient(memoryRepository, provider, model, key, imnportantDescriptionInvoicing);


    }
}
