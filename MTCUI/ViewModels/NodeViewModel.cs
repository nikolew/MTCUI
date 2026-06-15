using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using MTCCore.Models;
using MTCCore.Protocol;
using MTCCore.Services.Groups;
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
        [ObservableProperty] private SolidColorBrush _groupColor;

        [ObservableProperty] private NodeModel _node;

        private INodeService _nodeService;
        private IGroupService _groupService;

        public NodeViewModel()
        {
            _nodeService = Ioc.Default.GetRequiredService<INodeService>();
            _groupService = Ioc.Default.GetRequiredService<IGroupService>();
        }

        public void InitTemplateView()
        {
            var color = _groupService.GetColorGrupByName(Node.GroupName).Result;

            TargetTypeView = (DataTemplate)Application.Current.Resources[Node.TargetType.ToString()];
            TargetStateView = (Style)Application.Current.Resources[Node.State.ToString()];
            GroupColor = new SolidColorBrush(color);
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
