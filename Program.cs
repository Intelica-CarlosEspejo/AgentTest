using Microsoft.Extensions.AI;
using Anthropic;
using OpenAI;

// Configurar las API keys desde variables de entorno
var anthropicApiKey = Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY");
var openAiApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");

if (string.IsNullOrEmpty(anthropicApiKey) || string.IsNullOrEmpty(openAiApiKey))
{
    Console.WriteLine("Error: Debes configurar las variables de entorno:");
    Console.WriteLine("  - ANTHROPIC_API_KEY");
    Console.WriteLine("  - OPENAI_API_KEY");
    return;
}

// Crear los clientes usando IChatClient (interfaz unificada de Microsoft.Extensions.AI)
IChatClient claudeClient = new AnthropicClient(new() { ApiKey = anthropicApiKey })
    .AsIChatClient("claude-sonnet-4-20250514");

IChatClient gptClient = new OpenAIClient(openAiApiKey)
    .GetChatClient("gpt-4o")
    .AsIChatClient();

// Diccionario de proveedores
var providers = new Dictionary<string, IChatClient>
{
    ["claude"] = claudeClient,
    ["gpt"] = gptClient
};

Console.WriteLine("=== Agente Multi-IA (Microsoft.Extensions.AI) ===");
Console.WriteLine("Proveedores disponibles:");
Console.WriteLine("  1. Claude Sonnet 4 (Anthropic)");
Console.WriteLine("  2. GPT-4o (OpenAI)");
Console.WriteLine("  3. Ambos (comparar respuestas)");
Console.WriteLine();
Console.WriteLine("Comandos: /claude, /gpt, /ambos, /salir");
Console.WriteLine("================================================");

var currentProvider = "claude";

while (true)
{
    Console.WriteLine();
    Console.Write($"[{currentProvider.ToUpper()}] > ");
    var input = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(input))
        continue;

    if (input.StartsWith("/"))
    {
        switch (input.ToLower())
        {
            case "/claude":
                currentProvider = "claude";
                Console.WriteLine("Cambiado a Claude Sonnet 4");
                continue;
            case "/gpt":
                currentProvider = "gpt";
                Console.WriteLine("Cambiado a GPT-4o");
                continue;
            case "/ambos":
                currentProvider = "ambos";
                Console.WriteLine("Modo comparación: ambos modelos responderán");
                continue;
            case "/salir":
                Console.WriteLine("¡Hasta luego!");
                return;
            default:
                Console.WriteLine("Comando no reconocido. Usa: /claude, /gpt, /ambos, /salir");
                continue;
        }
    }

    try
    {
        var providersToUse = currentProvider == "ambos"
            ? providers.Keys.ToList()
            : [currentProvider];

        foreach (var providerName in providersToUse)
        {
            Console.WriteLine($"\n--- {GetDisplayName(providerName)} ---");
            var response = await SendMessageAsync(providers[providerName], input);
            Console.WriteLine(response);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }
}

// Función unificada para enviar mensajes - misma interfaz para todos los proveedores
async Task<string> SendMessageAsync(IChatClient client, string message)
{
    var response = await client.GetResponseAsync(message);
    return response.Text ?? "Sin respuesta";
}

string GetDisplayName(string provider) => provider switch
{
    "claude" => "Claude Sonnet 4",
    "gpt" => "GPT-4o",
    _ => provider
};
