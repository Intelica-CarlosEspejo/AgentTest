using AgentTest.Agent.Domain.Common.EFCore;
using AgentTest.ChatClient;
using AgentTest.ChatClient.Domain.MemoryAggregate.Application.Interface;
using AgentTest.ChatClient.Domain.MemoryAggregate.Infrastructure;
// using Anthropic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
//using OpenAI;
//using System.ComponentModel;
string? environment;
//Configuracion de inyeccion de dependencias
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
var service = DependencyInyectionConfiguration();
// Definir la herramienta para capturar datos personales
// AIFunction capturarDatosPersonales = AIFunctionFactory.Create(
//     ([Description("Nombre del usuario")] string? nombre,
//      [Description("Edad del usuario en años")] int? edad,
//      [Description("Sexo del usuario: masculino o femenino")] string? sexo) =>
//     {
//         Console.ForegroundColor = ConsoleColor.Cyan;
//         Console.WriteLine("\n>>> [TOOL: CapturarDatosPersonales]");
//         if (nombre != null) Console.WriteLine($"    Nombre : {nombre}");
//         if (edad   != null) Console.WriteLine($"    Edad   : {edad}");
//         if (sexo   != null) Console.WriteLine($"    Sexo   : {sexo}");
//         Console.ResetColor();

//         // Aquí puedes guardar en DB, llamar una API, etc.
//         return "Datos personales registrados correctamente.";
//     },
//     "CapturarDatosPersonales",
//     "Llama a esta función cuando el usuario mencione su nombre, edad o sexo para registrar sus datos personales.");

// var chatOptions = new ChatOptions
// {
//     Tools = [capturarDatosPersonales]
// };

// // Crear clientes con middleware de invocación automática de herramientas
// IChatClient claudeClient = new AnthropicClient(new() { ApiKey = anthropicApiKey })
//     .AsIChatClient("claude-sonnet-4-20250514")
//     .AsBuilder()
//     .UseFunctionInvocation()
//     .Build();

// IChatClient gptClient = new OpenAIClient(openAiApiKey)
//     .GetChatClient("gpt-4o")
//     .AsIChatClient()
//     .AsBuilder()
//     .UseFunctionInvocation()
//     .Build();

// Diccionario de proveedores
// var providers = new Dictionary<string, IChatClient>
// {
//     ["claude"] = claudeClient,
//     ["gpt"]    = gptClient
// };

// Console.WriteLine("=== Agente Multi-IA (Microsoft.Extensions.AI) ===");
// Console.WriteLine("Proveedores disponibles:");
// Console.WriteLine("  1. Claude Sonnet 4 (Anthropic)");
// Console.WriteLine("  2. GPT-4o (OpenAI)");
// Console.WriteLine("  3. Ambos (comparar respuestas)");
// Console.WriteLine();
// Console.WriteLine("Comandos: /claude, /gpt, /ambos, /salir");
// Console.WriteLine("================================================");

var currentProvider = "claude";
//Instancia agente
ChatClientFactory? chatClientFactory = service.GetService<ChatClientFactory>();
if (chatClientFactory != null)
{
    var provider = new Provider();
    var chatClient = chatClientFactory.Create(provider.Anthropic, provider.Models.Sonet4, anthropicApiKey);
    while (true)
    {
        Console.WriteLine();
        Console.Write($"[{currentProvider.ToUpper()}] > ");
        var input = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(input)) continue;
        var response = await chatClient.SendMessageAsync(input);
        Console.WriteLine($"Respuesta del agente: {response}");


        // if (input.StartsWith("/"))
        // {
        //     switch (input.ToLower())
        //     {
        //         case "/claude":
        //             currentProvider = "claude";
        //             Console.WriteLine("Cambiado a Claude Sonnet 4");
        //             continue;
        //         case "/gpt":
        //             currentProvider = "gpt";
        //             Console.WriteLine("Cambiado a GPT-4o");
        //             continue;
        //         case "/ambos":
        //             currentProvider = "ambos";
        //             Console.WriteLine("Modo comparación: ambos modelos responderán");
        //             continue;
        //         case "/salir":
        //             Console.WriteLine("¡Hasta luego!");
        //             return;
        //         default:
        //             Console.WriteLine("Comando no reconocido. Usa: /claude, /gpt, /ambos, /salir");
        //             continue;
        //     }
        // }

        // try
        // {
        //     var providersToUse = currentProvider == "ambos" ? providers.Keys.ToList() : [currentProvider];
        //     foreach (var providerName in providersToUse)
        //     {
        //         Console.WriteLine($"\n--- {GetDisplayName(providerName)} ---");
        //         var response = await SendMessageAsync(providers[providerName], input);
        //         Console.WriteLine(response);
        //     }
        // }
        // catch (Exception ex)
        // {
        //     Console.WriteLine($"Error: {ex.Message}")
    }
}

else
{
    Console.WriteLine("Error: No se pudo instanciar ChatClientFactory.");
    return;
}


// Función unificada para enviar mensajes - pasa las tools en las opciones
// async Task<string> SendMessageAsync(IChatClient client, string message)
// {
//     var response = await client.GetResponseAsync(message, chatOptions);
//     return response.Text ?? "Sin respuesta";
// }

// string GetDisplayName(string provider) => provider switch
// {
//     "claude" => "Claude Sonnet 4",
//     "gpt" => "GPT-4o",
//     _ => provider
// };
IConfiguration SetConfiguration()
{
    IConfigurationBuilder configurationBuilder;
    environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Local";
    configurationBuilder = new ConfigurationBuilder().AddJsonFile("appsettings/appsettings.json");
    return configurationBuilder.Build();
}
ServiceProvider DependencyInyectionConfiguration()
{
    IConfiguration configuration = SetConfiguration();
    IServiceCollection services = new ServiceCollection();
    services.AddDbContext<Context>(options =>
    {
        options.UseNpgsql(configuration.GetConnectionString("Security"),
            npgsqlOptionsAction: builder =>
            {
                builder.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorCodesToAdd: null);
            });
    });
    services.AddScoped<IMemoryRepository, MemoryRepository>();
    services.AddScoped<ChatClientFactory>();
    return services.BuildServiceProvider();
}