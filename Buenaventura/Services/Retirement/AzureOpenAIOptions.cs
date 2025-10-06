namespace Buenaventura.Services.Retirement;

public class AzureOpenAIOptions
{
    // Example: https://my-aoai-resource.openai.azure.com/
    public string? Endpoint { get; set; }
    // API key for the Azure OpenAI resource
    public string? ApiKey { get; set; }
    // The deployment name of your chat model (e.g., gpt-4o-mini)
    public string? DeploymentName { get; set; }
}