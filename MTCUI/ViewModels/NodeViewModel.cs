using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using MTCCore.Messages.Nodes;
using MTCCore.Models;

namespace MTCUI.ViewModels
{
    public partial class NodeViewModel : ViewModel
    {
        [ObservableProperty] private int _width;
        [ObservableProperty] private int _height;
        [ObservableProperty] private bool _isSelected;
        [ObservableProperty] private DataTemplate _targetTypeView;
        [ObservableProperty] private Style _targetStateView;

        // Replaced generated observable field with an explicit property to avoid MVVMTK0045 (AOT/WinRT compatibility).
        private NodeModel _node;
        public NodeModel Node
        {
            get => _node;
            set => SetProperty(ref _node, value);
        }

        public NodeViewModel()
        {
          
        }

        public void InitTemplateView()
        {
            TargetTypeView = (DataTemplate)Application.Current.Resources[Node.TargetType.ToString()];
            TargetStateView = (Style)Application.Current.Resources[Node.State.ToString()];
        }

        public void MouseDoubleClick()
        {
            var id = Node.NodeId;
            WeakReferenceMessenger.Default.Send(new NodeSendCommandMessage(id));
        }
    }
}
