using AgentTest.Agent.Domain.Common.EFCore;
using AgentTest.Agent.Domain.MemoryAggregate.Application.DTO;
using AgentTest.ChatClient.Domain.MemoryAggregate.Application.Interface;
using AgentTest.ChatClient.Domain.MemoryAggregate.Domain;
namespace AgentTest.ChatClient.Domain.MemoryAggregate.Infrastructure;

public class MemoryRepository(Context context) : IMemoryRepository
{
    public void Create(Memory memory)
    {
        context.Memories.Add(memory);
        context.SaveChanges();
    }
    public void Delete(List<Guid> ids)
    {
        var memories = context.Memories.Where(m => ids.Contains(m.MemoryID)).ToList();
        context.Memories.RemoveRange(memories);
        context.SaveChanges();
    }
    public List<MemorySimpleResponse> GetResponse(int maxMessages)
    {
        //Todos los resumenes
        var resumenes = context.Memories.Where(m => m.IsResumen).Select(m => new MemorySimpleResponse(m.MemoryID, m.Message, true)).Take(maxMessages).ToList();
        //Calcular restantes
        var resumenCount = Math.Min(resumenes.Count, maxMessages);
        var remaining = maxMessages - resumenCount;
        //Completar con no resumenes
        var nonResumenes = context.Memories.Where(m => !m.IsResumen).Select(m => new MemorySimpleResponse(m.MemoryID, m.Message, false)).Take(remaining).ToList();
        return [.. resumenes.Take(resumenCount), .. nonResumenes];
    }
}
