namespace Buenaventura.Shared;

public class CategoryDto
{
    public Guid CategoryId { get; set; }

    public string Name { get; set; } = "";

    public string Type { get; set; } = "";

    public Guid? ParentCategoryId { get; set; }
}