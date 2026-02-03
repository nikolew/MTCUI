using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MTCUI.ViewModels
{
    public partial class GraphViewModel : ObservableObject
    {
        public IEnumerable<NodeViewModel> NodesViewModel => _nodesViewModel;
        private readonly ObservableCollection<NodeViewModel> _nodesViewModel;

        public GraphViewModel()
        {
            _nodesViewModel = [];       
        }

        public void AddNode(NodeViewModel nodeViewModel)
        {
            _nodesViewModel.Add(nodeViewModel);
        }
    }
}
