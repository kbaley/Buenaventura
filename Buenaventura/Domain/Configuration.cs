using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Buenaventura.Domain;

[Table("configuration")]
public class Configuration
{
    [Key] public Guid ConfigurationId { get; set; }
    public string Name { get; set; }
    public string Value { get; set; }
}