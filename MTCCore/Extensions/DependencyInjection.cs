using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MTCCore.DataBase;
using MTCCore.Repositories;
using MTCCore.Services;


namespace MTCCore.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDependencyInjection(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<ApplicationDbContext>();
            services.AddSingleton<INodeService, NodeService>();
            services.AddSingleton<INodeRepository, NodeRepository>();
            return services;
        }
    }
}
