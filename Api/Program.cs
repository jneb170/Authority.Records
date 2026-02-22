using Api.Middleware;
using Shared.Infrastructure.Outbox;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//Note: if later we need to scale up, move this registration to a new Worker project.
builder.Services.AddHostedService<OutboxProcessor>();


var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();
app.UseMiddleware<TenantMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();


app.Run();

