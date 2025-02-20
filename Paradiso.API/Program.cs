var builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureModule();

builder.Services.ConfigureServices(builder);

builder.Services.AddEndpointsApiExplorer();

builder.Services.ConfigureApiDocumentation();

var app = builder.Build();

app.ConfigureMigrations();

app.ConfigureApiDocumentarionUi();

app.ConfigureMiddlewares();

app.MapControllers();

app.MapHealthChecks(builder.Configuration.GetSection("EndPointsConfig")["APIHealthCheckUrl"]!);

app.Run();
