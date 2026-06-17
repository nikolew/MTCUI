using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MTCCore.Protocol.Handlers;
using MTCCore.Data;
using MTCCore.Services.Common;
using MTCCore.Services.Communication;
using MTCCore.Services.Groups;
using MTCCore.Services.Nodes;
using MTCCore.Services.Scheduling;
using MTCCore.Protocol.Events;

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



            services.AddSingleton<IEnvelopeHandler, PingEnvelopeHandler>();
            services.AddSingleton<NodeListEnvelopeHandler>();
            services.AddSingleton<NodeDataEnvelopeHandler>();
            services.AddSingleton<ConfigAckEnvelopeHandler>();
            services.AddSingleton<NodeConfigEnvelopeHandler>();
            services.AddSingleton<NodeStatusEnvelopeHandler>();

            services.AddSingleton<IEnvelopeHandler>(sp => sp.GetRequiredService<NodeListEnvelopeHandler>());
            services.AddSingleton<IEnvelopeHandler>(sp => sp.GetRequiredService<NodeDataEnvelopeHandler>());
            services.AddSingleton<IEnvelopeHandler>(sp => sp.GetRequiredService<ConfigAckEnvelopeHandler>());
            services.AddSingleton<IEnvelopeHandler>(sp => sp.GetRequiredService<NodeConfigEnvelopeHandler>());
            services.AddSingleton<IEnvelopeHandler>(sp => sp.GetRequiredService<NodeStatusEnvelopeHandler>());
            return services;
        }
    }
}
