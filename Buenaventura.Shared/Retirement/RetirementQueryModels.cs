namespace Buenaventura.Shared.Retirement;

public class RetirementQueryRequest
{
    public string Question { get; set; } = string.Empty;
}

public class RetirementQueryResponse
{
    public string Answer { get; set; } = string.Empty;
    public string? Reasoning { get; set; }
}
