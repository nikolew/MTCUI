using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MTCCore.Data;
using MTCCore.Repositories;
using MTCCore.Services.Common;
using MTCCore.Services.Groups;
using MTCCore.Services.Nodes;
using MTCCore.Services.Scheduling;

namespace MTCCore.Infrastructure
{
    public static class ServiceRegistration
    {
        public static IServiceCollection RegisterService(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<ApplicationDbContext>();
            services.AddSingleton<INodeService, NodeService>();
            services.AddSingleton<ITimeRepository, TimeRepository>();
            services.AddSingleton<IGroupRepository, GroupRepository>();
            services.AddSingleton<Clock>();
            services.AddSingleton<IGroupService, GroupService>();
            services.AddSingleton<ISchedulingService, SchedulingService>();
            return services;
        }
    }
}
