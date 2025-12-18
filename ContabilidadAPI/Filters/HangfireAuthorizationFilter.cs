using Hangfire.Dashboard;

namespace ContabilidadAPI.Filters
{
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            // En desarrollo, permitir acceso sin autenticación
            // En producción, puedes agregar validación JWT u otra autenticación
            var httpContext = context.GetHttpContext();
            
            // Por ahora permitimos acceso local
            // TODO: Implementar autenticación adecuada en producción
            return httpContext.Request.Host.Host == "localhost" 
                || httpContext.Request.Host.Host == "127.0.0.1"
                || httpContext.Request.Host.Host.StartsWith("192.168.");
        }
    }
}
