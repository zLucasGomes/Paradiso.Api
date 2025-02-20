namespace Paradiso.API.Domain.Entities;

public class LogVisualization
{
    public Guid Id { get; set; }
    public DateTime LogTime { get; set; }


    public Guid UserId { get; set; }
    public Guid ViewerId { get; set; }

    public virtual User User { get; set; }
    public virtual User Viewer { get; set; }
}
