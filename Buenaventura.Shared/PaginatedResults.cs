namespace Buenaventura.Shared;

public class PaginatedResults<T>
{

    public IEnumerable<T> Items { get; set; } = [];
    public int TotalCount { get; set; }
}