using Paradiso.API.Domain.Entities;

namespace Paradiso.API.Config;

public static class Migrations
{
    public static IApplicationBuilder ConfigureMigrations(this IApplicationBuilder app)
    {
        using var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>()?.CreateScope();

        var context = serviceScope?.ServiceProvider.GetRequiredService<EFContext>();

        if (context != null)
        {
            try
            {
                var pendingMigrations = context.Database.GetPendingMigrations();

                if (pendingMigrations != null && pendingMigrations.Any())
                {
                    context.Database.Migrate();

                    context.Database.ExecuteSqlRaw(File.ReadAllText("Config/DataDefault/data.sql"));
                }
            }
            catch (Exception ex)
            {
                throw new ExceptionDto() { Message = ex.Message };
            }
        }

        return app;
    }
}
