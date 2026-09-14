namespace GiftBase.Core.Interfaces;

public interface IGeminiClient
{
    Task<string> GenerateJsonAsync(string systemInstruction, string input, string responseSchema, CancellationToken cancellationToken);
}
