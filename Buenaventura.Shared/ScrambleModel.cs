namespace Buenaventura.Shared;

public class ScrambleModel
{
    public DateTime DeleteBeforeDate { get; set; }
    public IEnumerable<CategoryModel> Categories { get; set; } = [];
}