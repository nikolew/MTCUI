using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MTCCore.Protocol.Handlers;
using MTCCore.Data;
using MTCCore.Services.Common;
using MTCCore.Services.Communication;
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
            services.AddSingleton<Clock>();
            services.AddSingleton<IGroupService, GroupService>();
            services.AddSingleton<ISchedulingService, SchedulingService>();
            services.AddSingleton<IBluetoothService, BluetoothService>();
            services.AddSingleton<IBluetoothProtocolService, BluetoothProtocolService>();

            services.AddSingleton<NodeListHandler>();
            services.AddSingleton<NodeStatusHandler>();
            services.AddSingleton<NodeReadConfigHandler>();
            services.AddSingleton<NodeEventHandler>();  

            services.AddSingleton<IPacketHandler>(sp=>sp.GetRequiredService<NodeStatusHandler>());
            services.AddSingleton<IPacketHandler>(sp=>sp.GetRequiredService<NodeListHandler>());
            services.AddSingleton<IPacketHandler>(sp=>sp.GetRequiredService<NodeReadConfigHandler>());
            services.AddSingleton<IPacketHandler>(sp=>sp.GetRequiredService<NodeEventHandler>());

            services.AddSingleton<IPacketHandler, PingHandler>();
            return services;
        }
    }
}
