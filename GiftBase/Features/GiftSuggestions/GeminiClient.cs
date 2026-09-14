using System.Text.Json.Nodes;
using Google.GenAI;
using Google.GenAI.Types;
using GiftBase.Core.Exceptions;
using GiftBase.Core.Interfaces;

namespace GiftBase.Features.GiftSuggestions;

public class GeminiClient(Client client, IConfiguration configuration) : IGeminiClient
{
    private const string ErrorMessage = "Die KI-Vorschläge konnten nicht abgerufen werden. Bitte versuche es später erneut.";

    public async Task<string> GenerateJsonAsync(string systemInstruction, string input, string responseSchema, CancellationToken cancellationToken)
    {
        var model = configuration["Gemini:Model"]
            ?? throw new InvalidOperationException("Gemini:Model ist nicht konfiguriert.");

        var config = new GenerateContentConfig
        {
            SystemInstruction = new Content { Parts = [Part.FromText(systemInstruction)] },
            ResponseMimeType = "application/json",
            ResponseJsonSchema = JsonNode.Parse(responseSchema),
            Temperature = 1.0
        };

        GenerateContentResponse response;

        try
        {
            response = await client.Models.GenerateContentAsync(model, input, config, cancellationToken);
        }
        catch (HttpRequestException)
        {
            throw new ExternalServiceException(ErrorMessage);
        }
        catch (TaskCanceledException)
        {
            throw new ExternalServiceException(ErrorMessage);
        }

        return response.Text
            ?? throw new ExternalServiceException(ErrorMessage);
    }
}
