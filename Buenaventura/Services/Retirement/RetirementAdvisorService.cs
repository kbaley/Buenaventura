using Azure;
using Azure.AI.OpenAI;
using Buenaventura.Shared.Retirement;
using Microsoft.Extensions.Options;
using OpenAI;

namespace Buenaventura.Services.Retirement;

public interface IRetirementAdvisorService
{
    Task<RetirementQueryResponse> AskAsync(RetirementQueryRequest request, CancellationToken ct = default);
}

public class RetirementAdvisorService(IOptions<AzureOpenAIOptions> options, ILogger<RetirementAdvisorService> logger)
    : IRetirementAdvisorService
{
    private readonly AzureOpenAIOptions _options = options.Value;

    public async Task<RetirementQueryResponse> AskAsync(RetirementQueryRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_options.Endpoint) || string.IsNullOrWhiteSpace(_options.ApiKey) || string.IsNullOrWhiteSpace(_options.DeploymentName))
        {
            logger.LogWarning("Azure OpenAI configuration is missing. Endpoint, ApiKey, and DeploymentName are required.");
            return new RetirementQueryResponse
            {
                Answer = "Azure OpenAI is not configured on the server. Please set AzureOpenAI:Endpoint, AzureOpenAI:ApiKey, and AzureOpenAI:DeploymentName.",
                Reasoning = null
            };
        }

        if (string.IsNullOrWhiteSpace(request.Question))
        {
            return new RetirementQueryResponse { Answer = "Please enter a question about your retirement scenario." };
        }

        try
        {
            var endpoint = new Uri(_options.Endpoint!);
            var client = new OpenAIClient(endpoint, new AzureKeyCredential(_options.ApiKey!));

            var sysPrompt = "You are a helpful personal finance assistant specializing in retirement planning. " +
                            "Provide clear, practical, and cautious guidance. If you need assumptions, state them explicitly. " +
                            "You are not a financial advisor; include a brief disclaimer. Prefer concise bullet points.";

            var messages = new List<ChatRequestMessage>
            {
                new ChatRequestSystemMessage(sysPrompt),
                new ChatRequestUserMessage(request.Question)
            };

            var chatOptions = new ChatCompletionsOptions
            {
                Temperature = 0.3f,
                MaxTokens = 700,
            };
            foreach (var m in messages) chatOptions.Messages.Add(m);

            var response = await client.GetChatCompletionsAsync(_options.DeploymentName!, chatOptions, ct);
            var content = response.Value.Choices.FirstOrDefault()?.Message.Content ?? "";

            return new RetirementQueryResponse
            {
                Answer = string.IsNullOrWhiteSpace(content) ? "I couldn't generate an answer at this time. Please try again." : content,
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error calling Azure OpenAI");
            return new RetirementQueryResponse
            {
                Answer = "There was an error generating a response. Please try again later.",
                Reasoning = ex.Message
            };
        }
    }
}
