namespace AgentTest.Agent.Domain.MemoryAggregate.Domain;
public class Memory(string message, bool isResumen)
{
    public Guid MemoryID { get; private set; } = Guid.NewGuid();
    public string Message { get; private set; } = message;
    public bool IsResumen { get; private set; } = isResumen;
}