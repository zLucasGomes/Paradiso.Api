using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Paradiso.API.Domain.Entities;

public class City
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int Id { get; set; }
    public string Name { get; set; }


    public int StateId { get; set; }


    public virtual State State { get; set; }


    public virtual ICollection<User> Users { get; set; }
}
