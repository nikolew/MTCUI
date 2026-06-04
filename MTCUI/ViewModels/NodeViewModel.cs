using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml;
using MTCCore.Models;
using MTCCore.Protocol;
using MTCCore.Services.Nodes;
using System;

namespace MTCUI.ViewModels
{
    public partial class NodeViewModel : ViewModel
    {
       
        [ObservableProperty] private int _width;
        [ObservableProperty] private int _height;
        [ObservableProperty] private bool _isSelected;
        [ObservableProperty] private DataTemplate _targetTypeView;
        [ObservableProperty] private Style _targetStateView;

        [ObservableProperty] private NodeModel _node;

        private INodeService _nodeService;

        public NodeViewModel()
        {
            _nodeService = Ioc.Default.GetRequiredService<INodeService>();
        }

        public void InitTemplateView()
        {
            TargetTypeView = (DataTemplate)Application.Current.Resources[Node.TargetType.ToString()];
            TargetStateView = (Style)Application.Current.Resources[Node.State.ToString()];
        }

        public void MouseDoubleClick()
        {
            var packet = new Envelope
            {
                Seq = 1,
                TsMs = (uint)Environment.TickCount,
                SendNodeCommandReq = new SendNodeCommandReq
                {
                    NodeId = Node.NodeId,
                    NodeCommand = NodeCommand.CMD_GPIO_SET,    
                }
            };

            _nodeService.NodeCommand(packet);
        }
    }
}
