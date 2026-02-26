namespace AgentTest.ChatClient;

public class Provider
{
    public ProviderType Anthropic { get; private set; } = new("Anthropic");
    public ProviderType OpenAI { get; private set; } = new("OpenAI");
    public Model Models { get; private set; } = new();
    public class Model
    {
        //Anthropic
        public ModelType Sonet4 { get; private set; } = new("claude-sonnet-4-6");
        //OpenAI
        public ModelType GPT4o { get; private set; } = new("gpt-4o");
    }
}
public record ProviderType(string name);
public record ModelType(string name);