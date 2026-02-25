using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using MTCCore.Messages.Nodes;
using MTCCore.Models;
using MTCCore.Protocol;

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

        public void InitTemplateView()
        {
            TargetTypeView = (DataTemplate)Application.Current.Resources[Node.TargetType.ToString()];
            TargetStateView = (Style)Application.Current.Resources[Node.State.ToString()];
        }

        public void MouseDoubleClick()
        {
            WeakReferenceMessenger.Default.Send(new NodeSendCommandMessage(Node.NodeId, CommandType.CMD_NODECMD));
        }
    }
}
