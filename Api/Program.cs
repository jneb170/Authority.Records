using Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.



var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();
app.UseMiddleware<TenantMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();


app.Run();

