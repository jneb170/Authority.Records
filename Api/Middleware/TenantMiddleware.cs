namespace Api.Middleware
{
    public class TenantMiddleware
    {
        private readonly RequestDelegate _next;

        public TenantMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Headers.TryGetValue("X-Tenant-ID", out var tenantId))
            {
                context.Items["TenantId"] = tenantId.ToString();
            }
            else
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Tenant ID missing");
                return;
            }

            await _next(context);
        }
    }

}
