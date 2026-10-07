using DataTicket.Api.Health;
using DataTicket.Application;
using DataTicket.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Composition root: el adaptador primario conecta los casos de uso con los adaptadores secundarios.
builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapHealthEndpoints(includeDetails: app.Environment.IsDevelopment());

app.Run();
