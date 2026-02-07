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

        public void RemoveNode(NodeViewModel nodeViewModel)
        {
            _nodesViewModel.Remove(nodeViewModel);
        }

        public void ClearNodes()
        {
            _nodesViewModel.Clear();
        }
    }
}
