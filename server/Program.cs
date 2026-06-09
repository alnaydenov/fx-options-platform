using FxOptions.Server.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IOptionsRepository, OptionsRepository>();

var app = builder.Build();

app.MapGet("/api/options", (IOptionsRepository repo) => repo.GetAll());

app.Run();
