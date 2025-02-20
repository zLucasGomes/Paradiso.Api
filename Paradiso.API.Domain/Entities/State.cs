using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Paradiso.API.Domain.Entities;

public class State
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int Id { get; set; }
    public string Name { get; set; }
    public string Uf { get; set; }

    public virtual ICollection<City> Cities { get; set; }
}
