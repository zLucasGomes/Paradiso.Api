namespace Paradiso.API.Infra.Mapping;

public class LogVizualizationMap : IEntityTypeConfiguration<LogVisualization>
{
    public void Configure(EntityTypeBuilder<LogVisualization> builder)
    {
        builder.ToTable("LogVisualization")
            .HasKey(e => e.Id);

        builder.Property(x => x.LogTime).HasColumnType("datetime");

        builder.HasOne(e => e.User)
            .WithMany(e => e.LogVisualizationsUser)
            .HasForeignKey(e => e.UserId);

        builder.HasOne(e => e.Viewer)
            .WithMany(e => e.LogVisualizationsViewer)
            .HasForeignKey(e => e.ViewerId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
